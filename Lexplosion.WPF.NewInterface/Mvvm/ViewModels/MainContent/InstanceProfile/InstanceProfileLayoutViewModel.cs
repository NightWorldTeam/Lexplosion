using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileLayoutViewModel : ViewModelBase, ILayoutViewModel
    {
        private readonly InstanceModelBase _instanceModel;
        private readonly INavigationStore _navigationStore;

        private ViewModelBase _overviewViewModel = null;
        private ViewModelBase _settingsLayoutViewModel = null;
        private ViewModelBase _addonsViewModel = null;


        #region Properties


        public NotifyCallback Notify { get; }


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



        #endregion Commands


        #region Constructors


        public InstanceProfileLayoutViewModel(AppCore appCore, INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, InstanceModelBase instanceModelBase, NotifyCallback? notify = null)
        {
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
            _overviewViewModel = new InstanceProfileOverviewViewModel(_instanceModel);

            if (_instanceModel.InLibrary)
            {
                _addonsViewModel = new InstanceProfileAddonsLayoutViewModel(_navigationStore, _instanceModel);
                _settingsLayoutViewModel = new InstanceProfileSettingsLayoutViewModel(_instanceModel);
            }

            //Lexplosion.Runtime.TaskRun(() =>
            //{
                LeftPanel.AddTabItem("Overview", "Services", _overviewViewModel);

                if (_instanceModel.InLibrary)
                {
                    LeftPanel.AddTabItem("Addons", "Addons", _addonsViewModel);
                    LeftPanel.AddTabItem("Settings", "Settings", _settingsLayoutViewModel);
                }
                //LeftPanel.WaitHandler.Set();
                LeftPanel.SelectFirst();
            //});
        }

        private void OnLeftPanelSelectedItemChanged(ViewModelBase content)
        {
            Content = content;
            OnPropertyChanged(nameof(Content));
        }


        #endregion Private Methods
    }
}
