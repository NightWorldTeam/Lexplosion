using Lexplosion.Gui.Models;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lexplosion.Gui.ViewModels
{
    public class ImmutableList<T> : IEnumerable<T>
    {
        private readonly List<T> _list;

        public ImmutableList(List<T> list)
        {
            _list = list;
        }

        public List<T> ToList() => _list; 
        
        public T this[int index] 
        {
            get => _list[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MainViewModel : VMBase
    {
        public static readonly NavigationStore NavigationStore = new NavigationStore();
        public static MainMenuViewModel MainMenuVM { get; private set; }

        private ObservableCollection<string> _gameVersions;

        public static bool IsInstanceRunning = false;

        private static bool _isExporting = false;
        public bool IsExporting 
        {
            get => _isExporting; set 
            {
                _isExporting = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Данное свойство содержит в себе версии игры.
        /// Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.
        /// </summary>
        public static ImmutableList<string> GameVersions { get; private set; }

        #region props

        public MainModel Model { get; }

        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;

        private static bool _isShowInfoBar;
        public bool IsShowInfoBar 
        { 
            get => _isShowInfoBar; set 
            {
                _isShowInfoBar = value;
                OnPropertyChanged();
            }
        }

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

        private string _nickname;
        public string Nickname 
        {
            get => _nickname; set 
            {
                _nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public object InstanceForms { get; private set; }
        #endregion

        public ObservableCollection<string> Dirs { get; } = new ObservableCollection<string>();

        #region commands

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

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Экспорт, в Export Popup.
        /// Запускает экспорт модпака.
        /// </summary>
        public RelayCommand ExportInstance 
        {
            get => new RelayCommand(obj => 
            {
                IsExporting = false;
            });
        }

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Отмена, в Export Popup.
        /// Отменяет экспорт, скрывает popup меню.
        /// </summary>
        public RelayCommand CancelExport 
        {
            get => new RelayCommand(obj => 
            {
                IsExporting = false;
            });
        }

        #endregion

        public MainViewModel()
        {
            List<string> versions = new List<string>();
            Lexplosion.Run.TaskRun(() =>
            {
                foreach (var v in ToServer.GetVersionsList())
                {
                    if (v.type == "release") versions.Add(v.id);
                }
                GameVersions = new ImmutableList<string>(versions);
            });
            Model = new MainModel();
            MainMenuVM = new MainMenuViewModel(this);
            NavigationStore.CurrentViewModel = new AuthViewModel(this, LibraryInstanceLoading);
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        #region methods
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        private void LibraryInstanceLoading()
        {
            foreach (var instanceClient in InstanceClient.GetInstalledInstances()) 
            { 
                MainModel.LibraryInstances.Add(
                    instanceClient, new InstanceFormViewModel(instanceClient)
                );
            }
        }
        #endregion
    }
}