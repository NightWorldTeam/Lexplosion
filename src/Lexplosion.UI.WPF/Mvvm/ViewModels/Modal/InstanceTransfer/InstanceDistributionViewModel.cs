using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceDistributionViewModel : ViewModelBase, ILimitedAccess
    {
        public InstanceDistributionModel Model { get; }



        #region Commands


        private RelayCommand _downloadInstanceCommand;
        public ICommand DownloadInstanceCommand
        {
            get => RelayCommand.GetCommand<InstanceDistribution>(ref _downloadInstanceCommand, Model.Download);
        }


        private RelayCommand _refreshListCommand;
        public ICommand RefreshListCommand
        {
            get => RelayCommand.GetCommand(ref _refreshListCommand, Model.LoadInstanceDistribution);
        }


        private RelayCommand _cancelDownloadInstanceCommand;
        public ICommand CancelDownloadInstanceCommand
        {
            get => RelayCommand.GetCommand<InstanceDistribution>(ref _cancelDownloadInstanceCommand, Model.CancelDownloadInstance);
        }

        public bool HasAccess { get; private set; }


        #endregion Commands



        public InstanceDistributionViewModel(LibraryController controller, InstanceSharesController instanceSharesController)
        {
            Model = new InstanceDistributionModel(controller, instanceSharesController);
        }

        public void RefreshAccessData()
        {

        }
    }
}
