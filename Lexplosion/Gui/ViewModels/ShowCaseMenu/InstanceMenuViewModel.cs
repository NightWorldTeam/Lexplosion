using Lexplosion.Gui.Commands;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.Views.CustomControls;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Gui.ViewModels.ShowCaseMenu
{
    public class InstanceMenuViewModel : SubmenuViewModel, ISubmenu
    {
        public event ISubmenu.NavigationToMenuCallBack NavigationToMainMenu;
        private int _tabControlSelectedValue;

        private MainViewModel _mainViewModel;

        private ObservableCollection<Tab> _showCaseTabMenu = new ObservableCollection<Tab>();
        private ObservableCollection<Tab> _settingsTabs = new ObservableCollection<Tab>();

        private InstanceFormViewModel _instanceForm;
        private List<VMBase> _buttons = new List<VMBase>();


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

            if (_instanceForm.Client.InLibrary)
                UpdateSettingsTab();

            UpdateShowCaseMenu();
            UpdateTabMenu();


            ObservableColletionSort(_settingsTabs);
            SelectedTab = Tabs[0];
        }

        public void OnInstanceStateChanged() 
        {

        }

        private void UpdateShowCaseMenu() 
        {

            if (!_instanceForm.Client.InLibrary)
            {
                _showCaseTabMenu.Add(
                    new Tab()
                    {
                        Header = "Overview",
                        Content = new OverviewViewModel(_instanceForm.Client, this)
                    }
                );
                _showCaseTabMenu.Add(
                    new Tab()
                    {
                        Header = "Changelog",
                        Content = new DevСurtainViewModel()
                    }
                );
            }
            else 
            {
                _showCaseTabMenu.Add(
                     new Tab()
                     {
                          Header = "Overview",
                          Content = new OverviewViewModel(_instanceForm.Client, this)
                     }
                     );
                _showCaseTabMenu.Add(
                    new Tab()
                    {
                        Header = "Mods",
                        Content = new DevСurtainViewModel()
                    }
                );
                _showCaseTabMenu.Add(
                    new Tab()
                    {
                        Header = "Changelog",
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
                    Header = "Обзор",
                    Content = new TabMenuViewModel(_showCaseTabMenu, _instanceForm.Client.Name, _instanceForm)
                });

                Tabs.Add(new Tab
                {
                    Header = "Конфигурация",
                    Content = new TabMenuViewModel(_settingsTabs, "Настройки сборки"),
                });

                Tabs.Add(new Tab
                {
                    Header = "Назад",
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
                    Header = "Обзор",
                    Content = new TabMenuViewModel(_showCaseTabMenu, _instanceForm.Client.Name, _instanceForm)
                });

                Tabs.Add(new Tab
                {
                    Header = "Назад",
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

                _settingsTabs.Add(new Tab { Id = 1, Header = "О Сборке", Content = new InstanceProfileViewModel(_instanceForm.Client) });
                _settingsTabs.Add(new Tab
                {
                    Id = 4,
                    Header = "Журнал изменений",
                    Content = new DevСurtainViewModel()
                });
            }

                _settingsTabs.Add(
                    new Tab
                    {
                        Id = 0,
                        Header = "Параметры",
                        Content = new InstanceSettingsViewModel(_instanceForm.Client)
                    }
                );
                _settingsTabs.Add(
                    new Tab
                    {
                        Id = 3,
                        Header = "Дополнения",
                        Content = new FactoryDLCVM(_mainViewModel, _instanceForm.Client)
                    }
                );
        }

        private void ObservableColletionSort<T>(ObservableCollection<T> colletion) 
        {
            List<T> list = new List<T>(colletion);

            list.Sort();

            colletion =  new ObservableCollection<T>(list);
        }
    }
}
