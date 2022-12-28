using Lexplosion.Gui.ModalWindow;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class ImportFile : VMBase
    {
        private readonly ImportViewModel _importVM;
        public string Name { get; }
        public string Path { get; }
        private bool _isImportFinished;
        public bool IsImportFinished
        {
            get => _isImportFinished;
            set
            {
                _isImportFinished = value;
                OnPropertyChanged();
            }
        }

        public ImportFile(ImportViewModel importVM, string path)
        {
            _importVM = importVM;
            Name = System.IO.Path.GetFileName(path);
            Path = path;
            IsImportFinished = false;
        }

        private RelayCommand _cancelUploadCommand;
        public RelayCommand CancelUploadCommand
        {
            get => _cancelUploadCommand ?? (_cancelUploadCommand = new RelayCommand(obj =>
            {
                CancelUpload();
            }));
        }

        private void CancelUpload()
        {
            var index = _importVM.UploadedFiles.IndexOf(this);
            _importVM.UploadedFiles.RemoveAt(index);
        }
    }

    public sealed class ImportViewModel : ModalVMBase
    {
        private readonly MainViewModel _mainViewModel;
        public FactoryGeneralViewModel FactoryGeneralViewModel { get; }


        #region Properties


        private ObservableCollection<ImportFile> _uploadedFiles;
        /// <summary>
        /// Коллекция с испортируемыми файлами.
        /// </summary>
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
        /// Делегат который вызывает принимает в качестве агрумента массив строк.
        /// Проходится по массиву и для каждого элемента начинает импорт.
        /// </summary>
        public Action<string[]> ImportAction { get; }


        #endregion Properties


        #region Commands


        private RelayCommand _importCommand;
        /// <summary>
        /// Команда для ImportView -> [кнопка]Обзор.
        /// </summary>
        public RelayCommand ImportCommand
        {
            get => _importCommand ?? (_importCommand ?? new RelayCommand(obj =>
            {
                ShowOpenFileDialogForImport();
            }));
        }

        private RelayCommand _closeModalWindowCommand;
        /// <summary>
        /// Свойтсво отрабатывает при нажатии кнопки Отмена, в Export Popup.
        /// Отменяет экспорт, скрывает popup меню.
        /// </summary>
        public override RelayCommand CloseModalWindowCommand
        {
            get => _closeModalWindowCommand ?? (_closeModalWindowCommand = new RelayCommand(obj =>
            {
                _mainViewModel.ModalWindowVM.IsOpen = false;
            }));
        }


        #endregion Commands


        #region Public & Protected Methods


        public async void Import(string path)
        {
#nullable enable
            InstanceClient? instanceClient = null;

            var importFile = new ImportFile(this, path);

            // Добавляем импортируемый файл в ObservableColletion для вывода загрузки.
            UploadedFilesChanged(importFile);

            // Начинаем импорт файла.
            var result = await Task.Run(() => InstanceClient.Import(path, out instanceClient));

            // Импорт закончился.
            importFile.IsImportFinished = result == ImportResult.Successful;

            if (instanceClient == null || result != ImportResult.Successful)
            {
                // Выводим сообщение о результате испорта.
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("importResultError"),
                    result.ToString(),
                    Controls.ToastMessageState.Error);
                return;
            }


            //Закрываем модальное окно.
            // TODO: Надо ли?
            //FactoryGeneralViewModel.CloseModalWindowCommand.Execute(null);

            // Добавляем сборку в библиотеку.
            _mainViewModel.Model.LibraryInstances.Add(new InstanceFormViewModel(_mainViewModel, instanceClient));

            // Выводим сообщение о результате испорта.
            MainViewModel.ShowToastMessage(
                ResourceGetter.GetString("importResult"),
                ResourceGetter.GetString("importResultSuccessfulWannaPlay"),
                Controls.ToastMessageState.Notification);
        }


        #endregion Public & Protected Methods


        #region Construcotors


        public ImportViewModel(MainViewModel mainViewModel, FactoryGeneralViewModel factoryGeneralViewModel)
        {
            _mainViewModel = mainViewModel;
            FactoryGeneralViewModel = factoryGeneralViewModel;

            ImportAction = (string[] files) =>
            {
                foreach (var file in files)
                    Import(file);
            };

            UploadedFiles = new ObservableCollection<ImportFile>();
            //{
            //    new ImportFile(this, "C:\\Users\\HelJo\\Downloads\\git-lfs-windows-v3.3.0.exe")
            //};
            //UploadedFiles[0].IsImportFinished = true;
        }


        #endregion Construcotors


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
