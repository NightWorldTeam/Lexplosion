using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public sealed class InstanceMenuViewModel : SubmenuViewModel, ISubmenu
    {
        public event ISubmenu.NavigationToMenuCallBack NavigationToMainMenu;

        private int _selectedSettingsTabIndex;

        private readonly MainViewModel _mainViewModel;

        private readonly ObservableCollection<Tab> _showCaseTabMenu = new ObservableCollection<Tab>();
        private readonly ObservableCollection<Tab> _settingsTabs = new ObservableCollection<Tab>();

        private readonly InstanceFormViewModel _instanceForm;


        #region Commands


        public ICommand NavigationMainMenuCommand
        {
            get => new NavigateCommand<MainMenuViewModel>
                (
                    MainViewModel.NavigationStore, () => 
                    { 
                        NavigationToMainMenu?.Invoke();
                        return _mainViewModel.MainMenuVM; 
                    }
                );
        }

        private RelayCommand ClearMemory
        {
            get => new RelayCommand(obj =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }


        #endregion Commands


        #region Properties


        private int _tabControlSelectedValue;
        public int TabControlSelectedIndex
        {
            get => _tabControlSelectedValue;
            set
            {
                _tabControlSelectedValue = value;
                OnPropertyChanged();
                if (value == Tabs.Count - 1)
                {
                    NavigationMainMenuCommand.Execute(null);
                }
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceMenuViewModel(InstanceFormViewModel instanceForm, MainViewModel mainViewModel, int selectedTab = 0, int selectedSettingsTabIndex = 0) : base()
        {
            _instanceForm = instanceForm;
            _selectedSettingsTabIndex = selectedSettingsTabIndex;

            _mainViewModel = mainViewModel;
            OnInstanceStateChanged();

            _instanceForm.Client.StateChanged += OnInstanceStateChanged;

            ObservableCollectionExtensions.ObservableColletionSort(_settingsTabs);
            SelectedTab = Tabs[selectedTab];
        }


        #endregion Constructors


        #region Private Methods


        private void OnInstanceStateChanged() 
        {
            if (_instanceForm.Client.InLibrary)
                UpdateSettingsTab();

            UpdateShowCaseMenu();
            UpdateTabMenu();
        }

        private void UpdateShowCaseMenu()
        {
            _showCaseTabMenu.Add(
                new Tab()
                {
                    Header = ResourceGetter.GetString("general"),
                    Content = new OverviewViewModel(_instanceForm.Client, this)
                }
            );
            _showCaseTabMenu.Add(
                new Tab()
                {
                    Header = ResourceGetter.GetString("changelog"),
                    Content = new DevСurtainViewModel()
                }
            );
            _showCaseTabMenu.Add(
                new Tab() 
                {
                    Header = ResourceGetter.GetString("version"),
                    Content = new InstancePreviousVersionsViewModel(_instanceForm)
                });
        }

        private void UpdateTabMenu() 
        {
            if (_instanceForm.Client.IsInstalled || _instanceForm.Client.InLibrary) 
            {
                if (Tabs.Count == 3)
                    return;

                Tabs.Clear();

                
                Tabs.Add(new Tab
                {
                    Header = ResourceGetter.GetString("overview"),
                    Content = new TabMenuViewModel(_showCaseTabMenu, _instanceForm.Client.Name, 0, _instanceForm)
                });

                Tabs.Add(new Tab
                {
                    Header = ResourceGetter.GetString("configuration"),
                    Content = new TabMenuViewModel(_settingsTabs, ResourceGetter.GetString("instanceSettings"), _selectedSettingsTabIndex),
                });

                Tabs.Add(new Tab
                {
                    Header = ResourceGetter.GetString("back"),
                    Content = null,
                    Command = ClearMemory
                });
            }
            else 
            {
                if (Tabs.Count == 2)
                    return;

                Tabs.Clear();

                Tabs.Add(new Tab
                {
                    Header = ResourceGetter.GetString("overview"),
                    Content = new TabMenuViewModel(_showCaseTabMenu, _instanceForm.Client.Name, 0, _instanceForm)
                });

                Tabs.Add(new Tab
                {
                    Header = ResourceGetter.GetString("back"),
                    Content = null,
                    Command = ClearMemory
                });
            }
        }

        private void UpdateSettingsTab() 
        {
            if (_instanceForm.Client.Type == InstanceSource.Local)
            {
                if (_settingsTabs.Count == 4)
                    return;

                _settingsTabs.Add(new Tab { Id = 1, Header = ResourceGetter.GetString("aboutInstance"), Content = new InstanceProfileViewModel(_instanceForm.Client) });
                _settingsTabs.Add(new Tab
                {
                    Id = 4,
                    Header = ResourceGetter.GetString("changelog"),
                    Content = new DevСurtainViewModel()
                });
            }

                _settingsTabs.Add(
                    new Tab
                    {
                        Id = 0,
                        Header = ResourceGetter.GetString("settings"),
                        Content = new InstanceSettingsViewModel(_instanceForm.Client)
                    }
                );
                _settingsTabs.Add(
                    new Tab
                    {
                        Id = 3,
                        Header = ResourceGetter.GetString("dlc"),
                        Content = new FactoryDLCVM(_mainViewModel, _instanceForm.Client)
                    }
                );
        }


        #endregion Private Methods
    }
}
