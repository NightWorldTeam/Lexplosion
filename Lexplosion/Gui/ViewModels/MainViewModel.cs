using Lexplosion.Controls;
using Lexplosion.Gui.Extension;
using Lexplosion.Gui.ModalWindow;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Tools.Immutable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.ViewModels
{
    public class UserProfile : VMBase 
    {
        /// <summary>
        /// Данное свойство содержить информации - о том показан ли InfoBar.
        /// </summary>
        private static bool _isShowInfoBar;
        public bool IsShowInfoBar
        {
            get => _isShowInfoBar; set
            {
                _isShowInfoBar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Данное свойство содержить информации - о том авторизован ли пользователь.
        /// </summary>
        private bool _isAuthorized;
        public bool IsAuthorized
        {
            get => _isAuthorized; set
            {
                _isAuthorized = value;
                IsShowInfoBar = value;
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        /// <summary>
        /// Данное свойство содержить ник пользователя.
        /// </summary>
        private string _nickname;
        public string Nickname
        {
            get => _nickname; set
            {
                _nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public RelayCommand ChangeStatusCommand
        {
            get => new RelayCommand(obj =>
            {
                if (obj == null)
                    return;

                ActivityStatus newStatus;

                Enum.TryParse((string)obj, out newStatus);
                Global.UserData.User.ChangeBaseStatus(newStatus);
            });
        }
    }

    public class ModalWindowViewModel : VMBase
    {
        private readonly NavigationStore ModalWindowNavigationStore = new NavigationStore();

        public ModalVMBase CurrentModalContent => (ModalVMBase)ModalWindowNavigationStore.CurrentViewModel;
        
        /// <summary>
        /// Данное свойство содержить информации - открыт ли Экспорт [Popup].
        /// </summary>
        private bool _isModalOpen = false;
        public bool IsModalOpen
        {
            get => _isModalOpen; set
            {
                _isModalOpen = value;
                OnPropertyChanged();
            }
        }

        public ModalWindowViewModel()
        {
            ModalWindowNavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }


        public void ChangeCurrentModalContent(ModalVMBase modalVM) 
        {
            ModalWindowNavigationStore.CurrentViewModel = modalVM;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalContent));
        }
    }

    public class MainViewModel : VMBase
    {

        #region statics

        public static readonly NavigationStore NavigationStore = new NavigationStore();

        public static MainMenuViewModel MainMenuVM { get; private set; }

        /// <summary>
        /// Если запушена сборка true, иначе else.
        /// </summary>
        private bool _isInstanceRunning = false;
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

        public static ObservableCollection<MessageModel> Messages { get; private set; } = new ObservableCollection<MessageModel>();


        public static void ShowToastMessage(string header, string message, ToastMessageState state = ToastMessageState.Notification)
        {
            var model = new ToastMessageModel(header, message, state);
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

        #endregion statics

        #region props

        public MainModel Model { get; }
        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;
        public ExportViewModel ExportViewModel { get; set; }
        public LoadingBoard LoadingBoard { get; } = new LoadingBoard();
        public UserProfile UserProfile { get; } = new UserProfile();
        public ModalWindowViewModel ModalWindowVM { get; } = new ModalWindowViewModel();

        #endregion


        #region commands

        // MainWindow base

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

        #endregion


        public MainViewModel()
        {
            PreLoadGameVersions();

            //MainViewModel.ShowDialogMessage("Test", "Test123", () => { Console.WriteLine(123); }, () => { Console.WriteLine(321); }, "Yes", "No");

            Model = new MainModel();
            LibraryInstanceLoading();

            MainMenuVM = new MainMenuViewModel(this);

            NavigationStore.CurrentViewModel = new AuthViewModel(this);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            ExportViewModel = new ExportViewModel(this);
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

        #endregion
    }
}