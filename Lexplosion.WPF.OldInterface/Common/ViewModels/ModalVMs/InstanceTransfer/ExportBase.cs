using Lexplosion.Common.ModalWindow;
using Lexplosion.Controls;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System.Collections.Generic;

namespace Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer
{
    public abstract class ExportBase : ModalVMBase, INotifiable
    {
        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; protected set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }


        #region ModalProperties

        public override double Width => 620;

        public override double Height => 420;


        #endregion ModalProperties


        #region Properties


        protected readonly InstanceClient _instanceClient;

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
        /// Отвечает на вопрос закончился ли экспорт сборка.
        /// </summary>
        private bool _isExportFinished = true;
        public bool IsExportFinished
        {
            get => _isExportFinished; set
            {
                _isExportFinished = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Свойство содержит информацию - экспортируются ли все файлы сборки.
        /// </summary>
        private bool _isFullExport = false;
        public bool IsFullExport
        {
            get => _isFullExport; set
            {
                _isFullExport = value;
                OnPropertyChanged();
                ReselectedAllUnits(value);
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
                ContentPreload(value);
            }
        }

        #endregion Properties


        #region Commands

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Экспорт, в Export Popup.
        /// Запускает экспорт модпака.
        /// </summary>
        private RelayCommand _actionCommand;
        public override RelayCommand ActionCommand
        {
            get => _actionCommand ?? (_actionCommand = new RelayCommand(obj =>
            {
                Action();
            }));
        }

        /// <summary>
        /// Скрывает модальное окно с контентом.
        /// </summary>
        private RelayCommand _hideModalWindowCommand;
        public override RelayCommand HideModalWindowCommand
        {
            get => _hideModalWindowCommand ?? (_hideModalWindowCommand = new RelayCommand(obj =>
            {
                ModalWindowViewModelSingleton.Instance.Hide();
                // TODO: ещё надо станавливать анимацию до появления окна.
            }));
        }

        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Отмена, в Export Popup.
        /// Отменяет экспорт, скрывает popup меню.
        /// </summary>
        private RelayCommand _closeModalWindowCommand;
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeModalWindowCommand ?? (_closeModalWindowCommand = new RelayCommand(obj =>
            {
                ModalWindowViewModelSingleton.Instance.Close();
            }));
        }

        /// <summary>
        /// Команда отрабатывает при раскрытии TreeViewItem.
        /// </summary>
        private RelayCommand _treeViewItemExpanded;
        public RelayCommand TreeViewItemExpanded
        {
            get => _treeViewItemExpanded ?? (_treeViewItemExpanded = new RelayCommand(obj =>
            {
                if (obj == null)
                    return;

                // key - directory, value - pathlevel class
                var keyValuePair = (KeyValuePair<string, PathLevel>)obj;
                // PathLevel.FullPath | PathLevel

                var sub = LoadDirContent(keyValuePair.Value.FullPath, keyValuePair.Value);
                ContentPreload(sub);
            }));
        }


        #endregion Commands


        #region Constructors


        public ExportBase(InstanceClient instanceClient, DoNotificationCallback doNotification)
        {
            DoNotification = doNotification;
            _instanceClient = instanceClient;
            InstanceName = instanceClient.Name;
            IsFullExport = false;
            UnitsList = instanceClient.GetPathContent();
        }


        #endregion Constructors


        #region Private Methods

        /// <summary>
        /// Перевыделяет все элементы в UnitsList.
        /// </summary>
        /// <param name="value">значение выделения</param>
        private void ReselectedAllUnits(bool value)
        {
            if (UnitsList != null)
            {
                foreach (var val in UnitsList.Values)
                {
                    val.IsSelected = value;
                }
            }
        }

        /// <summary>
        /// Загрузка данных для UnitsList, на один шаг вперёд.
        /// </summary>
        /// <param name="subUnits">Словарь UnitsList</param>
        private void ContentPreload(Dictionary<string, PathLevel> subUnits)
        {
            foreach (var i in subUnits)
            {
                LoadDirContent(i.Value.FullPath, i.Value);
            }
        }

        /// <summary>
        /// Загружает контент для директории. Если директория уже загружена, то производиться выход из метода.
        /// </summary>
        /// <param name="dir"></param>
        private Dictionary<string, PathLevel> LoadDirContent(string dir, PathLevel pathLevel)
        {
            if (pathLevel.IsFile)
                return pathLevel.UnitsList = new Dictionary<string, PathLevel>();

            if (LoadedDirectories.Contains(dir) || dir == null)
                return pathLevel.UnitsList;

            pathLevel.UnitsList = _instanceClient.GetPathContent(dir);

            if (pathLevel.IsSelected)
                pathLevel.IsSelected = true;

            LoadedDirectories.Add(dir);
            return pathLevel.UnitsList;
        }

        #endregion Private Methods


        #region Public & Protected Methods


        protected abstract void Action();

        protected void ExportResultHandler(ExportResult result)
        {
            var message = GetExportResultMessage(result);
            switch (result)
            {
                case ExportResult.Successful:
                    DoNotification(ResourceGetter.GetString("resultSuccesfulTitle"), message, 5, 0);
                    break;
                default:
                    DoNotification(ResourceGetter.GetString("resultNotSuccesfulTitle"), message, 5, 1);
                    break;
            }
        }


        public static string GetExportResultMessage(ExportResult exportResult)
        {
            switch (exportResult)
            {
                case ExportResult.Successful: return ResourceGetter.GetString("exportResultSuccesful");
                case ExportResult.TempPathError: return ResourceGetter.GetString("exportTempPathError");
                case ExportResult.FileCopyError: return ResourceGetter.GetString("exportFileCopyError");
                case ExportResult.InfoFileError: return ResourceGetter.GetString("exportInfoFileError");
                case ExportResult.ZipFileError: return ResourceGetter.GetString("exportZipFileError");
                default: return ResourceGetter.GetString("resultNotExists");
            }
        }
        #endregion Public & Protected Methods
    }
}
