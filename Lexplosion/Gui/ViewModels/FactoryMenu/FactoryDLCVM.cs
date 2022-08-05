using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public class FactoryDLCVM : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;

        private List<FactoryDLCModel> _models = new List<FactoryDLCModel>();

        private FactoryDLCModel _currentAddon;
        public FactoryDLCModel CurrentAddon 
        {
            get => _currentAddon; set 
            {
                _currentAddon = value;
                OnPropertyChanged();
            }
        }

        private int _selectedAddonIndex;
        public int SelectedAddonIndex
        {
            get => _selectedAddonIndex; set
            {
                _selectedAddonIndex = value;
                ChangeCurrentAddonModel(value);
                OnPropertyChanged();
            }
        }

        private bool _isLoaded;
        public bool IsLoaded 
        {
            get => _isLoaded; set 
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        #region Command

        private RelayCommand _curseforgeCommand;
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

        private RelayCommand _updateCommand;
        public RelayCommand UpdateCommand
        {
            get => _updateCommand ?? (new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;
                Lexplosion.Run.TaskRun(instanceAddon.Update);
            }));
        }

        private RelayCommand _deleteCommand;
        public RelayCommand DeleteCommand
        {
            get => _deleteCommand ?? (new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;
                CurrentAddon.Uninstall(instanceAddon);
            }));
        }

        private RelayCommand _openMarket;
        public RelayCommand OpenMarket
        {
            get => _openMarket ?? (new RelayCommand(obj =>
            {
                MainViewModel.NavigationStore.PrevViewModel = MainViewModel.NavigationStore.CurrentViewModel;
                MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(CurrentAddon.InstalledAddons, _mainViewModel, _instanceClient, ((FactoryDLCModel)obj).Type);
            }));
        }

        private RelayCommand _openAddonsFolder;
        public RelayCommand OpenAddonsFolder 
        {
            get => _openAddonsFolder ?? new RelayCommand(obj => 
            {
                System.Diagnostics.Process.Start("explorer", GetAddonsFolder());
            });
        }

        #endregion

        public FactoryDLCVM(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            IsLoaded = false;
            _mainViewModel = mainViewModel;
            _instanceClient = instanceClient;
            
            Lexplosion.Run.TaskRun(() =>
            {
                _models.Add(new FactoryDLCModel(InstanceAddon.GetInstalledMods(instanceClient.GetBaseData), AddonType.Mods));
                _models.Add(new FactoryDLCModel(InstanceAddon.GetInstalledResourcepacks(instanceClient.GetBaseData), AddonType.Resourcepacks));
                _models.Add(new FactoryDLCModel(InstanceAddon.GetInstalledWorlds(instanceClient.GetBaseData), AddonType.Maps));

                CurrentAddon = _models[0];
                IsLoaded = true;
            });
        }

        public void ChangeCurrentAddonModel(int index) => CurrentAddon = _models[index];

        public string GetAddonsFolder() => _instanceClient.GetDirectoryPath();
    }
}