using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management;
using System;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels
{
    public class AuthViewModel : VMBase
    {
        private string _login = "";
        private string _password = "";
        private bool _isSaveMe = false;

        private MainViewModel _model;
        private Action _libraryInstancesLoading;
        private RelayCommand _signUpCommand;

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

        public RelayCommand SingUpCommand
        {
            get => _signUpCommand ?? (new RelayCommand(obj =>
            {
                Lexplosion.Run.TaskRun(() => 
                {
                    AuthCode authCode = ManageLogic.Auth(Login, Password, IsSaveMe);
                    App.Current.Dispatcher.Invoke(() => 
                    {
                        switch (authCode) 
                        {
                            case AuthCode.Successfully:
                                _model.Nickname = UserData.Login;
                                _model.IsAuthorized = true;
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

        #endregion

        public ICommand NavigationCommand { get; }

        public AuthViewModel(MainViewModel model, Action libraryInstancesLoading)
        {
            _model = model;
            _libraryInstancesLoading = libraryInstancesLoading;

            DataFilesManager.GetAccount(out _login, out _password);
            if (_login != null && _password != null)
                IsSaveMe = true;
            Login = _login; Password = _password;

            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => MainViewModel.MainMenuVM);
        }
    }
}
