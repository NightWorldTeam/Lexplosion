using Lexplosion.Controls;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Gui.TrayMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Lexplosion.Logic.Management;
using System.Diagnostics;
using Lexplosion.Global;
using System.Windows.Input;
using System.Threading;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class MainViewModel : VMBase
    {
        #region Static Properties and Fields


        /// <summary>
        /// Выведенные сообщения.
        /// </summary>
        public static ObservableCollection<MessageModel> Messages { get; } = new ObservableCollection<MessageModel>();
        public static List<ExportViewModel> ExportedInstance { get; } = new List<ExportViewModel>();
        public static readonly NavigationStore NavigationStore = new NavigationStore();

        /// <summary>
        /// Данное свойство содержит в себе версии игры.
        /// Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.
        /// </summary>

        public static string[] ReleaseGameVersions { get; private set; }
        public static string[] AllGameVersions { get; private set; }

        #endregion Static Properties and Fields


        #region ShowToastMessage Methods
        // вынести в отдельный класс

        public static void ShowToastMessage(string header, string message, uint time, byte type) 
        {
            if ((bool)!GlobalData.GeneralSettings.IsHiddenMode || ((bool)GlobalData.GeneralSettings.IsHiddenMode && !MainModel.Instance.IsInstanceRunning))
            {
                var timeSpan = time > 0 ? TimeSpan.FromSeconds(time) : TimeSpan.MaxValue;
                ShowToastMessage(header, message, timeSpan, (ToastMessageState)type);
            }
        }

        public static void ShowToastMessage(string header, string message, ToastMessageState state = ToastMessageState.Notification)
        {
            if ((bool)!GlobalData.GeneralSettings.IsHiddenMode || ((bool)GlobalData.GeneralSettings.IsHiddenMode && !MainModel.Instance.IsInstanceRunning))
            {
                ShowToastMessage(header, message, state, null);
            }
        }

        public static void ShowToastMessage(string header, string message)
        {
            if ((bool)!GlobalData.GeneralSettings.IsHiddenMode || ((bool)GlobalData.GeneralSettings.IsHiddenMode && !MainModel.Instance.IsInstanceRunning))
            {
                ShowToastMessage(header, message, ToastMessageState.Notification, null);
            }
        }

        public static void ShowToastMessage(string header, string message, TimeSpan? time = null, ToastMessageState state = ToastMessageState.Notification)
        {
            if ((bool)!GlobalData.GeneralSettings.IsHiddenMode || ((bool)GlobalData.GeneralSettings.IsHiddenMode && !MainModel.Instance.IsInstanceRunning))
            {
                ShowToastMessage(header, message, state, time);
            }
        }

        private static void ShowToastMessage(string header, string message, ToastMessageState state, TimeSpan? time)
        {
            var model = new ToastMessageModel(header, message, state, time);
            App.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(model);
            });
        }

        public static void ShowDialogMessage(string header, string message, Action leftButtonCommand, Action rightButtonCommand, string leftButtonContent, string rightButtonContent)
        {
            var model = new DialogMessageModel(header, message, leftButtonCommand, rightButtonCommand, leftButtonContent, rightButtonContent);

            App.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(model);
            });
        }


        #endregion ShowToastMessage methods


        #region Properties


        public ICommand NavigationShowCaseCommand { get; private set; }
        public MainMenuViewModel MainMenuVM { get; private set; }

        private MainModel Model { get => MainModel.Instance; }
        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;
        public ExportViewModel ExportViewModel { get; set; }
        public LoadingBoard LoadingBoard { get; } = new LoadingBoard();
        public UserData UserData { get; }
        public ModalWindowViewModelSingleton ModalWindowVM { get => ModalWindowViewModelSingleton.Instance; }
        public DownloadManagerViewModel DownloadManager;
        public ObservableCollection<TrayCompontent> TrayComponents { get; } = new ObservableCollection<TrayCompontent>();


        #endregion Properties


        #region Commands


        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки x, в Header окна.
        /// Закрывает окно лаунчера (всё приложение).
        /// </summary>
        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand
        {
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj =>
            {
                RuntimeApp.Exit();
                InitTrayComponents();
            }));
        }

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки -, в Header окна.
        /// Сворачивает окно лаунчера.
        /// </summary>
        private RelayCommand _hideCommand;
        public RelayCommand HideCommand
        {
            get => _hideCommand ?? (_hideCommand = new RelayCommand(obj =>
            {
                System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }));
        }

        private RelayCommand _contactSupportCommand;
        public RelayCommand ContactSupportCommand
        {
            get => _contactSupportCommand ?? (_contactSupportCommand = new RelayCommand(obj =>
            {
                MainViewModel.ContentSupport();
            }));
        }

        private RelayCommand _showMainWindowCommand;
        public RelayCommand ShowMainWindowCommand
        {
            get => _showMainWindowCommand ?? (_showMainWindowCommand = new RelayCommand(obj =>
            {
                RuntimeApp.ShowMainWindow();
                InitTrayComponents();
            }));
        }

        #endregion Command


        #region Constructors


        public MainViewModel()
        {
            MainModel.Instance.SetMainViewModel(this);

            PreLoadGameVersions();

            UserData = new UserData(InitTrayComponents);
            LibraryInstanceLoading();

            NavigationStore.CurrentViewModel = new AuthViewModel(this);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            DownloadManager = new DownloadManagerViewModel();

            RuntimeApp.TrayMenuElementClicked += InitTrayComponents;

            InitTrayComponents();
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void SubscribeToOpenModpackEvent()
        {
            CommandReceiver.OpenModpackPage += delegate (string modpackId)
            {
                InstanceClient instanceClient = InstanceClient.GetInstance(InstanceSource.Nightworld, modpackId);
                if (instanceClient != null)
                {
                    InstanceFormViewModel viewModel;

                    if (MainModel.Instance.LibraryController.TryGetInstanceByInstanceClient(instanceClient, out viewModel))
                    {
                    }
                    else if (MainModel.Instance.LibraryController.IsLibraryContainsInstance(instanceClient))
                    {
                        viewModel = MainModel.Instance.CatalogController.GetInstance(instanceClient);
                    }
                    else
                    {
                        viewModel = new InstanceFormViewModel(this, instanceClient);
                    }

                    MainMenuVM.OpenModpackPage(viewModel);
                    NativeMethods.ShowProcessWindows(RuntimeApp.CurrentProcess.MainWindowHandle);
                }
            };
        }

        public static void ContentSupport()
        {
            try
            {
                Process.Start(Constants.VKGroupToChatUrl);
            }
            catch
            { }
        }

        #endregion Public & Protected Methods


        #region Private Methods

        internal void InitTrayComponents()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (Model.RunningInstance == null)
                    InitTrayComponentsWithoutGame();
                else InitTrayComponentsWithGame(Model.RunningInstance);
            });
        }

        internal void InitTrayComponentsWithGame(InstanceFormViewModel instanceFormViewModel)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TrayComponents.Clear();

                if (instanceFormViewModel != null)
                    TrayComponents.Add(new TrayButton(0, ResourceGetter.GetString("closeInstance"), ResourceGetter.GetString("ExtensionOff"), instanceFormViewModel.CloseInstance) { IsEnable = Model.IsInstanceRunning });

                TrayComponents.Add(new TrayButton(1, ResourceGetter.GetString("trayHideLauncher"), ResourceGetter.GetString("SubtitlesOff"), RuntimeApp.CloseMainWindow) { IsEnable = App.Current.MainWindow != null });
                TrayComponents.Add(new TrayButton(2, ResourceGetter.GetString("maximizeLauncher"), ResourceGetter.GetString("AspectRatio"), RuntimeApp.ShowMainWindow) { IsEnable = App.Current.MainWindow == null });
                TrayComponents.Add(new TrayButton(3, ResourceGetter.GetString("rebootOnlineGame"), ResourceGetter.GetString("Refresh"), LaunchGame.RebootOnlineGame) { IsEnable = UserData.IsNightWorldAccount });
                TrayComponents.Add(new TrayButton(4, ResourceGetter.GetString("contactSupport"), ResourceGetter.GetString("ContactSupport"), ContentSupport) { IsEnable = true });
                TrayComponents.Add(new TrayButton(5, ResourceGetter.GetString("close"), ResourceGetter.GetString("CloseCycle"), RuntimeApp.KillApp) { IsEnable = true });
            });
        }

        internal void InitTrayComponentsWithoutGame()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TrayComponents.Clear();

                TrayComponents.Add(new TrayButton(1, ResourceGetter.GetString("trayHideLauncher"), ResourceGetter.GetString("SubtitlesOff"), RuntimeApp.CloseMainWindow) { IsEnable = App.Current.MainWindow != null });
                TrayComponents.Add(new TrayButton(2, ResourceGetter.GetString("maximizeLauncher"), ResourceGetter.GetString("AspectRatio"), RuntimeApp.ShowMainWindow) { IsEnable = App.Current.MainWindow == null });
                TrayComponents.Add(new TrayButton(3, ResourceGetter.GetString("rebootOnlineGame"), ResourceGetter.GetString("Refresh"), LaunchGame.RebootOnlineGame) { IsEnable = UserData.IsNightWorldAccount });
                TrayComponents.Add(new TrayButton(4, ResourceGetter.GetString("contactSupport"), ResourceGetter.GetString("ContactSupport"), ContentSupport) { IsEnable = true });
                TrayComponents.Add(new TrayButton(5, ResourceGetter.GetString("close"), ResourceGetter.GetString("CloseCycle"), RuntimeApp.KillApp) { IsEnable = true });
            });
        }


        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        /// <summary>
        /// Метод загружает сборки для библиотеки.
        /// </summary>
        private void LibraryInstanceLoading()
        {
            foreach (var instanceClient in InstanceClient.GetInstalledInstances())
            {
                MainModel.Instance.LibraryController.AddInstance(new InstanceFormViewModel(this, instanceClient));
            }
        }

        /// <summary>
        /// Метод в отдельном потоке загружает данные о версии игры.
        /// После данные заносятся в статичный неизменяемый лист [ImmutableList] - _releaseGameVersions. 
        /// </summary>
        private static void PreLoadGameVersions()
        {
            var releaseOnlyVersions = new List<string>();
            var allVersions = new List<string>();
            Lexplosion.Runtime.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release")
                    {
                        releaseOnlyVersions.Add(v.id);
                        allVersions.Add("release " + v.id);
                    }
                    else
                    {
                        allVersions.Add("snapshot " + v.id);
                    }

                }
                ReleaseGameVersions = releaseOnlyVersions.ToArray();
                AllGameVersions = allVersions.ToArray();
                releaseOnlyVersions.Clear();
                allVersions.Clear();
            });
        }

        public MainMenuViewModel InitMainMenuViewModel(MainMenuViewModel mainMenuViewModel)
        {
            if (MainMenuVM == null)
                return MainMenuVM = mainMenuViewModel;
            else return MainMenuVM;
        }


        #endregion Private Methods
    }
}