using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class ImportFile
    {
        private readonly ImportViewModel _importVM;
        public string Name { get; }
        public string Path { get; }
        public bool IsImportFinished { get; set; }

        public ImportFile(ImportViewModel importVM, string path)
        {
            _importVM = importVM;
            Name = System.IO.Path.GetFileName(path);
            Path = path;
            IsImportFinished = false;
        }

        public RelayCommand CancelUploadCommand
        {
            get => new RelayCommand(obj => 
            {
                CancelUpload();
            });
        }

        private void CancelUpload() 
        {
            var index = _importVM.UploadedFiles.IndexOf(this);
            _importVM.UploadedFiles.RemoveAt(index);
        }
    }

    public sealed class ImportViewModel : VMBase
    {
        private MainViewModel _mainViewModel;
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
            }
        }

        /// <summary>
        /// Делегат который вызывает принимает в качестве агрумента массив строк.
        /// Проходится по массиву и для каждого элемента начинает импорт.
        /// </summary>
        public Action<string[]> ImportAction { get;}


        #endregion Properties


        #region Commands

        /// <summary>
        /// Команда для ImportView -> [кнопка]Обзор.
        /// </summary>
        public RelayCommand ImportCommand
        {
            get => new RelayCommand(obj =>
            {
                ShowOpenFileDialogForImport();
            });
        }


        #endregion Commands


        #region Public & Protected Methods


        public void Import(string path)
        {
            #nullable enable
            InstanceClient? instanceClient;

            var importFile = new ImportFile(this, path);
            
            // Добавляем импортируемый файл в ObservableColletion для вывода загрузки.
            UploadedFiles.Add(importFile);
            
            // Начинаем импорт файла.
            ImportResult result = InstanceClient.Import(path, out instanceClient);
            
            // Импорт закончился.
            importFile.IsImportFinished = result == ImportResult.Successful;

            if (instanceClient == null || result != ImportResult.Successful)
            {
                // Выводим сообщение о результате испорта.
                MainViewModel.ShowToastMessage("Импорт завершился с ошибкой", result.ToString(), Controls.ToastMessageState.Error);
                return;
            }

            //Закрываем модальное окно.
            FactoryGeneralViewModel.CloseModalWindow.Execute(null);
            
            // Добавляем сборку в библиотеку.
            _mainViewModel.Model.LibraryInstances.Add(new InstanceFormViewModel(_mainViewModel, instanceClient));
            
            // Выводим сообщение о результате испорта.
            MainViewModel.ShowToastMessage("Результат импорта", "Импорт завершился успешно, хотите запустить сборку?", Controls.ToastMessageState.Notification);
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
                 {
                     Console.WriteLine(file);
                     Import(file);
                 }
             };

            UploadedFiles = new ObservableCollection<ImportFile>();
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
                ofd.ShowDialog();

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


        #endregion Private Methods
    }
}
