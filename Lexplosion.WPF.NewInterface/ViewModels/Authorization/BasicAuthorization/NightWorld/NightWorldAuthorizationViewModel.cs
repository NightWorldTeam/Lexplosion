using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.Authorization;
using Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public sealed class NightWorldAuthorizationViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;

        public IBasicAuthModel Model { get; }


        #region Commands


        private RelayCommand authorizationCommand;
        public ICommand AuthorizationCommand
        {
            get => authorizationCommand ?? (authorizationCommand = new RelayCommand(obj =>
            {
                Model.LogIn();
                _navigationStore.CurrentViewModel = new MainMenuLayoutViewModel();
            }));
        }


        private RelayCommand _changeAuthorizationFormCommand;
        public ICommand ChangeAuthorizationFormCommand
        {
            get => _changeAuthorizationFormCommand ?? (_changeAuthorizationFormCommand = new RelayCommand(obj =>
            {

            }));
        }


        private RelayCommand _passwordResetCommand;
        public ICommand PasswordResetCommand
        {
            get => _passwordResetCommand ?? (_passwordResetCommand = new RelayCommand(obj =>
            {
                _navigationStore.CurrentViewModel = new PasswordResetViewModel(_navigationStore);
            }));
        }


        #endregion Commands


        #region Constructors


        public NightWorldAuthorizationViewModel(INavigationStore navigationStore, string loadedLogin = "")
        {
            Model = new NightWorldAuthorizationModel(null, loadedLogin);
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
