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
        private string _login = "";
        private string _password = "";
        private bool _isSaveMe = false;

        private readonly MainViewModel _mainViewModel;
        private Action _libraryInstancesLoading;
        private RelayCommand _signUpCommand;

        private AccountType _accountType;

        #region props

        public string Login 
        {
            get => _login; set 
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        public string Password 
        {
            get => _password; set 
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

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
                    if (AccountTypeSelectedIndex == 0)
                    {
                        _accountType = AccountType.NightWorld;
                    }
                    else if (AccountTypeSelectedIndex == 1)
                    {
                        _accountType = AccountType.Mojang;
                    }
                    else if (AccountTypeSelectedIndex == 2) 
                    {
                        _accountType = AccountType.NoAuth;
                    }

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
                                Console.WriteLine("Неверный логин или пароль");
                                break;
                            case AuthCode.NoConnect:
                                Console.WriteLine("Нет соединения с сервером!");
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
            }
        }

        #endregion

        public ICommand NavigationCommand { get; }

        public AuthViewModel(MainViewModel model, Action libraryInstancesLoading)
        {
            _mainViewModel = model;
            _libraryInstancesLoading = libraryInstancesLoading;

            AccountType type = DataFilesManager.GetAccount(out _login, out _password, null);
            if (_login != null && _password != null)
            {
                IsSaveMe = true;
                Login = _login; Password = _password;
                _accountType = type;
            }
                
            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);
        }
    }
}
