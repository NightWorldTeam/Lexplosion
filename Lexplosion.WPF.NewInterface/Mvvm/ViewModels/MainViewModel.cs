using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System.Collections.Generic;
using System.Windows.Media;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Mvvm.Models;
using System;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
{
    public abstract class ModalAbstractFactory 
    {
        public abstract IModalViewModel Create();
    }

    public sealed class ModalInstanceCreatorFactory : ModalAbstractFactory
    {
        private readonly Action<InstanceClient> _addToLibrary;
        private readonly Action<InstanceClient> _removeFromLibrary;

        public ModalInstanceCreatorFactory(Action<InstanceClient> addToLibrary, Action<InstanceClient> removeFromLibrary)
        {
            _addToLibrary = addToLibrary;
            _removeFromLibrary = removeFromLibrary;
        }

        public override IModalViewModel Create()
        {
            return new LeftMenuControl(
                new ModalLeftMenuTabItem[3]
                {
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "AddCircle",
                        TitleKey = "Create",
                        IsEnable = true,
                        IsSelected = true,
                        Content = new InstanceFactoryViewModel(_addToLibrary)
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "PlaceItem",
                        TitleKey = "Import",
                        IsEnable = true,
                        IsSelected = false,
                        Content = new InstanceImportViewModel(_addToLibrary, _removeFromLibrary)
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "DownloadCloud",
                        TitleKey = "Distributions",
                        IsEnable = true,
                        IsSelected = false
                    }
                }
                );
        }
    }


    public sealed class MainViewModel : VMBase
    {
        public static event Action AllVersionsLoaded;


        #region Properties


        public MainModel Model { get; }

        /// <summary>
        /// Навигационное хранилище, хранит активный viewmodel для окна.
        /// </summary>
        internal INavigationStore NavigationStore { get; } = new NavigationStore();

        /// <summary>
        /// Выбранный в данный момент viewmodel для окна. 
        /// </summary>
        public ViewModelBase CurrentViewModel => NavigationStore.CurrentViewModel;

        internal ModalNavigationStore ModalNavigationStore { get => ModalNavigationStore.Instance; }

        /// <summary>
        /// Выбранный в данный момент viewmodel для модального окна.
        /// </summary>
        public IModalViewModel CurrentModalViewModel => ModalNavigationStore.CurrentViewModel;

        /// <summary>
        /// Открыто ли модальное окно.
        /// </summary>
        public bool IsModalOpen { get => ModalNavigationStore.CurrentViewModel != null; }



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


        #region Constructors


        static MainViewModel()
        {
            PreLoadGameVersions();
        }

        public MainViewModel()
        {
            Model = new MainModel();
            OnPropertyChanged(nameof(Model));
            // так как грузится в отдельном потоке, может загрузится позже чем создатся экземпляр класса InstanceFactory!!!
            ModalNavigationStore.Instance.CurrentViewModelChanged += Instance_CurrentViewModelChanged;
            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;


            // Register Modal Window Contents
            ModalNavigationStore.Instance.RegisterAbstractFactory(typeof(InstanceFactoryViewModel), new ModalInstanceCreatorFactory(Model.LibraryController.Add, Model.LibraryController.Remove));


            //ModalNavigationStore.Close();
            //ModalNavigationStore.Open(new DialogBoxViewModel("Library", "Protection", (obj) => { }, (obj) => { }));
            //ModalNavigationStore.Close();

            var mainMenuLayout = new MainMenuLayoutViewModel(NavigationStore, ModalNavigationStore, Model);
            var toMainMenu = new NavigateCommand<ViewModelBase>(NavigationStore, () => mainMenuLayout);
            toMainMenu?.Execute(null);
            //var toAuthForms = new NavigateCommand<ViewModelBase>(NavigationStore, () => new AuthorizationMenuViewModel(NavigationStore, toMainMenu));

            //toAuthForms.Execute(null);

            //NavigationStore.CurrentViewModel = new ModrinthAddonPageViewModel(null);
            //NavigationStore.CurrentViewModel = new CurseforgeRepositoryViewModel(InstanceClient.GetInstalledInstances()[0].GetBaseData);
            //new MainMenuLayoutViewModel(NavigationStore); 
            //NavigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(null, null, LibraryController.Instance.Instances.Last());
            //new InstanceModelBase(InstanceClient.GetOutsideInstances( InstanceSource.Modrinth, 2, 0, new IProjectCategory[] { new SimpleCategory() { Name = "All", Id = "-1", ClassId = "", ParentCategoryId = "" }}, "", CfSortField.Featured, "1.19.4")[1])); //new MainMenuLayoutViewModel(); //new ModrinthRepositoryViewModel(AddonType.Mods, ClientType.Fabric, "1.19.4");
            //NavigationStore.Content = new AuthorizationMenuViewModel(NavigationStore);
        }


        #endregion Constructors


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

                AllVersionsLoaded?.Invoke();
            });
        }


        #endregion Private Methods
    }
}
