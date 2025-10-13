using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Stores;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly AppCore _appCore;


        private readonly InstanceModelBase _instanceModel;
        private readonly INavigationStore _navigationStore;

        private ViewModelBase _overviewViewModel = null;
        private ViewModelBase _instanceVersionsViewModel = null;
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


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands



        #endregion Commands


        #region Constructors


        public InstanceProfileLayoutViewModel(AppCore appCore, INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, InstanceModelBase instanceModelBase)
        {
            _appCore = appCore;
            _instanceModel = instanceModelBase;
            _navigationStore = navigationStore;
            LeftPanel = new InstanceProfileLeftPanelViewModel(appCore, navigationStore, toMainMenuLayoutCommand, _instanceModel);
            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            InitDefaultLeftPanelTabs();
            Runtime.DebugWrite("Instance Profile Layout created");
        }


        #endregion Constructors


        #region Public & Protected Methods


        /// <summary>
        /// Делает активным вкладку с Аддонами, если сборка установлена или в библиотеке.
        /// </summary>
        public void OpenAddonContainerPage()
        {
            if (_instanceModel.IsInstalled || _instanceModel.InLibrary)
            {
                LeftPanel.SelectItem(1);
            }
        }


        #endregion Public & Protected Methods


        #region Private Methods


        /// <summary>
        /// TODO IMPORTANT!!! Пролаг при открытии страницы сборки вероятнее всего из-за InstanceData в InstanceModelBase.
        /// </summary>


        private void InitDefaultLeftPanelTabs()
        {
            _overviewViewModel = new InstanceProfileOverviewLayoutViewModel(_appCore, _instanceModel);

            if (_instanceModel.InLibrary)
            {
                _addonsViewModel = new InstanceProfileAddonsLayoutViewModel(_appCore, _instanceModel);
                _settingsLayoutViewModel = new InstanceProfileSettingsLayoutViewModel(_instanceModel);
            }

            LeftPanel.AddTabItem("Overview", "Services", _overviewViewModel);

            if (_instanceModel.InLibrary)
            {
                LeftPanel.AddTabItem("Addons", "Addons", _addonsViewModel);
                LeftPanel.AddTabItem("Settings", "Settings", _settingsLayoutViewModel);
            }

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
