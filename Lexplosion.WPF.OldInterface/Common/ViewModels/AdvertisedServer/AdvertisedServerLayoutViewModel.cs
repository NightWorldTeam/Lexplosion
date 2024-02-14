using Lexplosion.Common.Commands;
using Lexplosion.Common.Models;
using Lexplosion.Common.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.AdvertisedServer
{
    public struct RunInstanceParameters
    {
        public string Name { get; }

        public InstanceFormViewModel InstanceFormViewModel { get; }

        public RunInstanceParameters(InstanceFormViewModel instanceFormViewModel, bool isNew = false)
        {
            InstanceFormViewModel = instanceFormViewModel;

            Name = isNew ? "Создать новую сборку" : instanceFormViewModel.Client.Name;
        }
    }

    public sealed class AdvertisedServerLayoutModel : VMBase
    {
        private readonly MinecraftServerInstance _minecraftServerInstance;

        private ObservableCollection<RunInstanceParameters> _availableInstances;
        public IEnumerable<RunInstanceParameters> AvailableInstances { get => _availableInstances; }


        private RunInstanceParameters _selectedRunInstanceParameters;
        public RunInstanceParameters SelectedRunInstanceParameters
        {
            get => _selectedRunInstanceParameters; set
            {
                _selectedRunInstanceParameters = value;
                OnPropertyChanged();
            }
        }

        public AdvertisedServerLayoutModel(MinecraftServerInstance minecraftServerInstance)
        {
            _minecraftServerInstance = minecraftServerInstance;
            _availableInstances = new ObservableCollection<RunInstanceParameters>(
                MainModel.Instance.LibraryController
                    .GetInstances(ic => ic.GameVersion.ToString() == _minecraftServerInstance.GameVersion)
                    .Select(i => new RunInstanceParameters(i))
            );

            var newInstanceForm = MainModel.Instance.CreateInstanceForm(
                InstanceClient.CreateClient(_minecraftServerInstance, false)
                );

            _availableInstances.Insert(0, new RunInstanceParameters(newInstanceForm, true));

            SelectedRunInstanceParameters = _availableInstances[0];
            OnPropertyChanged(nameof(AvailableInstances));
        }

        public void SetIpAddressToClipboard() 
        {
            Clipboard.SetText(_minecraftServerInstance.Address);
        }
    }

    public sealed class AdvertisedServerLayoutViewModel : SubmenuViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ObservableCollection<Tab<VMBase>> _showCaseTabMenu = new ObservableCollection<Tab<VMBase>>();

        public AdvertisedServerLayoutModel Model { get; }


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
                    BackToMainMenu.Execute(null);
                }
            }
        }


        private int _selectedInstanceIndex;
        public int SelectedInstanceIndex
        {
            get => _selectedInstanceIndex; set
            {
                _selectedInstanceIndex = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties



        #region Commands


        private ICommand BackToMainMenu;

        private RelayCommand _copyAddressCommand;
        public ICommand CopyAddressCommand
        {
            get => _copyAddressCommand ?? (_copyAddressCommand = new RelayCommand(obj =>
            {
                Model.SetIpAddressToClipboard();
                // TODO Translate
                MainViewModel.ShowToastMessage("Копирование", "Ip address успешно был скопирован", 3, 0);
            }));
        }



        #endregion Commands




        public AdvertisedServerLayoutViewModel(MainViewModel mainViewModel, MinecraftServerInstance adServer)
        {
            _mainViewModel = mainViewModel;

            BackToMainMenu = new NavigateCommand<MainMenuViewModel>
                (
                    MainViewModel.NavigationStore, () =>
                    {
                        return _mainViewModel.MainMenuVM;
                    }
                );

            UpdateShowCaseMenu(adServer);

            Model = new AdvertisedServerLayoutModel(adServer);

            Tabs.Add(new Tab<VMBase> { Header = ResourceGetter.GetString("overview"), Content = new TabMenuViewModel(_showCaseTabMenu, adServer.InstanceName, 0, null) });
            Tabs.Add(new Tab<VMBase> { Header = ResourceGetter.GetString("back"), Content = null, Command = BackToMainMenu });

            SelectedTab = Tabs[0];
            SelectedTab.IsSelected = true;
        }

        private void UpdateShowCaseMenu(MinecraftServerInstance adServer)
        {
            _showCaseTabMenu.Clear();

            _showCaseTabMenu.Add(new Tab<VMBase>() { Header = ResourceGetter.GetString("general"), Content = new AdServerOverviewViewModel(adServer) });

            if (adServer.InstanceSource != InstanceSource.Local)
                _showCaseTabMenu.Add(new Tab<VMBase>() { Header = ResourceGetter.GetString("mods"), Content = null });
        }
    }
}

