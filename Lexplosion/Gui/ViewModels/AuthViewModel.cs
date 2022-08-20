using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.FileSystem;
using System;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels
{
    public class AuthViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private AccountType _accountType;


        #region props


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
                LoadSavedAccount((AccountType)_accountTypeSelectedIndex);
            }
        }

        #endregion


        #region commands

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
                Authorization();
            }));
        }

        #endregion commands


        #region constructors

        public AuthViewModel(MainViewModel model)
        {
            _mainViewModel = model;

            // получаем последний выбранный аккаунт
            LoadSavedAccount(null);
            // устанавливаем тип этого аккаунта
            _accountTypeSelectedIndex = (int)_accountType;

            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);
        }

        #endregion constructors


        #region methods

        /// <summary>
        /// Загружает данные сохранённого аккаунта.
        /// </summary>
        /// <param name="accountType">Тип аккаунта, если null, то возвращает последний использованный сохранённый аккаунт.</param>
        public void LoadSavedAccount(AccountType? accountType) 
        {
            AccountType type = DataFilesManager.GetAccount(out _login, out _password, accountType);

            if (_login != null && _password != null)
            {
                IsSaveMe = true;
                Login = _login; Password = _password;
            }
            else
            {
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
                Console.WriteLine(_accountType);

                // получаем ответ от проверки данных.
                AuthCode authCode = UserData.Auth(Login, Password, IsSaveMe, _accountType);

                App.Current.Dispatcher.Invoke(() =>
                {
                    // обрабатываем полученный код.
                    switch (authCode)
                    {
                        case AuthCode.Successfully:
                            _mainViewModel.UserProfile.Nickname = UserData.User.Login;
                            _mainViewModel.UserProfile.IsAuthorized = true;
                            NavigationCommand.Execute(null);
                            break;
                        case AuthCode.DataError:
                            MainViewModel.ShowToastMessage("Ошибка авторизации", "Неверный логин или пароль", Controls.ToastMessageState.Error);
                            break;
                        case AuthCode.NoConnect:
                            MainViewModel.ShowToastMessage("Ошибка авторизации", "Нет соединения с сервером!", Controls.ToastMessageState.Error);
                            break;
                    }
                });
            });
        }

        #endregion methods
    }
}
