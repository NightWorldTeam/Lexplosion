using Lexplosion.Gui.Models.InstanceFactory;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public sealed class FactoryDLCVM : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;


        #region Properties


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

        private bool _isVanillaGameType = true; 
        public bool IsVanillaGameType 
        {
            get => _isVanillaGameType; set 
            {
                _isVanillaGameType = value; 
                OnPropertyChanged();
            }
        }

        #endregion Properties


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
                Lexplosion.Run.TaskRun(delegate ()
                {
                    instanceAddon.Update();
                });
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
                MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(_mainViewModel, _instanceClient, ((FactoryDLCModel)obj).Type, CurrentAddon.InstalledAddons);
            }));
        }

        private RelayCommand _openAddonsFolder;
        public RelayCommand OpenAddonsFolder
        {
            get => _openAddonsFolder ?? (_openAddonsFolder = new RelayCommand(obj =>
            {
                System.Diagnostics.Process.Start("explorer", GetAddonsFolder());
            }));
        }

        #endregion


        #region Constructors


        public FactoryDLCVM(MainViewModel mainViewModel, InstanceClient instanceClient)
        {
            IsLoaded = false;
            _mainViewModel = mainViewModel;
            _instanceClient = instanceClient;

            Lexplosion.Run.TaskRun(() =>
            {
                IsVanillaGameType = _instanceClient.GetBaseData.Modloader == ModloaderType.Vanilla;

                _models.Add(new FactoryDLCModel(InstanceAddon.GetInstalledResourcepacks(instanceClient.GetBaseData), CfProjectType.Resourcepacks));
                _models.Add(new FactoryDLCModel(InstanceAddon.GetInstalledWorlds(instanceClient.GetBaseData), CfProjectType.Maps));

                if (!IsVanillaGameType)
                {
                    _models.Add(new FactoryDLCModel(InstanceAddon.GetInstalledMods(instanceClient.GetBaseData), CfProjectType.Mods));
                    SelectedAddonIndex = 1;
                }
                else 
                {

                    SelectedAddonIndex = 1;
                }

                IsLoaded = true;
            });
        }

        #endregion Construtors


        #region Public & Protected Methods


        public void ChangeCurrentAddonModel(int index) => CurrentAddon = _models[index];

        public string GetAddonsFolder() => _instanceClient.GetDirectoryPath();


        #endregion Public & Protected Methods
    }
}