using Lexplosion.WPF.NewInterface.Models.Authorization;

namespace Lexplosion.WPF.NewInterface.ViewModels.Authorization
{
    public abstract class AuthorizationViewModelBase : VMBase
    {
        public IAuthorizationModel model;

        protected AuthorizationViewModelBase() 
        {

        }
    }
}
