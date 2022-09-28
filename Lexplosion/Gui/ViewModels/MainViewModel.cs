using Lexplosion.Controls;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.Tools.Immutable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lexplosion.Gui.ViewModels
{
    public sealed class MainViewModel : VMBase
    {
        #region Static Properties and Fields


        public static readonly NavigationStore NavigationStore = new NavigationStore();

        public MainMenuViewModel MainMenuVM { get; private set; }


        public InstanceFormModel RunningInstance = null;

        private bool _isInstanceRunning = false;
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
            ShowToastMessage(header, message, state, null);
        }

        public static void ShowToastMessage(string header, string message)
        {
            ShowToastMessage(header, message, ToastMessageState.Notification, null);
        }

        public static void ShowToastMessage(string header, string message, TimeSpan? time = null, ToastMessageState state = ToastMessageState.Notification)
        {
            ShowToastMessage(header, message, state, time);
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
        public UserProfile UserProfile { get; } = new UserProfile();
        public ModalWindowViewModel ModalWindowVM { get; } = new ModalWindowViewModel();

        public DownloadManagerViewModel DownloadManager;


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
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }));
        }

        #endregion Command


        #region Constructors


        public MainViewModel()
        {
            PreLoadGameVersions();

            Model = new MainModel();

            LibraryInstanceLoading();

            MainMenuVM = new MainMenuViewModel(this);

            NavigationStore.CurrentViewModel = new AuthViewModel(this);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            ExportViewModel = new ExportViewModel(this);

            DownloadManager = new DownloadManagerViewModel(this);
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
                    NativeMethods.ShowWindow(Runtime.CurrentProcess.MainWindowHandle, 1);
                    NativeMethods.SetForegroundWindow(Runtime.CurrentProcess.MainWindowHandle);
                }
            };
        }


        #endregion Public & Protected Methods


        #region Private Methods


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
            Console.WriteLine("\n-----Library Instance Loading-----");
            foreach (var instanceClient in InstanceClient.GetInstalledInstances())
            {
                Console.WriteLine("Instance [" + instanceClient.Name + "] loaded.");
                Model.LibraryInstances.Add(new InstanceFormViewModel(this, instanceClient));
            }
            Console.WriteLine("\n");
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


        #endregion Private Methods
    }
}