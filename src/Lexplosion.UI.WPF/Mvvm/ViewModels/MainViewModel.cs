﻿using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Models;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using Lexplosion.UI.WPF.Mvvm.ViewModels.ModalFactory;
using Lexplosion.UI.WPF.Mvvm.Views.Windows;
using Lexplosion.UI.WPF.Stores;
using Lexplosion.UI.WPF.TrayMenu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels
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

            Model = new MainModel(appCore, _clientsManager);

            SubscribeToOpenModpackEvent();

            NavigationStore = appCore.NavigationStore;

            // так как грузится в отдельном потоке, может загрузится позже чем создатся экземпляр класса InstanceFactory!!!
            ModalNavigationStore.CurrentViewModelChanged += Instance_CurrentViewModelChanged;
            NavigationStore.CurrentViewModelChanged += NavigationStore_CurrentViewModelChanged;

            _mainMenuLayoutViewModel = new MainMenuLayoutViewModel(appCore, Model, _clientsManager);
            Model.SetToAuthorization(_mainMenuLayoutViewModel.OpenAccountFactory);

            // Register Modal Window Contents
            ModalNavigationStore.RegisterAbstractFactory(
                typeof(InstanceFactoryViewModel),
                new ModalInstanceCreatorFactory(appCore,
                    Model.StartImport,
                    Model.GetActiveImports,
                    Model.LibraryController as LibraryController,
                    Model.InstanceSharesController,
                    _mainMenuLayoutViewModel.OpenAccountFactory));

            ToMainMenu = new NavigateCommand<ViewModelBase>(NavigationStore, () => _mainMenuLayoutViewModel);
            //ToMainMenu = new NavigateCommand<ViewModelBase>(NavigationStore, () => new ProfileLayoutViewModel(appCore));

            InitTrayComponents();
            RuntimeApp.TrayMenuElementClicked += InitTrayComponents;
            // Обновляем элементы меню, так как ContextMenu находится в другом визуальном дереве
            // и через DynamicResource обновление просто не сделать
            RuntimeApp.TrayContextMenuOpened += InitTrayComponents;

            //ModalNavigationStore.Open(new ErrorViewerViewModel(["test/test.org not found", "test-pc/test-pc.org not found"]));
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
                            var args = new InstanceModelArgs(AppCore, instanceClient, Model.Export, Model.SetRunningGame);
                            viewModel = new InstanceModelBase(args);
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

                TrayComponents.Add(new TrayButton(1, (string)resources["TrayHideLauncher"], RuntimeApp.CloseMainWindow)
                {
                    IsEnabled = App.Current.MainWindow?.GetType() == typeof(MainWindow)
                });
                TrayComponents.Add(new TrayButton(2, (string)resources["MaximizeLauncher"], RuntimeApp.ShowMainWindow)
                {
                    IsEnabled = App.Current.MainWindow?.GetType() != typeof(MainWindow)
                });
                TrayComponents.Add(new TrayButton(3, (string)resources["TrayReloadMultiplayer"], LaunchGame.RebootOnlineGame)
                {
                    IsEnabled = Account.ActiveAccount != null && Account.ActiveAccount.AccountType == AccountType.NightWorld
                });
                TrayComponents.Add(new TrayButton(4, (string)resources["ContactSupport"], ContentSupport)
                {
                    IsEnabled = true
                });
                TrayComponents.Add(new TrayButton(5, (string)resources["Close"], Runtime.KillApp)
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
