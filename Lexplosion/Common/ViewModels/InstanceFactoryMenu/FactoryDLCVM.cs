using Lexplosion.Common.Models.InstanceFactory;
using Lexplosion.Controls;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.Common.ViewModels.FactoryMenu
{

    public sealed class FactoryDLCVM : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceClient _instanceClient;


        private readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };


        #region Properties


        private ObservableCollection<FactoryDLCModel> _models = new ObservableCollection<FactoryDLCModel>();
        public ObservableCollection<Tab<FactoryDLCVM>> AddonTabs { get; set; } = new ObservableCollection<Tab<FactoryDLCVM>>();

        private FactoryDLCModel _currentAddon;
        public FactoryDLCModel CurrentAddonModel
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


        // TODO: GUI пеименовать на Website command
        private RelayCommand _webpageCommand;
        public RelayCommand WebsiteCommand
        {
            get => _webpageCommand ?? (_webpageCommand = new RelayCommand(obj =>
            {
                if (obj != null)
                {
                    System.Diagnostics.Process.Start((string)obj);
                }
                else
                {
                    _doNotification("Link = null", ResourceGetter.GetString("noUrl"), 8, 1);
                }
            }));
        }

        private RelayCommand _updateCommand;
        public RelayCommand UpdateCommand
        {
            get => _updateCommand ?? (_updateCommand = new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;
                Lexplosion.Runtime.TaskRun(delegate ()
                {
                    instanceAddon.Update();
                });
            }));
        }

        private RelayCommand _uninstallAddonCommand;
        public RelayCommand UninstallAddonCommand
        {
            get => _uninstallAddonCommand ?? (_uninstallAddonCommand = new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;
                CurrentAddonModel.Uninstall(instanceAddon);
            }));
        }

        private RelayCommand _openMarket;
        public RelayCommand OpenMarket
        {
            get => _openMarket ?? (_openMarket = new RelayCommand(obj =>
            {
                MainViewModel.NavigationStore.PrevViewModel = MainViewModel.NavigationStore.CurrentViewModel;
                MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(
                    _mainViewModel,
                    _instanceClient,
                    ((FactoryDLCModel)obj).Type,
                    CurrentAddonModel,
                    this,
                    _doNotification
                );
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


        private bool _isRefreshingDLC;
        public bool IsRefreshingDLC
        {
            get => _isRefreshingDLC; private set 
            {
                _isRefreshingDLC = value;
                Runtime.DebugWrite(value);
                OnPropertyChanged();
            }
        }


        private RelayCommand _refreshDLCDataCommand;
        public RelayCommand RefreshDLCDataCommand 
        {
            get => _refreshDLCDataCommand ?? (_refreshDLCDataCommand = new RelayCommand(obj => 
            {
                if (IsRefreshingDLC) 
                {
                    return;
                }

                var model = (FactoryDLCModel)obj;

                IsRefreshingDLC = true;
                Lexplosion.Runtime.TaskRun(() =>
                {
                    switch (model.Type)
                    {
                        case CfProjectType.Mods:
                            {
                                var addons = InstanceAddon.GetInstalledMods(_instanceClient.GetBaseData);
                                App.Current.Dispatcher.Invoke(new Action(() => 
                                { 
                                    model.InstalledAddons = new ObservableCollection<InstanceAddon>(addons);
                                    IsRefreshingDLC = false;
                                }));
                            }
                            break;
                        case CfProjectType.Resourcepacks:
                            {
                                var addons = InstanceAddon.GetInstalledResourcepacks(_instanceClient.GetBaseData);
                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    model.InstalledAddons = new ObservableCollection<InstanceAddon>(addons);
                                    IsRefreshingDLC = false;
                                }));
                            }
                            break;
                        case CfProjectType.Maps:
                            {
                                var addons = InstanceAddon.GetInstalledWorlds(_instanceClient.GetBaseData);
                                App.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    model.InstalledAddons = new ObservableCollection<InstanceAddon>(addons);
                                    IsRefreshingDLC = false;
                                }));
                            }
                            break;
                    }
                });
            })); 
        }


        private void RefreshDLCData(FactoryDLCModel model) 
        {
            
        }


        #endregion Command


        #region Constructors


        public FactoryDLCVM(MainViewModel mainViewModel, InstanceClient instanceClient, DoNotificationCallback doNotification = null)
        {
            // Сделал динамическое обновление
            _doNotification = doNotification ?? _doNotification;

            IsLoaded = false;
            _mainViewModel = mainViewModel;
            _instanceClient = instanceClient;

            Lexplosion.Runtime.TaskRun(() =>
            {
                IsVanillaGameType = _instanceClient.GetBaseData.Modloader == ClientType.Vanilla;

                if (!IsVanillaGameType)
                {
                    // моды
                    _models.Add(
                        new FactoryDLCModel(
                            InstanceAddon.GetInstalledMods(instanceClient.GetBaseData),
                            CfProjectType.Mods,
                            ResourceGetter.GetString("noInstalledModification")
                            )
                        );

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AddonTabs.Add(new Tab<FactoryDLCVM>() { Header = ResourceGetter.GetString("mods"), Content = this });
                    });
                }

                // ресурспаки
                _models.Add(
                    new FactoryDLCModel(
                        InstanceAddon.GetInstalledResourcepacks(instanceClient.GetBaseData),
                        CfProjectType.Resourcepacks,
                        ResourceGetter.GetString("noInstalledResourcepacks")
                        )
                    );

                // карты
                _models.Add(
                    new FactoryDLCModel(
                        InstanceAddon.GetInstalledWorlds(instanceClient.GetBaseData),
                        CfProjectType.Maps,
                        ResourceGetter.GetString("noInstalledMaps")
                        )
                    );

                App.Current.Dispatcher.Invoke(() =>
                {
                    AddonTabs.Add(new Tab<FactoryDLCVM>() { Header = ResourceGetter.GetString("resourcepacks"), Content = this });
                    AddonTabs.Add(new Tab<FactoryDLCVM>() { Header = ResourceGetter.GetString("worlds"), Content = this });

                    SelectedAddonIndex = 0;

                    IsLoaded = true;
                });
            });
        }


        #endregion Construtors


        #region Public & Protected Methods


        public void ChangeCurrentAddonModel(int index) => CurrentAddonModel = _models[index];

        public string GetAddonsFolder() => _instanceClient.GetDirectoryPath();


        #endregion Public & Protected Methods
    }
}