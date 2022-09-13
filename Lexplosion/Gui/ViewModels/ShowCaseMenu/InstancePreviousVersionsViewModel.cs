using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstancePreviousVersionsViewModel : VMBase
    {
        private InstanceFormViewModel _viewModel;


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


        private RelayCommand _installInstanceCommand;
        public RelayCommand InstallInstanceCommand 
        {
            get => _installInstanceCommand ?? (_installInstanceCommand = new RelayCommand(obj => 
            {
                _viewModel.DownloadInstance();
            }));
        }

        #endregion Properties


        #region Constructors


        public InstancePreviousVersionsViewModel(InstanceFormViewModel viewModel)
        {
            _viewModel = viewModel;
            LoadPreviousVersions();
        }


        #endregion Constructors


        #region Public & Protected Methods
        #endregion Public & Protected Methods


        #region Private Methods


        private void LoadPreviousVersions() 
        {
            Lexplosion.Run.TaskRun(() => 
            {
                var versions = _viewModel.Model.InstanceClient.GetVersions();
                App.Current.Dispatcher.Invoke(() => 
                {
                    PreviousVersions = new ObservableCollection<InstanceVersion>(versions);
                });
            });
        }


        #endregion Private Methods
    }
}
