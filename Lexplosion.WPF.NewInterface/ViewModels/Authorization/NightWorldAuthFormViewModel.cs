using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public sealed class AuthorizationFormModel : VMBase
    {
        #region Properties


        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsSavePassword { get; set; }


        #endregion Properties


        #region Constructors


        public AuthorizationFormModel(AccountTypeMenuItem accountType) 
        {
            
        }


        #endregion Constructors


        #region Public Methods


        public void Auth() 
        {

        }


        #endregion Public Methods
    }

    public sealed class NightWorldAuthFormViewModel : VMBase
    {
        private readonly INavigationStore<VMBase> _navigationStore;

        public AuthorizationFormModel Model { get; private set; }


        #region Commands


        private RelayCommand authorizationCommand;
        public ICommand AuthorizationCommand 
        {
            get => authorizationCommand ?? (authorizationCommand = new RelayCommand(obj => 
            {
                var newObj = obj as AuthorizationFormModel;
                newObj.Auth();
            }));
        }


        private RelayCommand _changeAuthorizationFormCommand;
        public ICommand ChangeAuthorizationFormCommand 
        {
            get => _changeAuthorizationFormCommand ?? (_changeAuthorizationFormCommand = new RelayCommand(obj => 
            {
                var accountType = obj as AccountTypeMenuItem;
                Model = new AuthorizationFormModel(accountType);
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


        public NightWorldAuthFormViewModel(INavigationStore<VMBase> navigationStore)
        {
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
