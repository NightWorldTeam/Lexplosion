using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using System.Threading.Tasks;
using static Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel.InstanceModelBase;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
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

        public bool IsEmpty { get => _sharesController.AvailableInstanceDistribution.Count == 0 && !IsLoading; }

        public bool IsLoading { get; private set; }

        public InstanceDistributionModel(LibraryController controller, InstanceSharesController instanceSharesController)
        {
            IsLoading = true;
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

			var services = Runtime.ServicesContainer;
			var receivers = await Task.Run(() => FileReceiver.GetDistributors(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, services));

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
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
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
}
