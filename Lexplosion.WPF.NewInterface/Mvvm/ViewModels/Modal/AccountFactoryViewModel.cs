using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class AccountFactoryViewModel : ActionModalViewModelBase
    {
        public AccountFactoryModel Model { get;  }


        public ICommand ManualInputCommand { get; }
        public ICommand CancelCommand { get; }


        #region Constructors


        public AccountFactoryViewModel(AppCore appCore)
        {
            Model = new AccountFactoryModel(appCore, () => CloseCommand.Execute(null), () => appCore.ModalNavigationStore.Open(this));

            CancelCommand = new RelayCommand((obj) => Model.Cancel());
            ManualInputCommand = new RelayCommand((obj) => Model.ManualInput());

            IsCloseAfterCommandExecuted = false;
            ActionCommandExecutedEvent += (o) =>
            {
                Model.Auth();
            };
        }


        #endregion Constructors
    }
}
