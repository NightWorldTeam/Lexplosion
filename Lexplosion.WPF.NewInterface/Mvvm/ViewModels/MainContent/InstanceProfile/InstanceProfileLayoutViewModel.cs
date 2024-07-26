using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using System;
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


        public InstanceProfileLayoutViewModel(INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, InstanceModelBase instanceModelBase)
        {
            _instanceModel = instanceModelBase;
            _navigationStore = navigationStore;
            LeftPanel = new InstanceProfileLeftPanelViewModel(navigationStore, toMainMenuLayoutCommand, _instanceModel);
            LeftPanel.SelectedItemChanged += OnLeftPanelSelectedItemChanged;

            InitDefaultLeftPanelTabs();
            Runtime.DebugWrite("Instance Profile Layout created");
        }


        #endregion Constructors


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

            Lexplosion.Runtime.TaskRun(() =>
            {
                LeftPanel.AddTabItem("Overview", "Services", _overviewViewModel);

                if (_instanceModel.InLibrary) 
                {
                    LeftPanel.AddTabItem("Addons", "Addons", _addonsViewModel);
                    LeftPanel.AddTabItem("Settings", "Settings", _settingsLayoutViewModel);
                }

                LeftPanel.SelectFirst();
            });
        }

        private void OnLeftPanelSelectedItemChanged(ViewModelBase content)
        {
            Content = content;
            OnPropertyChanged(nameof(Content));
        }


        #endregion Private Methods
    }
}
