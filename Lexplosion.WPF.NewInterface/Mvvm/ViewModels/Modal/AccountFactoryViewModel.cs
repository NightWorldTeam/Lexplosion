using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Modal;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class AccountFactoryViewModel : ActionModalViewModelBase
    {
        public AccountFactoryModel Model { get;  }


        public AccountFactoryViewModel(Action<Account> addAccount)
        {
            Model = new AccountFactoryModel(addAccount);
            ActionCommandExecutedEvent += (o) =>
            {
                Model.Auth();
            };
        }
    }
}
