using Lexplosion.Common.Models;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.ViewModels.FactoryMenu;
using Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer;
using Lexplosion.Controls;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.ObjectModel;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class ImportViewModel : ImportBase
    {
        private InstanceClient _instanceClient;


        #region Properties


        /// <summary>
        /// Коллекция с испортируемыми файлами.
        /// </summary>
        private ObservableCollection<ImportFile> _uploadedFiles;
        public ObservableCollection<ImportFile> UploadedFiles
        {
            get => _uploadedFiles; set
            {
                _uploadedFiles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmptyUploadedFiles));
            }
        }

        public bool IsEmptyUploadedFiles { get => UploadedFiles.Count == 0; }

        /// <summary>
        /// Делегат который принимает в качестве агрумента массив строк.
        /// Проходится по массиву и для каждого элемента начинает импорт.
        /// </summary>
        public Action<string[]> ImportAction { get; }


        public string ImportURL { get; set; }


        #endregion Properties


        #region Commands


        /// <summary>
        /// Команда для ImportView -> [кнопка]Обзор.
        /// </summary>
        private RelayCommand _importCommand;
        public RelayCommand ImportCommand
        {
            get => _importCommand ?? (_importCommand ?? new RelayCommand(obj =>
            {
                ShowOpenFileDialogForImport();
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


        private RelayCommand _cancelUploadCommand;
        public RelayCommand CancelUploadCommand
        {
            get => _cancelUploadCommand ?? (_cancelUploadCommand = new RelayCommand(obj =>
            {
                var file = (ImportFile)obj;
                var index = UploadedFiles.IndexOf(file);
                UploadedFiles.RemoveAt(index);
            }));
        }


        private RelayCommand _importFromYandexOrGoogleDiskCommand;
        public RelayCommand ImportFromYandexOrGoogleDiskCommand
        {
            get => _importFromYandexOrGoogleDiskCommand ?? (_importFromYandexOrGoogleDiskCommand ?? new RelayCommand(obj =>
            {
                Import();
            }));
        }


        #endregion Commands


        #region Construcotors


        public ImportViewModel(DoNotificationCallback doNotification = null) : base(doNotification)
        {
            ImportAction = (string[] files) =>
            {
                foreach (var file in files)
                    Import(file);
            };

            UploadedFiles = new ObservableCollection<ImportFile>();
        }


        #endregion Construcotors


        #region Public & Protected Methods


        public void Import() 
        {
            if (string.IsNullOrEmpty(ImportURL)) 
            {
                DownloadResultHandler(ImportResult.WrongUrl);
                return;
            }

            var uri = new Uri(ImportURL);
            var importFile = new ImportFile(uri);

            // Добавляем импортируемый файл в ObservableColletion для вывода загрузки.
            UploadedFilesChanged(importFile);

            _instanceClient = InstanceClient.Import(uri, (r) => 
            {
                importFile.IsImportFinished = true;
                DownloadResultHandler(r);
                if (r != ImportResult.Successful && _instanceClient != null)
                {
                    importFile.IsImportSuccessful = false;
                    MainModel.Instance.LibraryController.RemoveByInstanceClient(_instanceClient);
                }
            });
            MainModel.Instance.AddInstanceForm(_instanceClient);
        }

        public void Import(string path)
        {
#nullable enable
            var importFile = new ImportFile(path);

            // Добавляем импортируемый файл в ObservableColletion для вывода загрузки.
            UploadedFilesChanged(importFile);

            _instanceClient = InstanceClient.Import(path, (_result) =>
                {
                    importFile.IsImportFinished = true;
                    DownloadResultHandler(_result);
                    if (_result != ImportResult.Successful && _instanceClient != null)
                    {
                        importFile.IsImportSuccessful = false;
                        MainModel.Instance.LibraryController.RemoveByInstanceClient(_instanceClient);
                    }
                }
            );
            MainModel.Instance.AddInstanceForm(_instanceClient);
        }


        #endregion Public & Protected Methods


        #region Private Methods

        /// <summary>
        /// Открывает диалогое окно позволяя выбрать .zip файл(ы) и начать его(их) импорт.
        /// </summary>
        private void ShowOpenFileDialogForImport()
        {
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Filter = "Archives files (.zip)|*.zip";

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Если выбранный файл является *zip.
                    if (ofd.FileName.EndsWith(".zip"))
                    {
                        Import(ofd.FileName);
                    }
                }
            }
        }

        private void UploadedFilesChanged(ImportFile importFile)
        {
            UploadedFiles.Add(importFile);
            OnPropertyChanged(nameof(IsEmptyUploadedFiles));
        }


        #endregion Private Methods
    }
}
