using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using System;
using System.Windows.Input;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Tools;

namespace Lexplosion.Gui.ViewModels
{
    public class AuthViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private AccountType _accountType;
        private bool _isSavedAccountOAuth2 = false;


        #region Properties

        #region Main Auth

        private string _login = String.Empty;
        /// <summary>
        /// Содержит логин пользователя.
        /// </summary>
        public string Login
        {
            get => _login; set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        private string _password = String.Empty;
        /// <summary>
        /// Cодержит пароль пользователя.
        /// </summary>
        public string Password
        {
            get => _password; set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private bool _isSaveMe = false;
        /// <summary>
        /// Хранит ответ на вопрос, хочет ли пользователь сохранить аккаунт(данные).
        /// </summary>
        public bool IsSaveMe
        {
            get => _isSaveMe; set
            {
                _isSaveMe = value;
                OnPropertyChanged(nameof(IsSaveMe));
            }
        }

        private int _accountTypeSelectedIndex;
        /// <summary>
        /// Хранит индекс выбранного типа авторизации.
        /// [0] - NoAuth
        /// [1] - NightWorld
        /// [2] - Mojang
        /// </summary>
        public int AccountTypeSelectedIndex
        {
            get => _accountTypeSelectedIndex; set
            {
                _accountTypeSelectedIndex = value;
                OnPropertyChanged();

                if (_accountTypeSelectedIndex == 3 && !_isSavedAccountOAuth2) 
                {
                    IsAuthFinished = false;
                    LoadingBoardPlaceholder = ResourceGetter.GetString("microsoftAuthInProgress");
                    FollowToMicrosoft();
                }
                LoadSavedAccount((AccountType)_accountTypeSelectedIndex);
            }
        }

        #endregion Main Auth

        private bool _isAuthing = false;
        /// <summary>
        /// Запустил ли пользователь метод авторизации.
        /// </summary>
        public bool IsAuthing
        {
            get => _isAuthing; set
            {
                _isAuthing = value;
                OnPropertyChanged();
            }
        }

        private bool _isAuthFinished = true;
        public bool IsAuthFinished 
        {
            get => _isAuthFinished; set 
            {
                _isAuthFinished = value; 
                OnPropertyChanged();
            }
        }

        private string _loadingBoardPlaceholder;
        public string LoadingBoardPlaceholder 
        {
            get => _loadingBoardPlaceholder; set 
            {
                _loadingBoardPlaceholder = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Commands


        /// <summary>
        /// Команда навигации меняет viewmodel.
        /// </summary>
        public ICommand NavigationCommand { get; }

        private RelayCommand _signUpCommand;
        /// <summary>
        /// Вызывается при клике по кнопке [Войти]
        /// </summary>
        public RelayCommand SignUpCommand
        {
            get => _signUpCommand ?? (new RelayCommand(obj =>
            {
                if (!IsAuthing)
                    Authorization();
            }));
        }

        private RelayCommand _cancelMicrosoftAuthCommand;
        public RelayCommand CancelMicrosoftAuthCommand 
        {
            get => _cancelMicrosoftAuthCommand ?? (_cancelMicrosoftAuthCommand = new RelayCommand(obj => 
            {
                CancelMicrosoftAuth();
            }));
        }

        #endregion Commands


        #region Constructors


        public AuthViewModel(MainViewModel viewModel)
        {
            _mainViewModel = viewModel;

            // получаем последний выбранный аккаунт
            LoadSavedAccount(null);
            // устанавливаем тип этого аккаунта
            AccountTypeSelectedIndex = (int)_accountType;

            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => viewModel.MainMenuVM);

            CommandReceiver.MicrosoftAuthPassed += PreformAuthMicrosoft;
        }


        #endregion Constructors


        #region Methods

        /// <summary>
        /// Загружает данные сохранённого аккаунта.
        /// </summary>
        /// <param name="accountType">Тип аккаунта, если null, то возвращает последний использованный сохранённый аккаунт.</param>
        public void LoadSavedAccount(AccountType? accountType)
        {
            AccountType type = DataFilesManager.GetAccount(out _login, out _password, accountType);

            if (_login != null && _password != null)
            {
                if (type == AccountType.Microsoft) 
                    _isSavedAccountOAuth2 = true;

                IsSaveMe = true;
                Login = _login; Password = _password;
            }
            else
            {
                if (type == AccountType.Microsoft)
                    _isSavedAccountOAuth2 = false;

                Login = ""; Password = "";
            }

            _accountType = type;
        }

        /// <summary>
        /// Запускает процесс авторизации.
        /// </summary>
        private void Authorization()
        {
            Lexplosion.Run.TaskRun(() =>
            {
                // получаем выбранный тип акканута.
                _accountType = (AccountType)AccountTypeSelectedIndex;

                // получаем ответ от проверки данных.
                AuthCode authCode = UserData.Auth(Login, Password, IsSaveMe, _accountType);

                App.Current.Dispatcher.Invoke(() =>
                {
                    PerformAuthCode(authCode);
                });

                IsAuthing = false;
            });
        }


        private void PreformAuthMicrosoft(string microsoftData) 
        {
            // на случае нештатной ситуации.
            if (_accountType != AccountType.Microsoft)
                return;

            var token = MojangApi.GetToken(microsoftData);
            var authCode = UserData.MicrosoftAuth(token, true);
            PerformAuthCode(authCode);
            NativeMethods.ShowWindow(Run.CurrentProcess.MainWindowHandle, 1);
            NativeMethods.SetForegroundWindow(Run.CurrentProcess.MainWindowHandle);
        }

        private void PerformAuthCode(AuthCode authCode) 
        {
            // обрабатываем полученный код.
            switch (authCode)
            {
                case AuthCode.Successfully:
                    {
                        CommandReceiver.MicrosoftAuthPassed -= PreformAuthMicrosoft;

                        _mainViewModel.UserProfile.Nickname = UserData.User.Login;
                        _mainViewModel.UserProfile.IsAuthorized = true;

                        NavigationCommand.Execute(null);
                        
                        _mainViewModel.SubscribeToOpenModpackEvent();
                        
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.DataError:
                    MainViewModel.ShowToastMessage("Ошибка авторизации", "Неверный логин или пароль", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    break;
                case AuthCode.NoConnect:
                    MainViewModel.ShowToastMessage("Ошибка авторизации", "Нет соединения с сервером!", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    break;
                case AuthCode.TokenError:
                    MainViewModel.ShowToastMessage("Ошибка авторизации", "Нет соединения с сервером!", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    FollowToMicrosoft();
                    break;
                default:
                    MainViewModel.ShowToastMessage("Ошибка что-то не так", authCode.ToString(), TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    break;
            }
        }


        private void FollowToMicrosoft() 
        {
            System.Diagnostics.Process.Start("https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED");
        }

        private void CancelMicrosoftAuth() 
        {
            AccountTypeSelectedIndex = 1;
            IsAuthFinished = true;
        }
        #endregion Methods
    }
}
