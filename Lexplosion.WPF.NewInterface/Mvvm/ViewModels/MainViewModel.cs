using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System.Collections.Generic;
using System.Windows.Media;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Mvvm.Models;
using System;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory;
using Lexplosion.Logic;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System.Windows.Forms;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        public static event Action AllVersionsLoaded;


        public AppCore AppCore { get; private set; }

        private readonly MainMenuLayoutViewModel _mainMenuLayoutViewModel;


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

        internal ModalNavigationStore ModalNavigationStore { get => AppCore.ModalNavigationStore; }

        /// <summary>
        /// Выбранный в данный момент viewmodel для модального окна.
        /// </summary>
        public IModalViewModel CurrentModalViewModel => ModalNavigationStore.CurrentViewModel;

        /// <summary>
        /// Открыто ли модальное окно.
        /// </summary>
        public bool IsModalOpen { get => ModalNavigationStore.CurrentViewModel != null; }


        public NavigateCommand<ViewModelBase> ToMainMenu { get; }


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

        public MainViewModel(AppCore appCore)
        {
            AppCore = appCore;
            Model = new MainModel(appCore);

            SubscribeToOpenModpackEvent();

            NavigationStore = appCore.NavigationStore;

            // так как грузится в отдельном потоке, может загрузится позже чем создатся экземпляр класса InstanceFactory!!!
            ModalNavigationStore.CurrentViewModelChanged += Instance_CurrentViewModelChanged;
            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;


            // Register Modal Window Contents
            ModalNavigationStore.RegisterAbstractFactory(
                typeof(InstanceFactoryViewModel), 
                new ModalInstanceCreatorFactory(appCore, Model.LibraryController as LibraryController, Model.InstanceSharesController)
            );

            _mainMenuLayoutViewModel = new MainMenuLayoutViewModel(appCore, NavigationStore, ModalNavigationStore, Model);
            ToMainMenu = new NavigateCommand<ViewModelBase>(NavigationStore, () => _mainMenuLayoutViewModel);
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
                var versionsList = CoreServicesManager.MinecraftInfo.GetVersionsList();
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


        private void SubscribeToOpenModpackEvent()
        {
            CommandReceiver.OpenModpackPage += delegate (string modpackId)
            {
                InstanceClient instanceClient = InstanceClient.GetInstance(InstanceSource.Nightworld, modpackId);
                if (instanceClient != null)
                {
                    InstanceModelBase viewModel = Model.LibraryController.Instances.FirstOrDefault(i => i.CheckInstanceClient(instanceClient));
                    if (viewModel == null)
                    {
                        viewModel = Model.CatalogController.Instances.FirstOrDefault(i => i.CheckInstanceClient(instanceClient));

                        if (viewModel == null) 
                        {
                            viewModel = new InstanceModelBase(AppCore, instanceClient, Model.Export);
                        }
                    }
                    _mainMenuLayoutViewModel.ToInstanceProfile(viewModel);
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                }
            };
        }


        #endregion Private Methods
    }
}
