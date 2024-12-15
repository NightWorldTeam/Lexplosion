using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceDistributionModel : ViewModelBase
    {
        private readonly LibraryController _controller;
        private readonly InstanceSharesController _sharesController;

        //public IEnumerable<InstanceDistribution> Distributions { get => _sharesController.AvailableInstanceDistribution; }

        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText; set
            {
                _filterText = value;
                OnPropertyChanged();
                Distributions.Filter = DistFilter;
            }
        }

        public FiltableObservableCollection Distributions { get; set; } = new FiltableObservableCollection();

        public bool IsEmpty { get; }

        public InstanceDistributionModel(LibraryController controller, InstanceSharesController instanceSharesController)
        {
            _controller = controller;
            _sharesController = instanceSharesController;

            LoadInstanceDistribution();
        }


        bool DistFilter(object obj)
        {
            var dist = (obj as InstanceDistribution);
            return dist.Name.IndexOf(FilterText ?? string.Empty, System.StringComparison.InvariantCultureIgnoreCase) > -1;
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

            Distributions.Source = _sharesController.AvailableInstanceDistribution;
            Distributions.Filter = DistFilter;
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
