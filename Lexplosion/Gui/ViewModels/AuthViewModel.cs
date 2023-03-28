using Lexplosion.Global;
using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.MainMenu;
using System.Windows.Input;
using Lexplosion.Gui.Models;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class AuthViewModel : VMBase
    {
        #region Properties


        private readonly MainViewModel _mainViewModel;

        public AuthModel Model { get; }

        private bool _isManualInput;
        public bool IsManualInput 
        { 
            get => _isManualInput; set
            { 
                _isManualInput = value; 
                OnPropertyChanged(); 
            } 
        }


        #endregion Properties


        #region Commands


        /// <summary>
        /// Команда навигации меняет viewmodel.
        /// </summary>
        public readonly ICommand NavigationCommand;

        private RelayCommand _signUpCommand;
        /// <summary>
        /// Вызывается при клике по кнопке [Войти]
        /// </summary>
        public RelayCommand SignUpCommand
        {
            get => _signUpCommand ?? (new RelayCommand(obj =>
            {
                Model.SignIn();
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
                Model.FollowToMicrosoft();
            }));
        }

        private RelayCommand _cancelMicrosoftAuthCommand;
        public RelayCommand CancelMicrosoftAuthCommand
        {
            get => _cancelMicrosoftAuthCommand ?? (_cancelMicrosoftAuthCommand = new RelayCommand(obj =>
            {
                Model.CancelMicrosoftAuth();
                IsManualInput = false;
            }));
        }

        private RelayCommand _manualInputMicrosoftDataFormOpenCommand;
        public RelayCommand ManualInputMicrosoftDataFormOpenCommand
        {
            get => _manualInputMicrosoftDataFormOpenCommand ?? (_manualInputMicrosoftDataFormOpenCommand = new RelayCommand(obj => 
            {
                IsManualInput = true;
            }));
        }

        private RelayCommand _manualInputMicrosoftDataFormCloseCommand;
        public RelayCommand ManualInputMicrosoftDataFormCloseCommand
        {
            get => _manualInputMicrosoftDataFormCloseCommand ?? (_manualInputMicrosoftDataFormCloseCommand = new RelayCommand(obj =>
            {
                IsManualInput = false;
            }));
        }

        private RelayCommand _manualInputMicrosoftDataCommand;
        public RelayCommand ManualInputMicrosoftDataCommand
        {
            get => _manualInputMicrosoftDataCommand ?? (_manualInputMicrosoftDataCommand = new RelayCommand(obj =>
            {
                Model.MicrosoftManualInput();
                IsManualInput = false;
            }));
        }


        #endregion Commands


        #region Constructors


        public AuthViewModel(MainViewModel viewModel)
        {
            NavigationCommand = new NavigateCommand<MainMenuViewModel>(
                MainViewModel.NavigationStore, () => viewModel.MainMenuVM ?? viewModel.InitMainMenuViewModel(new MainMenuViewModel(viewModel)));
            _mainViewModel = viewModel;
            Model = new AuthModel(AuthorizationSuccessful, MainViewModel.ShowToastMessage);
        }


        #endregion Constructors


        private void AuthorizationSuccessful(string name, bool isAuth, bool isNightWorldAccount) 
        {
            _mainViewModel.UserData.Nickname = GlobalData.User.Login;
            _mainViewModel.UserData.IsAuthorized = isAuth;
            _mainViewModel.UserData.IsNightWorldAccount = isNightWorldAccount;

            NavigationCommand.Execute(null);
        }
    }
}
