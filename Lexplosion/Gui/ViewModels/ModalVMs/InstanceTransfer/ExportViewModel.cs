using Lexplosion.Controls;
using Lexplosion.Gui.ModalWindow;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public abstract class ExportBase : ModalVMBase 
    {
        #region ModalProperties

        public override double Width => 620;

        public override double Height => 420;

        #endregion ModalProperties

        #region Commands

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

        #endregion Commands
    }

    public sealed class ExportViewModel : ExportBase
    {
        /// <summary>
        /// Список хранит в себе загруженные директории [string].
        /// </summary>
        private List<string> LoadedDirectories = new List<string>();


        #region Properties


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
                Export();
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


        #region Private Methods


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

#if DEBUG
            foreach (var i in pathLevel.UnitsList.Values)
                Runtime.DebugWrite(pathLevel.FullPath + " into " + i.FullPath);
#endif

            if (pathLevel.IsSelected)
                pathLevel.IsSelected = true;

            LoadedDirectories.Add(dir);
            return pathLevel.UnitsList;
        }

        /// <summary>
        /// Собственно, открытие диалогового окна.
        /// Вызов экспорта.
        /// </summary>
        private async void Export()
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = @"C:\Users\GamerStorm_Hel2x_\night-world\export";
                saveFileDialog.Filter = "zip files (*.zip)|*.zip";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = InstanceClient.LocalId + ".zip";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IsExportFinished = false;
                    MainViewModel.ExportedInstance.Add(this);

                    var result = await Task.Run(() => InstanceClient.Export(UnitsList, saveFileDialog.FileName, InstanceName));

                    MainViewModel.ExportedInstance.Remove(this);

                    if (result == ExportResult.Successful)
                    {
                        MainViewModel.ShowToastMessage(
                            ResourceGetter.GetString("instanceExport"),
                            String.Format(ResourceGetter.GetString("instanceExportSuccessfulOpenFolder"), InstanceName),
                            ToastMessageState.Notification);

                        IsExportFinished = true;
                        ModalWindowViewModelSingleton.Instance.Close();
                    }
                    else
                    {
                        MainViewModel.ShowToastMessage(
                            result.ToString(),
                            String.Format(ResourceGetter.GetString("instanceExportUnsuccessful"), InstanceName),
                            ToastMessageState.Error);

                        IsExportFinished = true;
                    }
                }
            }
        }


        #endregion Private Methods
    }
}
