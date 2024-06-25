using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Modal;
using System;
using System.CodeDom;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class AccountFactoryViewModel : ActionModalViewModelBase
    {
        public AccountFactoryModel Model { get;  }


        #region Commands


        public AccountFactoryViewModel(Action<Account> addAccount)
        {
            Model = new AccountFactoryModel(addAccount);
            ActionCommandExecutedEvent += (o) =>
            {
                Model.Auth();
            };
        }

        #endregion Commands


        #region Constructors


        public AccountFactoryViewModel()
        {
            IsCloseAfterCommandExecuted = false;
        }


        #endregion Constructors
    }
}
