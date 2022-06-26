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

        public ObservableCollection<InstanceAddon> InstalledMods { get; private set; }

        private RelayCommand _curseforgeCommand;
        private RelayCommand _updateCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _openMarket;

        #region Command

        public RelayCommand CurseforgeCommand
        {
            get => _curseforgeCommand ?? (new RelayCommand(obj =>
            {
                System.Diagnostics.Process.Start((string)obj);
            }));
        }

        public RelayCommand UpdateCommand
        {
            get => _updateCommand ?? (new RelayCommand(obj =>
            {

            }));
        }

        public RelayCommand DeleteCommand
        {
            get => _deleteCommand ?? (new RelayCommand(obj =>
            {

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
                        MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(_mainViewModel, _instanceClient);
                        break;
                    case MarketDLCType.Resourcepacks:
                        break;
                    case MarketDLCType.World:
                        break;
                }
            }));
        }
        #endregion

        public FactoryDLCVM(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            _mainViewModel = mainViewModel;
            _instanceClient = instanceClient;
            //InstalledMods = new ObservableCollection<InstanceAddon>(InstanceAddon.GetInstalledMods(instanceClient.GetBaseData));
        }
    }
}
