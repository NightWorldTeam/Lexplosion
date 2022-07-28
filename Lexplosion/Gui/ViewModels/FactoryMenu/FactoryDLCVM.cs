using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public enum MarketDLCType 
    {
        Mods,
        Resourcepacks,
        World
    }

    public class FactoryDLCVM : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;

        private ObservableCollection<InstanceAddon> _installedMods;
        public ObservableCollection<InstanceAddon> InstalledMods 
        {
            get => _installedMods; set 
            {
                _installedMods = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<InstanceAddon> _installedResourcepacks;
        public ObservableCollection<InstanceAddon> InstalledResourcepacks
        {
            get => _installedResourcepacks; set 
            {
                _installedResourcepacks = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<InstanceAddon> _InstalledWorlds;
        public ObservableCollection<InstanceAddon> InstalledWorlds
        {
            get => _InstalledWorlds; set 
            {
                _InstalledWorlds = value;
                OnPropertyChanged();
            }
        }



        private RelayCommand _curseforgeCommand;
        private RelayCommand _updateCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _openMarket;

        #region Command

        public RelayCommand CurseforgeCommand
        {
            get => _curseforgeCommand ?? (new RelayCommand(obj =>
            {
                if (obj != null)
                {
                    System.Diagnostics.Process.Start((string)obj);
                }
                else 
                {
                    MainViewModel.ShowToastMessage("Link null", "Отсутсвует ссылка на страницу curseforge.", Controls.ToastMessageState.Error);
                }
            }));
        }

        public RelayCommand UpdateCommand
        {
            get => _updateCommand ?? (new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;
            }));
        }

        public RelayCommand DeleteCommand
        {
            get => _deleteCommand ?? (new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;
                InstalledMods.Remove(instanceAddon);
            }));
        }

        public RelayCommand OpenMarket 
        {
            get => _openMarket ?? (new RelayCommand(obj => 
            {
                var type = (MarketDLCType)obj;
                switch (type) 
                {
                    case MarketDLCType.Mods:
                        MainViewModel.NavigationStore.PrevViewModel = MainViewModel.NavigationStore.CurrentViewModel;
                        MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(InstalledMods, _mainViewModel, _instanceClient);
                        break;
                    case MarketDLCType.Resourcepacks:
                        MainViewModel.NavigationStore.PrevViewModel = MainViewModel.NavigationStore.CurrentViewModel;
                        MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(InstalledResourcepacks, _mainViewModel, _instanceClient);
                        break;
                    case MarketDLCType.World:
                        MainViewModel.NavigationStore.PrevViewModel = MainViewModel.NavigationStore.CurrentViewModel;
                        MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(InstalledWorlds, _mainViewModel, _instanceClient);
                        break;
                }
            }));
        }
        #endregion

        public FactoryDLCVM(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            _mainViewModel = mainViewModel;
            _instanceClient = instanceClient;
            
            Lexplosion.Run.TaskRun(() => 
            {
                InstalledMods = new ObservableCollection<InstanceAddon>(InstanceAddon.GetInstalledMods(instanceClient.GetBaseData));
                InstalledResourcepacks = new ObservableCollection<InstanceAddon>(InstanceAddon.GetInstalledResourcepacks(instanceClient.GetBaseData));
                InstalledWorlds = new ObservableCollection<InstanceAddon>(InstanceAddon.GetInstalledWorlds(instanceClient.GetBaseData));
            });
        }
    }
}