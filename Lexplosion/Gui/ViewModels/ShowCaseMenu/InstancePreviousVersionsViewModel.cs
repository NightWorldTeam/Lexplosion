using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstancePreviousVersionsViewModel : VMBase
    {
        private readonly InstanceFormViewModel _viewModel;

        #region Properties


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
                _viewModel.DownloadInstance(version: instanceVersion.Id);
                foreach (var pv in PreviousVersions) 
                {
                    pv.CanInstall = false;
                }
            }));
        }


        #endregion


        #region Constructors


        public InstancePreviousVersionsViewModel(InstanceFormViewModel viewModel)
        {
            _viewModel = viewModel;
            LoadPreviousVersions();
            _viewModel.Model.DownloadModel.ComplitedDownloadActions.Add(ComplitedInstalled);
        }


        #endregion Constructors


        #region Public & Protected Methods
        #endregion Public & Protected Methods


        #region Private Methods


        private void LoadPreviousVersions() 
        {
            Lexplosion.Runtime.TaskRun(() => 
            {
                var versions = _viewModel.Model.InstanceClient.GetVersions();
                App.Current.Dispatcher.Invoke(() => 
                {
                    PreviousVersions = new ObservableCollection<InstanceVersion>(versions);
                    DisableButton();
                    IsLoadingFinished = true;
                });
            });
        }

        private void DisableButton(bool isDisable = false) 
        {
            foreach (var pv in PreviousVersions)
            {
                if (pv.Id == _viewModel.Client.ProfileVersion)
                {
                    pv.CanInstall = false;
                    if (isDisable)
                        break;
                }
                else 
                {
                    pv.CanInstall = true;
                }
            }
        }


        private void ComplitedInstalled(InstanceInit result, List<string> downloadErrors, bool launchGame)
        {
            DisableButton(true);
        }

        #endregion Private Methods
    }
}
