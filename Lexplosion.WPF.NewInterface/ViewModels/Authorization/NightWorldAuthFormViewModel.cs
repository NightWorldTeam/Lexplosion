using Lexplosion.WPF.NewInterface.Commands;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public sealed class AuthorizationFormModel 
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


        #endregion Commands


        #region Constructors


        public NightWorldAuthFormViewModel()
        {

        }


        #endregion Constructors
    }
}
