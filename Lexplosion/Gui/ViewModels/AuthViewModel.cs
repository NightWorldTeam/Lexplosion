using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Network;
using System;
using System.Windows.Input;
using Lexplosion.Tools;
using Lexplosion.Logic.Management.Authentication;

namespace Lexplosion.Gui.ViewModels
{
    public class AuthViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private AccountType _accountType;
        private bool _isSavedAccountOAuth2 = false;
        private Authentication _authentication;

        #region Properties

        public bool NoAccountAuth { get => _accountType == AccountType.NoAuth; }

        private bool _isMicrosoftAccountManager = false;
        public bool IsMicrosoftAccountManager 
        {
            get => _isMicrosoftAccountManager; set 
            {
                _isMicrosoftAccountManager = value;
                OnPropertyChanged();
            }
        }


        #region Main Auth

        private string _savedLogin = string.Empty;
        private string _login = String.Empty;
        /// <summary>
        /// Содержит логин пользователя.
        /// </summary>
        public string Login
        {
            get => _login; set
            {
                _login = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private int _accountTypeSelectedIndex = 1;
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
                if (_accountTypeSelectedIndex == 3) 
                {
                    LoadSavedAccount(AccountType.Microsoft);
                    if (_isSavedAccountOAuth2)
                        IsMicrosoftAccountManager = true;
                    else FollowToMicrosoft();
                }
                LoadSavedAccount((AccountType)_accountTypeSelectedIndex);
                OnPropertyChanged(nameof(NoAccountAuth));
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
                {
                    if (_accountType == AccountType.NoAuth && !string.IsNullOrEmpty(Login))
                    {
                        Authorization();
                    }
                    else if (!string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password))
                    {
                        Authorization();
                    }
                    else 
                    {
                        MainViewModel.ShowToastMessage("Заполните логин и пароль!", "Алло! А кто будет данными заполять? :)", Controls.ToastMessageState.Error);
                    }
                }
            }));
        }

        private RelayCommand _singUpMicrosoftCommand;
        /// <summary>
        /// Редиректит на сайт авторизации майков.
        /// </summary>
        public RelayCommand SingUpMicrosoftCommand
        {
            get => _singUpMicrosoftCommand ?? (_singUpMicrosoftCommand = new RelayCommand(obj => 
            {
                FollowToMicrosoft();
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
            _authentication = new Authentication();

            _accountTypeSelectedIndex = (int)LoadSavedAccount(null);

            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => viewModel.MainMenuVM ?? viewModel.InitMainMenuViewModel(new MainMenuViewModel(viewModel)));

            CommandReceiver.MicrosoftAuthPassed += PreformAuthMicrosoft;
        }


        #endregion Constructors


        #region Methods

        /// <summary>
        /// Загружает данные сохранённого аккаунта.
        /// </summary>
        /// <param name="accountType">Тип аккаунта, если null, то возвращает последний использованный сохранённый аккаунт.</param>
        public AccountType LoadSavedAccount(AccountType? accountType)
        {
            AccountType type = _authentication.GetAccount(accountType, out _savedLogin);

            if (!string.IsNullOrEmpty(_savedLogin))
            {
                // так как логин сохранён, а при авторизации Microsoft
                // логин == Ник, мы выводим авторизацию с сохранёным аккаунтов для Microsoft
                if (type == AccountType.Microsoft)
                {
                    _isSavedAccountOAuth2 = true;
                    IsMicrosoftAccountManager = true;
                }
                else IsMicrosoftAccountManager = false;

                IsSaveMe = true;
                Login = _savedLogin; Password = "";
            }
            else
            {
                if (type == AccountType.Microsoft)
                {
                    _isSavedAccountOAuth2 = false;
                    IsMicrosoftAccountManager = false;
                }
                else IsMicrosoftAccountManager = false;

                Login = ""; Password = "";
            }

            _accountType = type;

            return type;
        }


        /// <summary>
        /// Запускает процесс авторизации.
        /// </summary>
        private void Authorization()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                // получаем выбранный тип акканута.
                _accountType = (AccountType)AccountTypeSelectedIndex;

                // получаем ответ от проверки данных.
                AuthCode authCode = _authentication.Auth(_accountType, (Login == _savedLogin) ? null : Login, Password == "" ? null : Password, IsSaveMe);

                App.Current.Dispatcher.Invoke(() =>
                {
                    PerformAuthCode(authCode);
                });

                IsAuthing = false;
            });
        }


        private void PreformAuthMicrosoft(string microsoftData, MicrosoftAuthRes reult) 
        {
            // на случае нештатной ситуации.
            if (_accountType != AccountType.Microsoft)
                return;

            var authCode = _authentication.Auth(_accountType, "", microsoftData, true);
            PerformAuthCode(authCode);
            NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
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
                        _mainViewModel.UserProfile.IsNightWorldAccount = _accountType == AccountType.NightWorld;

                        NavigationCommand.Execute(null);
                        
                        _mainViewModel.SubscribeToOpenModpackEvent();
                        
                        IsAuthFinished = true;
                        break;
                    }
                case AuthCode.DataError:
                    MainViewModel.ShowToastMessage("Ошибка авторизации!", "Неверный логин или пароль.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    break;
                case AuthCode.NoConnect:
                    MainViewModel.ShowToastMessage("Ошибка авторизации!", "Нет соединения с сервером.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    break;
                case AuthCode.TokenError:
                    MainViewModel.ShowToastMessage("Ошибка авторизации!", "Ошибка с токеном.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    FollowToMicrosoft();
                    break;
                case AuthCode.SessionExpired:
                    {
                        MainViewModel.ShowToastMessage("Ошибка авторизации!", "Ошибка сессии. Попробуйте переавторизировать.", TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                        break;
                    }
                default:
                    MainViewModel.ShowToastMessage("Ошибка. Что-то не так", authCode.ToString(), TimeSpan.FromSeconds(8), Controls.ToastMessageState.Error);
                    break;
            }
        }


        private void FollowToMicrosoft() 
        {
            IsAuthFinished = false;
            LoadingBoardPlaceholder = ResourceGetter.GetString("microsoftAuthInProgress");
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
