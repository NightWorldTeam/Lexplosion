using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Tools;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly InstanceModelBase _instanceModel;

        private ViewModelBase _overviewViewModel = null;
        private ViewModelBase _settingsLayoutViewModel = null;
        private ViewModelBase _addonsViewModel = null;


        #region Properties


        private InstanceProfileLeftPanelViewModel _leftPanel;
        public InstanceProfileLeftPanelViewModel LeftPanel
        {
            get => _leftPanel; set
            {
                _leftPanel = value;
                OnPropertyChanged();
            }
        }

        public ViewModelBase Content { get; private set; }


        #endregion Properties


        #region Commands


        private RelayCommand _backCommand;
        public ICommand BackCoommand 
        {
            get => RelayCommand.GetCommand(ref _backCommand, null);
        }


        #endregion Commands


        #region Constructors


        public InstanceProfileLayoutViewModel(InstanceModelBase instanceModelBase)
        {
            _instanceModel = instanceModelBase;
            LeftPanel = new InstanceProfileLeftPanelViewModel();
            _instanceModel.LogoChanged += () =>
            {
                App.Current.Dispatcher.Invoke(() => { 
                    LeftPanel.InstanceImage = new ImageBrush(ImageTools.ToImage(_instanceModel.Logo));
                });
            };
            LeftPanel.InstanceName = _instanceModel.Name;
            LeftPanel.InstanceImage = new ImageBrush(ImageTools.ToImage(_instanceModel.Logo));
            LeftPanel.InstanceVersion = _instanceModel.InstanceData.GameVersion;
            LeftPanel.InstanceModloader = _instanceModel.InstanceData.Modloader.ToString();

            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            InitDefaultLeftPanelTabs();
        }


        #endregion Constructors


        #region Private Methods


        private void InitDefaultLeftPanelTabs()
        {
            _overviewViewModel = new InstanceProfileOverviewViewModel();
            _settingsLayoutViewModel = new InstanceProfileSettingsLayoutViewModel(_instanceModel);
            _addonsViewModel = new InstanceProfileAddonsLayoutViewModel(_instanceModel);

            LeftPanel.AddTabItem("Overview", "Services", _overviewViewModel);
            LeftPanel.AddTabItem("Addons", "Addons", _addonsViewModel);
            LeftPanel.AddTabItem("Settings", "Settings", _settingsLayoutViewModel);
            LeftPanel.SelectFirst();
        }

        private void OnLeftPanelSelectedItemChanged(ViewModelBase content)
        {
            Content = content;
            OnPropertyChanged(nameof(Content));
        }


        #endregion Private Methods
    }
}
