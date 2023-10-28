using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLeftPanelViewModel : LeftPanelViewModel
    {
        private InstanceModelBase _instanceModel;
        private INavigationStore _navigationStore;
        private NavigateCommand<ViewModelBase> _toMainMenuLayoutCommand;

        #region Properties


        public ImageBrush InstanceImage
        {
            get => new ImageBrush(ImageTools.ToImage(_instanceModel.Logo));
        }

        public string InstanceName { get => _instanceModel.Name; }
        public string InstanceVersion { get => _instanceModel.InstanceData.GameVersion?.Id; }
        public string InstanceModloader { get => _instanceModel.InstanceData.Modloader.ToString(); }
        public string PlayerPlayedTime { get => _instanceModel.IsInstalled ? "10ч" : DownloadCount; }
        public string DownloadCount { get => _instanceModel.TotalDonwloads; }


        private ObservableCollection<FrameworkElementModel> _instanceActions = new ObservableCollection<FrameworkElementModel>();
        public IEnumerable<FrameworkElementModel> InstanceActions { get => _instanceActions; }


        #endregion Properties


        #region Commands


        private RelayCommand _playCommand;
        public ICommand PlayCommand
        {
            get => RelayCommand.GetCommand(ref _playCommand, (obj) => { _instanceModel.Run(); });
        }

        private RelayCommand _backCommand;
        public ICommand BackCommand
        {
            get => RelayCommand.GetCommand(ref _backCommand, () =>
            {
                _toMainMenuLayoutCommand.Execute(null);
            });
        }


        #endregion Commands


        #region Contructors


        public InstanceProfileLeftPanelViewModel(INavigationStore navigationStore, NavigateCommand<ViewModelBase> toMainMenuLayoutCommand, InstanceModelBase instanceModelBase)
        {
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
            _navigationStore = navigationStore;

            _instanceModel = instanceModelBase;
            _instanceModel.NameChanged += OnNameChanged;
            _instanceModel.GameVersionChanged += OnVersionChanged;
            _instanceModel.ModloaderChanged += OnModloaderChanged;
            _instanceModel.StageChanged += OnStateChanged;

            UpdateFrameworkElementModels();
        }


        #endregion Constructors


        #region Public Methods





        #endregion Public Methods


        #region Private Methods


        private void OnNameChanged()
        {
            OnPropertyChanged(nameof(InstanceName));
        }

        private void OnVersionChanged()
        {
            OnPropertyChanged(nameof(InstanceVersion));
        }

        private void OnModloaderChanged()
        {
            OnPropertyChanged(nameof(InstanceModloader));
        }

        private void OnStateChanged()
        {
            UpdateFrameworkElementModels();
        }

        private void UpdateFrameworkElementModels()
        {
            _instanceActions.Clear();
            // 1. Website
            // 2. AddToLibrary
            // 2. OpenFolder
            // 3. Export
            // 4. RemoveFromLibrary / Delete

            if (_instanceModel.Source != InstanceSource.Local)
            {
                _instanceActions.Add(new FrameworkElementModel("Visit" + _instanceModel.Source.ToString(), _instanceModel.GoToWebsite, _instanceModel.Source.ToString(), 20, 20));
            }

            if (!_instanceModel.IsInstalled && !_instanceModel.InLibrary)
            {
                _instanceActions.Add(new FrameworkElementModel("AddToLibrary", _instanceModel.AddToLibrary, "AddToLibrary"));
            }

            if (_instanceModel.InLibrary)
            {
                _instanceActions.Add(new FrameworkElementModel("OpenFolder", _instanceModel.OpenFolder, "Folder"));
                if (_instanceModel.IsInstalled)
                {
                    _instanceActions.Add(new FrameworkElementModel("Export", _instanceModel.Export, "Export"));
                }
            }

            if (!_instanceModel.IsInstalled && _instanceModel.InLibrary)
            {
                _instanceActions.Add(new FrameworkElementModel("RemoveFromLibrary", _instanceModel.Delete, "Delete"));
            }
            else if (_instanceModel.IsInstalled)
            {
                _instanceActions.Add(new FrameworkElementModel("DeleteInstance", _instanceModel.Delete, "Delete"));
            }
        }


        #endregion Private Methods
    }
}
