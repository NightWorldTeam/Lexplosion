using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Models.Modal;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public class AccountFactoryViewModel : ActionModalViewModelBase
    {
        public AccountFactoryModel Model { get; }


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
