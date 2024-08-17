using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceDistributionModel : ViewModelBase
    {
        private readonly LibraryController _controller;
        private readonly InstanceSharesController _sharesController;

        public IEnumerable<InstanceDistribution> Distributions { get => _sharesController.AvailableInstanceDistribution; }
        public bool IsEmpty { get; }


        public InstanceDistributionModel(LibraryController controller, InstanceSharesController instanceSharesController) 
        {
            _controller = controller;
            _sharesController = instanceSharesController;
            LoadInstanceDistribution();
        }



        #region Public Methods

        public async void LoadInstanceDistribution()
        {
            if (Account.ActiveAccount == null) 
            {
                // Todo: throw exeption
                return;
            }

            var receivers = await Task.Run(() => FileReceiver.GetDistributors(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken));

            foreach (var receiver in receivers) 
            {
                if (!_sharesController.Contains(receiver.Id))
                {
                    _sharesController.AddFileReceiver(
                        new InstanceDistribution(
                            new(receiver, (i) => { }, (i) => { }, _controller, _sharesController)
                            )
                        );
                }
            }
        }

        public void Download(InstanceDistribution instanceDistribution) 
        {
            instanceDistribution.Download();
        }


        public void CancelDownloadInstance(InstanceDistribution instanceDistribution)
        {
            instanceDistribution.CancelDownload();
        }


        #endregion Public Methods
    }


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
