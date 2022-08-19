using Lexplosion.Controls;
using Lexplosion.Gui.ModalWindow;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public class ExportViewModel : ModalVMBase
    {
        private MainViewModel _mainViewModel;

        public MainViewModel MainVM
        {
            get => _mainViewModel;
        }

        public ExportViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }


        #region commands

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Экспорт, в Export Popup.
        /// Запускает экспорт модпака.
        /// </summary>
        public override RelayCommand Action
        {
            get => new RelayCommand(obj =>
            {
                Export();
                _mainViewModel.ModalWindowVM.IsModalOpen = false;
            });
        }

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Отмена, в Export Popup.
        /// Отменяет экспорт, скрывает popup меню.
        /// </summary>
        public override RelayCommand CloseModalWindow
        {
            get => new RelayCommand(obj =>
            {
                _mainViewModel.ModalWindowVM.IsModalOpen = false;
            });
        }

        #endregion commands


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
                OnPropertyChanged();

                Console.WriteLine("Full Export: ", value);

                if (UnitsList != null)
                {
                    foreach (var val in UnitsList.Values)
                    {
                        val.IsSelected = value;
                    }
                }
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
                LoadDirContent(keyvaluepair.Value.FullPath, keyvaluepair.Value);
            });
        }

        /// <summary>
        /// Загружает контент для директории. Если директория уже загружена, то производиться выход из метода.
        /// </summary>
        /// <param name="dir"></param>
        private void LoadDirContent(string dir, PathLevel pathLevel)
        {
            if (LoadedDirectories.Contains(dir) || dir == null || pathLevel.IsFile)
                return;

            pathLevel.UnitsList = _instanceClient.GetPathContent(dir, pathLevel);

            if (pathLevel.IsSelected)
                pathLevel.IsSelected = true;

            LoadedDirectories.Add(dir);
        }

        /// <summary>
        /// Собственно, открытие диалогового окна.
        /// Вызов экспорта.
        /// </summary>
        private void Export()
        {
            var saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog1.InitialDirectory = @"C:\Users\GamerStorm_Hel2x_\night-world\export";
            saveFileDialog1.Filter = "zip files (*.zip)|*.zip";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = InstanceClient.LocalId + ".zip";

            var result = saveFileDialog1.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Lexplosion.Run.TaskRun(() =>
                {
                    var result = InstanceClient.Export(UnitsList, saveFileDialog1.FileName, InstanceName);
                    if (result == ExportResult.Successful)
                    {
                        MainViewModel.ShowToastMessage("Экспорт клиента", String.Format("Экспорт сборки {0} был успешно завершён. Открыть папку с файлом?", InstanceName), ToastMessageState.Notification);
                    }
                    else
                    {
                        MainViewModel.ShowToastMessage(result.ToString(), String.Format("Экспорт сборки {0} не успешно завершён. Открыть папку с файлом?", InstanceName), ToastMessageState.Error);
                    }
                });
            }
        }
    }
}
