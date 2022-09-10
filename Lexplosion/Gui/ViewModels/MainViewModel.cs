using Lexplosion.Controls;
using Lexplosion.Gui.Models;
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

        public static MainMenuViewModel MainMenuVM { get; private set; }


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

        /// <summary>
        /// Данное свойство содержит в себе версии игры.
        /// Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.
        /// </summary>
        public static ImmutableArray<string> GameVersions { get; private set; }

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
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => Run.Exit()));
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


        #region methods

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
        /// После данные заносятся в статичный неизменяемый лист [ImmutableList] - GameVersions. 
        /// </summary>
        private void PreLoadGameVersions()
        {
            var versions = new List<string>();
            Lexplosion.Run.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release") versions.Add(v.id);
                }
                GameVersions = new ImmutableArray<string>(versions);
                versions.Clear();
            });

        }

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

                    MainViewModel.MainMenuVM.LogoClickCommand.Execute(viewModel);
                    NativeMethods.ShowWindow(Run.CurrentProcess.MainWindowHandle, 1);
                    NativeMethods.SetForegroundWindow(Run.CurrentProcess.MainWindowHandle);
                }
            };
        }

        #endregion
    }
}