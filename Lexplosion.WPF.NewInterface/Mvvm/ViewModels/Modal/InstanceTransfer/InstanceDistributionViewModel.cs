using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceDistributionViewModel : ViewModelBase
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


        #endregion Commands



        public InstanceDistributionViewModel(LibraryController controller, InstanceSharesController instanceSharesController)
        {
            Model = new InstanceDistributionModel(controller, instanceSharesController);
        }
    }
}
