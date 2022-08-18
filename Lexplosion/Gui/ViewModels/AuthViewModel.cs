using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels
{
    public class AuthViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private Action _libraryInstancesLoading;
        private RelayCommand _signUpCommand;

        private AccountType _accountType;

        #region props

        private string _login = String.Empty;
        public string Login 
        {
            get => _login; set 
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        //
        private string _password = String.Empty;
        public string Password 
        {
            get => _password; set 
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        /// <summary>
        /// Хранит ответ на вопрос, хочет ли пользователь сохранить аккаунт(данные).
        /// </summary>
        private bool _isSaveMe = false;
        public bool IsSaveMe 
        {
            get => _isSaveMe; set 
            {
                _isSaveMe = value;
                OnPropertyChanged(nameof(IsSaveMe));
            }
        }

        public RelayCommand SignUpCommand
        {
            get => _signUpCommand ?? (new RelayCommand(obj =>
            {
                Lexplosion.Run.TaskRun(() => 
                {
                    _accountType = (AccountType)AccountTypeSelectedIndex;
                    Console.WriteLine(_accountType);
                    AuthCode authCode = UserData.Auth(Login, Password, IsSaveMe, _accountType);
                    App.Current.Dispatcher.Invoke(() => 
                    {
                        switch (authCode) 
                        {
                            case AuthCode.Successfully:
                                _mainViewModel.UserProfile.Nickname = UserData.User.Login;
                                _mainViewModel.UserProfile.IsAuthorized = true;
                                _libraryInstancesLoading();
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
            }));
        }

        private int _accountTypeSelectedIndex;
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

        public ICommand NavigationCommand { get; }

        public AuthViewModel(MainViewModel model, Action libraryInstancesLoading)
        {
            _mainViewModel = model;
            _libraryInstancesLoading = libraryInstancesLoading;

            // получаем последний выбранный аккаунт
            LoadSavedAccount(null);
            // устанавливаем тип этого аккаунта
            _accountTypeSelectedIndex = (int)_accountType;

            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);
        }

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

        #endregion methods
    }
}
