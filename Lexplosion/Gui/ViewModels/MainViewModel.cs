using Lexplosion.Controls;
using Lexplosion.Gui.Helpers;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Stores;
using Lexplosion.Gui.ViewModels.MainMenu;
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
    public class InstanceExport : VMBase
    {
        /// <summary>
        /// Список хранит в себе загруженные директории [string].
        /// </summary>
        private List<string> LoadedDirectories = new List<string>();

        private string _instanceName;
        public string InstanceName
        {
            get => _instanceName; set
            {
                _instanceName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Данные о сборке.
        /// </summary>
        private InstanceClient _instanceClient;
        public InstanceClient InstanceClient
        {
            get => _instanceClient; set
            {
                _instanceClient = value;
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

        /// <summary>
        /// Коллекция [Словарь] который обновляется при добавлении в него значений.
        /// </summary>
        private Dictionary<string, PathLevel> _unitsList;
        public Dictionary<string, PathLevel> UnitsList
        {
            get => _unitsList; set
            {
                _unitsList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Команда отрабатывает при раскрытии TreeViewItem.
        /// </summary>
        public RelayCommand TreeViewItemExpanded
        {
            get => new RelayCommand(obj =>
            {
                if (obj == null)
                    return;

                // key - directory, value - pathlevel class
                var keyvaluepair = (KeyValuePair<string, PathLevel>)obj;
                LoadDirContent(keyvaluepair.Key, keyvaluepair.Value);
            });
        }

        /// <summary>
        /// Загружает контент для директории. Если директория уже загружена, то производиться выход из метода.
        /// </summary>
        /// <param name="dir"></param>
        private void LoadDirContent(string dir, PathLevel pathLevel)
        {
            if (LoadedDirectories.Contains(dir) || dir == null)
                return;

            if (!pathLevel.IsFile)
                UnitsList[dir].UnitsList = _instanceClient.GetPathContent(dir);
           
            LoadedDirectories.Add(dir);
        }

        public void Export()
        {
            var saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog1.InitialDirectory = @"C:\Users\GamerStorm_Hel2x_\night-world\export";
            saveFileDialog1.Filter = "zip files (*.zip)|*.zip";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = InstanceName + ".zip";

            var result = saveFileDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Lexplosion.Run.TaskRun(() =>
                {
                    var result = InstanceClient.Export(UnitsList, saveFileDialog1.FileName);
                    if (result == ExportResult.Successful)
                    {
                        MainViewModel.ShowToastMessage("Экспорт сборки", String.Format("Экспорт сборки {0} был успешно завершён. Открыть папку с файлом?", InstanceName), ToastMessageState.Notification);
                    }
                    else
                    {
                        MainViewModel.ShowToastMessage(result.ToString(), String.Format("Экспорт сборки {0} не успешно завершён. Открыть папку с файлом?", InstanceName), ToastMessageState.Error);
                    }
                });
            }
        }
    }

    public class MainViewModel : VMBase
    {
        public InstanceExport InstanceExport { get; } = new InstanceExport();

        #region statics

        public static readonly NavigationStore NavigationStore = new NavigationStore();

        public static MainMenuViewModel MainMenuVM { get; private set; }

        /// <summary>
        /// Если запушена сборка true, иначе else.
        /// </summary>
        public static bool IsInstanceRunning = false;

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
                InstanceExport.Export();
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

            //MainViewModel.ShowDialogMessage("Test", "Test123", () => { Console.WriteLine(123); }, () => { Console.WriteLine(321); }, "Yes", "No");

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