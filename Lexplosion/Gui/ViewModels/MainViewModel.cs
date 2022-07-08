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

    public class ImmutableArray<T> : IEnumerable<T>
    {
        private readonly T[] _array;

        public ImmutableArray(T[]? array)
        {
            _array = _array;
        }

        public ImmutableArray(List<T> list)
        {
            _array = new T[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                _array[i] = list[i];
            }
        }

        public List<T> ToList()
        {
            var result = new List<T>();

            foreach (var item in _array)
            {
                result.Add(item);
            }

            return result;
        }

        public T[] ToArray() => (T[])_array.Clone();

        public T this[int index]
        {
            get => _array[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)_array.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MainViewModel : VMBase
    {

        #region statics

        public static readonly NavigationStore NavigationStore = new NavigationStore();
        public static MainMenuViewModel MainMenuVM { get; private set; }
        public static bool IsInstanceRunning = false;

        /// <summary>
        /// Данное свойство содержит в себе версии игры.
        /// Является static, т.к эксемпляр MainViewModel создаётся в единственном эксемляре, в начале запуска лаунчер, до появляния начального окна.
        /// </summary>
        public static ImmutableArray<string> GameVersions { get; private set; }

        #endregion statics


        #region props

        public MainModel Model { get; }
        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;

        /// InfoBar ///

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


        // Export Properties //

        /// <summary>
        /// Данное свойство содержить информации - открыт ли Экспорт [Popup].
        /// </summary>
        private bool _isExporting = false;
        public bool IsExporting
        {
            get => _isExporting; set
            {
                _isExporting = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Данное свойство содержить информации - о названии экспортируемой сборки.
        /// </summary>
        private string _exportInstanceName = "Название сборки";
        public string ExportInstanceName
        {
            get => _exportInstanceName; set
            {
                _exportInstanceName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Свойство содержит информацию - экспортируются ли все файлы сборки.
        /// </summary>
        private bool _isFullExport = true;
        public bool IsFullExport 
        {
            get => _isFullExport; set 
            {
                _isFullExport = value;
            }
        }

        #endregion


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
            PreLoadGameVersions();

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

        /// <summary>
        /// Метод загружает сборки для библиотеки.
        /// </summary>
        private void LibraryInstanceLoading()
        {
            foreach (var instanceClient in InstanceClient.GetInstalledInstances())
            {
                MainModel.LibraryInstances.Add(
                    instanceClient, new InstanceFormViewModel(this, instanceClient)
                );
            }
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