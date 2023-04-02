using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstancePreviousVersionsViewModel : VMBase
    {
        #region Properties

        public InstanceFormViewModel ViewModel { get; }

        private ObservableCollection<InstanceVersion> _previousVersions;
        public ObservableCollection<InstanceVersion> PreviousVersions
        {
            get => _previousVersions; set
            {
                _previousVersions = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoadingFinished;
        public bool IsLoadingFinished
        {
            get => _isLoadingFinished; set
            {
                _isLoadingFinished = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Commands


        private RelayCommand _installInstanceCommand;
        public RelayCommand InstallInstanceCommand
        {
            get => _installInstanceCommand ?? (_installInstanceCommand = new RelayCommand(obj =>
            {
                var instanceVersion = (InstanceVersion)obj;
                ViewModel.DownloadInstance(version: instanceVersion.Id);
                foreach (var pv in PreviousVersions)
                {
                    pv.CanInstall = false;
                }
            }));
        }


        #endregion Commands


        #region Constructors


        public InstancePreviousVersionsViewModel(InstanceFormViewModel viewModel)
        {
            ViewModel = viewModel;
            LoadPreviousVersions();
        }


        #endregion Constructors


        #region Private Methods


        private async void LoadPreviousVersions()
        {
            var versions = await Task.Run(() => ViewModel.Model.InstanceClient.GetVersions());
            PreviousVersions = new ObservableCollection<InstanceVersion>(versions);
            if (ViewModel.Model.InstanceClient.IsInstalled || ViewModel.Model.InstanceClient.InLibrary)
                ChangeButtonState();
            IsLoadingFinished = true;
            ViewModel.Model.DownloadModel.ComplitedDownloadActions.Add(ComplitedInstalled);
        }

        private void ChangeButtonState(bool isDisable = false)
        {
            foreach (var pv in PreviousVersions)
            {
                pv.CanInstall = true;
                if (pv.Id == ViewModel.Client.ProfileVersion)
                {
                    pv.CanInstall = false;
                    if (isDisable)
                        break;
                }
            }
        }


        private void ComplitedInstalled(InstanceInit result, List<string> downloadErrors, bool launchGame)
        {
            ChangeButtonState(true);
        }

        #endregion Private Methods
    }
}
