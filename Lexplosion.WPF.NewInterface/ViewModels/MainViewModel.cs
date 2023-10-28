using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Models.InstanceCatalogControllers;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.AddonsRepositories;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.ViewModels.Modal;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewModels
{
    public sealed class MainViewModel : VMBase
    {
        #region Properties


        /// <summary>
        /// Навигационное хранилище, хранит активный viewmodel для окна.
        /// </summary>
        internal INavigationStore NavigationStore { get; } = new NavigationStore();

        /// <summary>
        /// Выбранный в данный момент viewmodel для окна. 
        /// </summary>
        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        /// <summary>
        /// Выбранный в данный момент viewmodel для модального окна.
        /// </summary>
        public IModalViewModel CurrentModalViewModel => ModalNavigationStore.Instance.CurrentViewModel;

        /// <summary>
        /// Открыто ли модальное окно.
        /// </summary>
        public bool IsModalOpen { get => ModalNavigationStore.Instance.CurrentViewModel != null; }



        // Данное свойство содержит в себе версии игры.
        // Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.

        /// <summary>
        /// Все версии без снапшотов.
        /// </summary>
        public static MinecraftVersion[] ReleaseGameVersions { get; private set; }

        /// <summary>
        /// Все версии включая снапшоты.
        /// </summary>
        public static MinecraftVersion[] AllGameVersions { get; private set; }


        #endregion Properties

        public MainViewModel()
        {
            PreLoadGameVersions();

            ModalNavigationStore.Instance.CurrentViewModelChanged += Instance_CurrentViewModelChanged;
            ModalNavigationStore.Instance.Open(new LeftMenuControl(
                new ModalLeftMenuTabItem[3]
                {
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "AddCircle",
                        TitleKey = "Create",
                        IsEnable = true,
                        IsSelected = true
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "PlaceItem",
                        TitleKey = "Import",
                        IsEnable = true,
                        IsSelected = true
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "DownloadCloud",
                        TitleKey = "Distributions",
                        IsEnable = true,
                        IsSelected = true
                    }
                }
                ));

            ModalNavigationStore.Instance.Close();
            ModalNavigationStore.Instance.Open(new DialogBoxViewModel("Library", "Protection", (obj) => { }, (obj) => { }));
            ModalNavigationStore.Instance.Close();

            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;
            //NavigationStore.CurrentViewModel = new MainMenuLayoutViewModel(NavigationStore);
            //NavigationStore.CurrentViewModel = new ModrinthAddonPageViewModel(null);
            NavigationStore.CurrentViewModel = new CurseforgeRepositoryViewModel(InstanceClient.GetInstalledInstances()[0].GetBaseData);
            //new MainMenuLayoutViewModel(NavigationStore); 
            //NavigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(LibraryController.Instance.Instances.Last());
                //new InstanceModelBase(InstanceClient.GetOutsideInstances( InstanceSource.Modrinth, 2, 0, new IProjectCategory[] { new SimpleCategory() { Name = "All", Id = "-1", ClassId = "", ParentCategoryId = "" }}, "", CfSortField.Featured, "1.19.4")[1])); //new MainMenuLayoutViewModel(); //new ModrinthRepositoryViewModel(AddonType.Mods, ClientType.Fabric, "1.19.4");
            //NavigationStore.Content = new AuthorizationMenuViewModel(NavigationStore);
        }

        private void Instance_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        private void NavigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public static void ChangeColor(Color color)
        {
        }


        #region Private Methods


        /// <summary>
        /// Метод в отдельном потоке загружает данные о версии игры.
        /// После данные заносятся в статичный неизменяемый лист [ImmutableList] - _releaseGameVersions. 
        /// </summary>
        private static void PreLoadGameVersions()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var versionsList = ToServer.GetVersionsList();
                var releaseOnlyVersions = new List<MinecraftVersion>();
                var allVersions = new MinecraftVersion[versionsList.Count];
                var i = 0;
                foreach (var version in versionsList)
                {
                    if (version.type == "release")
                    {
                        var minecraftVersion = new MinecraftVersion(version.id, MinecraftVersion.VersionType.Release);
                        allVersions[i] = minecraftVersion;
                        releaseOnlyVersions.Add(minecraftVersion);
                    }
                    else
                    {
                        allVersions[i] = new MinecraftVersion(version.id, MinecraftVersion.VersionType.Snapshot);
                    }
                    i++;

                }
                ReleaseGameVersions = releaseOnlyVersions.ToArray();
                AllGameVersions = allVersions;
            });
        }


        #endregion Private Methods
    }
}
