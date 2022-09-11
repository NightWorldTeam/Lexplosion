using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Curseforge;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public sealed class CurseforgeMarketViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;

        private readonly BaseInstanceData _baseInstanceData;

        private readonly ObservableCollection<InstanceAddon> _instanceAddons;

        private readonly CfProjectType _addonsType;

        private int _pageSize = 20;


        #region Commands

        /// <summary>
        /// Закрывает CurseforgeMarket
        /// </summary>
        private RelayCommand _closePage;
        public RelayCommand ClosePageCommand
        {
            get => _closePage ?? (new RelayCommand(obj =>
            {
                _mainViewModel.UserProfile.IsShowInfoBar = true;
                InstanceAddon.ClearAddonsListCache();
                MainViewModel.NavigationStore.CurrentViewModel = MainViewModel.NavigationStore.PrevViewModel;
            }));
        }

        /// <summary>
        /// Переводит пользователя на официальную страницу curseforge.
        /// </summary>
        public RelayCommand GoToCurseforgeCommand
        {
            get => new RelayCommand(obj =>
            {
                var link = (string)obj;

                try
                {
                    System.Diagnostics.Process.Start(link);
                }
                catch
                {
                    // message box here.
                }

            });
        }

        /// <summary>
        /// Вызывает установку мода.
        /// </summary>
        public RelayCommand InstallModCommand
        {
            get => new RelayCommand(obj =>
            {
                var instanceAddon = (InstanceAddon)obj;

                Lexplosion.Run.TaskRun(delegate
                {
                    DownloadAddonRes result = instanceAddon.InstallLatestVersion(out Dictionary<string, DownloadAddonRes> dependenciesResults);
                    App.Current.Dispatcher.Invoke(() => 
                    {
                        if (result == DownloadAddonRes.Successful)
                        {
                            _instanceAddons.Add(instanceAddon);
                            MainViewModel.ShowToastMessage("Мод успешно установлен. Не за что.", "Название: " + instanceAddon.Name, TimeSpan.FromSeconds(5d));
                        }
                        else
                        {
                            MainViewModel.ShowToastMessage("Извиняемся, не удалось установить мод", 
                                "Название: " + instanceAddon.Name + ".\nОшибка " + result, Controls.ToastMessageState.Error);
                        }

                        if (dependenciesResults != null)
                        {
                            foreach (string key in dependenciesResults.Keys)
                            {
                                DownloadAddonRes res = dependenciesResults[key];
                                if (res == DownloadAddonRes.Successful)
                                {
                                    _instanceAddons.Add(instanceAddon);
                                    MainViewModel.ShowToastMessage("Зависимый мод успешно установлен", 
                                        "Название: " + key + ".\nНеобходим для " + instanceAddon.Name);
                                }
                                else
                                {
                                    MainViewModel.ShowToastMessage("Извиняемся, не удалось установить мод",
                                        "Название: " + instanceAddon.Name + ".\nОшибка " + result + ".\nНеобходим для " + instanceAddon.Name, Controls.ToastMessageState.Error);
                                }
                            }
                        }
                    });
                });
                
            });
        }

        #endregion Commands


        #region Properties 

        public ObservableCollection<CurseforgeCategory> ModCategories { get; } = new ObservableCollection<CurseforgeCategory>();
        public ObservableCollection<InstanceAddon> InstanceAddons { get; }

        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel();
        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();

        private bool _isLoaded;
        public bool IsLoaded 
        {
            get => _isLoaded; set 
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyList;
        /// <summary>
        /// <para>Отвечает на вопрос количество найденого контента равно 0?</para>
        /// </summary>
        public bool IsEmptyList
        {
            get => _isEmptyList; set
            {
                _isEmptyList = value;
                OnPropertyChanged();
            }
        }

        private bool _isPaginatorVisible = false;
        public bool IsPaginatorVisible
        {
            get => _isPaginatorVisible; set
            {
                _isPaginatorVisible = value;
                OnPropertyChanged();
            }
        }

        private CurseforgeCategory _selectedAddonCategory;
        public CurseforgeCategory SelectedAddonCategory
        {
            get => _selectedAddonCategory; set 
            {
                _selectedAddonCategory = value;
                OnPropertyChanged();
                GetInitializeInstance();
            }
        }

        #endregion Properties


        public CurseforgeMarketViewModel(ObservableCollection<InstanceAddon> installedAddons, MainViewModel mainViewModel, InstanceClient instanceClient, CfProjectType addonsType)
        {
            _instanceAddons = installedAddons;
            _mainViewModel = mainViewModel;
            _addonsType = addonsType;
            mainViewModel.UserProfile.IsShowInfoBar = false;

            _baseInstanceData = instanceClient.GetBaseData;

            ModCategories = new ObservableCollection<CurseforgeCategory>(CurseforgeApi.GetCategories(_addonsType));

            SelectedAddonCategory = ModCategories[0];

            InstanceAddons = new ObservableCollection<InstanceAddon>();

            SearchBoxVM.SearchChanged += GetInitializeInstance;
            PaginatorVM.PageChanged += GetInitializeInstance;
            GetInitializeInstance();
        }

        public async void GetInitializeInstance()
        {
            await Task.Run(() => InstancesPageLoading());
        }

        private void InstancesPageLoading()
        {
            IsLoaded = false;

            var addonType = (AddonType)Enum.Parse(typeof(AddonType), _addonsType.ToString());

            Lexplosion.Run.TaskRun(delegate ()
            {
                var instances = InstanceAddon.GetAddonsCatalog(
                    _baseInstanceData, 
                    _pageSize, 
                    PaginatorVM.PageIndex - 1,
                    addonType, 
                    SelectedAddonCategory.id, 
                    SearchBoxVM.SearchTextComfirmed);

                if (instances.Count == _pageSize)
                {
                    IsPaginatorVisible = true;
                }
                else IsPaginatorVisible = false;

                if (instances.Count == 0)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceAddons.Clear();
                        IsEmptyList = true;
                    });
                }
                else
                {
                    if (IsEmptyList)
                        IsEmptyList = false;

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceAddons.Clear();
                        Console.WriteLine("Начало загрузки модов");
                        foreach (var instance in instances)
                        {
                            InstanceAddons.Add(instance);
                        }
                        Console.WriteLine("Конец");
                    });
                }

                IsLoaded = true;
            });
        }
    }
}
