using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.Views.CustomControls;
using Lexplosion.Logic.Management.Instances;
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
        private int _tabControlSelectedValue;

        private MainViewModel _mainViewModel;

        private ObservableCollection<Tab> _showCaseTabMenu = new ObservableCollection<Tab>();
        private ObservableCollection<Tab> _settingsTabs = new ObservableCollection<Tab>();

        private InstanceFormViewModel _instanceForm;

        #region commands

        public ICommand NavigationMainMenuCommand
        {
            get => new NavigateCommand<MainMenuViewModel>
                (
                    MainViewModel.NavigationStore, () => 
                    { 
                        NavigationToMainMenu?.Invoke();
                        return MainViewModel.MainMenuVM; 
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

        #endregion


        #region props

        public int TabControlSelectedIndex
        {
            get => _tabControlSelectedValue;
            set
            {
                _tabControlSelectedValue = value;
                OnPropertyChanged(nameof(TabControlSelectedIndex));
                if (value == Tabs.Count - 1)
                {
                    NavigationMainMenuCommand.Execute(null);
                }
            }
        }

        #endregion


        public InstanceMenuViewModel(InstanceFormViewModel instanceForm, MainViewModel mainViewModel) : base()
        {
            _instanceForm = instanceForm;

            _mainViewModel = mainViewModel;
            OnInstanceStateChanged();

            _instanceForm.Client.StateChanged += OnInstanceStateChanged;

            ObservableColletionSort(_settingsTabs);
            SelectedTab = Tabs[0];
        }

        private void OnInstanceStateChanged() 
        {
            if (_instanceForm.Client.InLibrary)
                UpdateSettingsTab();

            UpdateShowCaseMenu();
            UpdateTabMenu();
        }

        private void UpdateShowCaseMenu() 
        {

            if (!_instanceForm.Client.InLibrary)
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
            }
            else 
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
                        Header = ResourceGetter.GetString("mods"),
                        Content = new DevСurtainViewModel()
                    }
                );
                _showCaseTabMenu.Add(
                    new Tab()
                    {
                        Header = ResourceGetter.GetString("changelog"),
                        Content = new DevСurtainViewModel()
                    }
                    );
            }
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
                    Content = new TabMenuViewModel(_showCaseTabMenu, _instanceForm.Client.Name, _instanceForm)
                });

                Tabs.Add(new Tab
                {
                    Header = ResourceGetter.GetString("configuration"),
                    Content = new TabMenuViewModel(_settingsTabs, ResourceGetter.GetString("instanceSettings")),
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
                    Content = new TabMenuViewModel(_showCaseTabMenu, _instanceForm.Client.Name, _instanceForm)
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

        private static void ObservableColletionSort<T>(ObservableCollection<T> colletion) 
        {
            List<T> list = new List<T>(colletion);

            list.Sort();

            colletion =  new ObservableCollection<T>(list);
        }
    }
}
