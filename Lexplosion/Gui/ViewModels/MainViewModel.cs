using Lexplosion.Controls;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Gui.TrayMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.Tools.Immutable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Lexplosion.Logic.Management;
using System.Diagnostics;
using Lexplosion.Global;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class MainViewModel : VMBase
    {
        #region Static Properties and Fields


        public static readonly NavigationStore NavigationStore = new NavigationStore();

        public MainMenuViewModel MainMenuVM { get; private set; }

        private InstanceFormViewModel _runningInstance;
        public InstanceFormViewModel RunningInstance 
        {
            get => _runningInstance; set
            {
                _runningInstance = value;
                OnPropertyChanged();
            }
        }

        private static bool _isInstanceRunning = false;
        /// <summary>
        /// Если запушена сборка true, иначе else.
        /// </summary>
        public bool IsInstanceRunning
        {
            get => _isInstanceRunning; set
            {
                _isInstanceRunning = value;
                OnPropertyChanged();
            }
        }

        private ImmutableArray<string> _releaseGameVersions;
        /// <summary>
        /// Данное свойство содержит в себе версии игры.
        /// Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.
        /// </summary>
        public ImmutableArray<string> ReleaseGameVersions 
        {   
            get => _releaseGameVersions; private set 
            {
                _releaseGameVersions = value;
                OnPropertyChanged();
            }
        }
        public static ImmutableArray<string> AllGameVersions { get; private set; }

        /// <summary>
        /// Выведенные сообщения.
        /// </summary>
        public static ObservableCollection<MessageModel> Messages { get; } = new ObservableCollection<MessageModel>();

        public static List<ExportViewModel> ExportedInstance { get; } = new List<ExportViewModel>();

        #endregion Static Properties and Fields


        #region ShowToastMessage Methods
        // вынести в отдельный класс

        public static void ShowToastMessage(string header, string message, ToastMessageState state = ToastMessageState.Notification)
        {
            if ((bool)!UserData.GeneralSettings.HiddenMode || ((bool)UserData.GeneralSettings.HiddenMode && !_isInstanceRunning)) 
            { 
                ShowToastMessage(header, message, state, null);
            }
        }

        public static void ShowToastMessage(string header, string message)
        {
            if ((bool)!UserData.GeneralSettings.HiddenMode || ((bool)UserData.GeneralSettings.HiddenMode && !_isInstanceRunning))
            {
                ShowToastMessage(header, message, ToastMessageState.Notification, null);
            }
        }

        public static void ShowToastMessage(string header, string message, TimeSpan? time = null, ToastMessageState state = ToastMessageState.Notification)
        {
            if ((bool)!UserData.GeneralSettings.HiddenMode || ((bool)UserData.GeneralSettings.HiddenMode && !_isInstanceRunning))
            {
                ShowToastMessage(header, message, state, time);
            }
        }

        private static void ShowToastMessage(string header, string message, ToastMessageState state, TimeSpan? time)
        {
            var model = new ToastMessageModel(header, message, state, time);
            App.Current.Dispatcher.Invoke(() => {
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


        public MainModel Model { get; }
        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;
        public ExportViewModel ExportViewModel { get; set; }
        public LoadingBoard LoadingBoard { get; } = new LoadingBoard();
        public UserProfile UserProfile { get; }
        public ModalWindowViewModel ModalWindowVM { get; } = new ModalWindowViewModel();

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
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => Runtime.Exit()));
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

        #endregion Command


        #region Constructors


        public MainViewModel()
        {
            PreLoadGameVersions();

            Model = new MainModel();
            UserProfile = new UserProfile(InitTrayComponents);
            LibraryInstanceLoading();

            NavigationStore.CurrentViewModel = new AuthViewModel(this);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            
            ExportViewModel = new ExportViewModel(this);

            DownloadManager = new DownloadManagerViewModel(this);

            Runtime.TrayMenuElementClicked += InitTrayComponents;

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

                    if (Model.IsLibraryContainsInstance(instanceClient))
                    {
                        viewModel = Model.GetInstance(instanceClient);
                    }
                    else if (Model.IsCatalogInstanceContains(instanceClient))
                    {
                        viewModel = Model.GetCatalogInstance(instanceClient);
                    }
                    else
                    {
                        viewModel = new InstanceFormViewModel(this, instanceClient);
                    }

                    MainMenuVM.OpenModpackPage(viewModel);
                    NativeMethods.ShowProcessWindows(Runtime.CurrentProcess.MainWindowHandle);
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

        internal void InitTrayComponents(InstanceFormViewModel instanceFormViewModel) 
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TrayComponents.Clear();

                if (instanceFormViewModel != null)
                    TrayComponents.Add(new TrayButton(0, "Закрыть игру", ResourceGetter.GetString("ExtensionOff"), instanceFormViewModel.CloseInstance) { IsEnable = IsInstanceRunning });

                TrayComponents.Add(new TrayButton(1, "Свернуть лаунчер", ResourceGetter.GetString("OpenFull"), Runtime.CloseMainWindow) { IsEnable = App.Current.MainWindow != null });
                TrayComponents.Add(new TrayButton(2, "Развернуть лаунчер", ResourceGetter.GetString("OpenFull"), Runtime.ShowMainWindow) { IsEnable = App.Current.MainWindow == null });
                TrayComponents.Add(new TrayButton(3, "Перезапустить сетевую игру", ResourceGetter.GetString("Refresh"), LaunchGame.RebootOnlineGame) { IsEnable = UserProfile.IsNightWorldAccount });
                TrayComponents.Add(new TrayButton(4, "Связаться с поддержкой", ResourceGetter.GetString("ContactSupport"), ContentSupport) { IsEnable = true });
                TrayComponents.Add(new TrayButton(5, "Закрыть", ResourceGetter.GetString("CloseCycle"), Runtime.KillApp) { IsEnable = true });
            });
        }

        internal void InitTrayComponents() 
        {
            App.Current.Dispatcher.Invoke(() => 
            { 
                TrayComponents.Clear();

                TrayComponents.Add(new TrayButton(1, "Свернуть лаунчер", ResourceGetter.GetString("SubtitlesOff"), Runtime.CloseMainWindow) { IsEnable = App.Current.MainWindow != null });
                TrayComponents.Add(new TrayButton(2, "Развернуть лаунчер", ResourceGetter.GetString("AspectRatio"), Runtime.ShowMainWindow) { IsEnable = App.Current.MainWindow == null });
                TrayComponents.Add(new TrayButton(3, "Перезапустить сетевую игру", ResourceGetter.GetString("Refresh"), LaunchGame.RebootOnlineGame) { IsEnable = UserProfile.IsNightWorldAccount });
                TrayComponents.Add(new TrayButton(4, "Связаться с поддержкой", ResourceGetter.GetString("ContactSupport"),ContentSupport) { IsEnable = true });
                TrayComponents.Add(new TrayButton(5, "Закрыть", ResourceGetter.GetString("CloseCycle"), Runtime.KillApp) { IsEnable = true });
            });
        } 

        // обновляем свойство currentviewmodel
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
                Model.LibraryInstances.Add(new InstanceFormViewModel(this, instanceClient));
            }
        }

        /// <summary>
        /// Метод в отдельном потоке загружает данные о версии игры.
        /// После данные заносятся в статичный неизменяемый лист [ImmutableList] - ReleaseGameVersions. 
        /// </summary>
        private void PreLoadGameVersions()
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
                ReleaseGameVersions = new ImmutableArray<string>(releaseOnlyVersions);
                AllGameVersions = new ImmutableArray<string>(allVersions);
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