using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System.Collections.Generic;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Mvvm.Models;
using System;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory;
using Lexplosion.Logic;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Linq;
using System.Collections.ObjectModel;
using Lexplosion.WPF.NewInterface.TrayMenu;
using System.Windows.Input;
using Lexplosion.Logic.Management.Accounts;
using System.Diagnostics;
using System.Windows.Controls;
using Lexplosion.WPF.NewInterface.Mvvm.Views.Windows;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        public static event Action AllVersionsLoaded;

        private readonly MainMenuLayoutViewModel _mainMenuLayoutViewModel;
		private readonly ClientsManager _clientsManager = Runtime.ClientsManager;

		public AppCore AppCore { get; private set; }


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


        private GlobalLoadingArgs _globalLoadingArgs;
        public GlobalLoadingArgs GlobalLoadingArgs
        {
            get => _globalLoadingArgs; set 
            {
                _globalLoadingArgs = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _showMainWindowCommand;
        public ICommand ShowMainWindowCommand
        {
            get => RelayCommand.GetCommand(ref _showMainWindowCommand, (obj) =>
            {
                RuntimeApp.ShowMainWindow();
                InitTrayComponents();
            });
        }


        #endregion Commands


        #region Constructors


        static MainViewModel()
        {
            PreLoadGameVersions();
        }

        public MainViewModel(AppCore appCore)
        {
            AppCore = appCore;

            appCore.GlobalLoadingStarted += (val) =>
            {
                GlobalLoadingArgs = val;
            };

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

            InitTrayComponents();
            RuntimeApp.TrayMenuElementClicked += InitTrayComponents;
            // Обновляем элементы меню, так как ContextMenu находится в другом визуальном дереве
            // и через DynamicResource обновление просто не сделать
            RuntimeApp.TrayContextMenuOpened += InitTrayComponents;
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


        #region Private Methods


        /// <summary>
        /// Метод в отдельном потоке загружает данные о версии игры.
        /// После данные заносятся в статичный неизменяемый лист [ImmutableList] - _releaseGameVersions. 
        /// </summary>
        private static void PreLoadGameVersions()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var versionsList = Runtime.ServicesContainer.MinecraftService.GetVersionsList();
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
                InstanceClient instanceClient = _clientsManager.GetInstance(InstanceSource.Nightworld, modpackId);
                if (instanceClient != null)
                {
                    InstanceModelBase viewModel = Model.LibraryController.Instances.FirstOrDefault(i => i.CheckInstanceClient(instanceClient));
                    if (viewModel == null)
                    {
                        viewModel = Model.CatalogController.Instances.FirstOrDefault(i => i.CheckInstanceClient(instanceClient));

                        if (viewModel == null)
                        {
                            viewModel = new InstanceModelBase(AppCore, instanceClient, Model.Export, Model.SetRunningGame);
                        }
                    }
                    _mainMenuLayoutViewModel.ToInstanceProfile(viewModel);
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
                }
            };
        }


        #endregion Private Methods


        internal void InitTrayComponents()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                InitTrayComponentsInternal(Model.RunningGame);
            });
        }

        public ObservableCollection<TrayComponentBase> TrayComponents { get; } = new();

        private void InitTrayComponentsInternal(InstanceModelBase instanceModel)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var resources = AppCore.Resources;

                TrayComponents.Clear();

                if (instanceModel != null)
                {
                    TrayComponents.Add(new TrayButton(0, "CloseInstance", instanceModel.Close)
                    {
                        IsEnabled = Model.RunningGame == null
                    });
                }

                TrayComponents.Add(new TrayButton(1, (string)resources("TrayHideLauncher"), RuntimeApp.CloseMainWindow)
                {
                    IsEnabled = App.Current.MainWindow?.GetType() == typeof(MainWindow)
                });
                TrayComponents.Add(new TrayButton(2, (string)resources("MaximizeLauncher"), RuntimeApp.ShowMainWindow)
                {
                    IsEnabled = App.Current.MainWindow?.GetType() != typeof(MainWindow)
                });
                TrayComponents.Add(new TrayButton(3, (string)resources("TrayReloadMultiplayer"), LaunchGame.RebootOnlineGame)
                {
                    IsEnabled = Account.ActiveAccount != null && Account.ActiveAccount.AccountType == AccountType.NightWorld
                });
                TrayComponents.Add(new TrayButton(4, (string)resources("ContactSupport"), ContentSupport)
                {
                    IsEnabled = true
                });
                TrayComponents.Add(new TrayButton(5, (string)resources("Close"), Runtime.KillApp)
                {
                    IsEnabled = true
                });
            });
        }

        void ContentSupport()
        {
            try
            {
                Process.Start(Constants.VKGroupToChatUrl);
            }
            catch
            {

            }
        }
    }
}
