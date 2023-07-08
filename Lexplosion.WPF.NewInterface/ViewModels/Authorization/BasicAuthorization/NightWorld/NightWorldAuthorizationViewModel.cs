using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;
using Lexplosion.WPF.NewInterface.Models.Authorization;
using Lexplosion.WPF.NewInterface.Models.Authorization.BasicAuthorization;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public sealed class NightWorldAuthorizationViewModel : VMBase
    {
        private readonly INavigationStore<VMBase> _navigationStore;

        public IBasicAuthModel Model { get; }


        #region Commands


        private RelayCommand authorizationCommand;
        public ICommand AuthorizationCommand 
        {
            get => authorizationCommand ?? (authorizationCommand = new RelayCommand(obj => 
            {
                Model.LogIn();
                _navigationStore.Open(new MainMenuLayoutViewModel());
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
                _navigationStore.Open(new PasswordResetViewModel(_navigationStore));
            }));
        }


        #endregion Commands


        #region Constructors


        public NightWorldAuthorizationViewModel(INavigationStore<VMBase> navigationStore, string loadedLogin = "")
        {
            Model = new NightWorldAuthorizationModel(null, loadedLogin);
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
