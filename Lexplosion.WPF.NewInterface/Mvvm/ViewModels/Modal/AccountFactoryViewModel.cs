using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Modal;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class AccountFactoryViewModel : ActionModalViewModelBase
    {
        public AccountFactoryModel Model { get;  }


        #region Constructors


        public AccountFactoryViewModel()
        {
            Model = new AccountFactoryModel();
            ActionCommandExecutedEvent += (o) =>
            {
                Model.Auth();
            };
        }


        #endregion Constructors
    }
}
