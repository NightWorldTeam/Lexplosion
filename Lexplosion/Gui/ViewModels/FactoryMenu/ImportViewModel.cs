using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public class ImportFile
    {
        public string Name { get; }
        public string Path { get; }
        public bool IsImported { get; }

        public ImportFile(string name, string path, bool isImported)
        {
            Name = name;
            Path = path;
            IsImported = isImported;
        }
    }

    public sealed class ImportViewModel : VMBase
    {
        private MainViewModel _mainViewModel;
        private FactoryGeneralViewModel _factoryGeneralVM;

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

        public FactoryGeneralViewModel FactoryGeneralVM { get => _factoryGeneralVM; }

        public RelayCommand ImportCommand
        {
            get => new RelayCommand(obj =>
            {
                // Process open file dialog box results

                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {

                    ofd.Filter = "Archives files (.zip)|*.zip";
                    ofd.ShowDialog();

                    //reopen OpenFileDialog if it is zip file. this part can be improved.
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (ofd.FileName.EndsWith(".zip"))
                        {
                            Import(ofd.FileName);
                        }
                    }
                }
            });
        }

        public void Import(string path)
        {
            //UploadedFiles

            #nullable enable
            InstanceClient? instanceClient;

            ImportResult result = ImportResult.ServerFilesError;

            var importFile = new ImportFile("name", path, false);

            UploadedFiles.Add(importFile);

            result = InstanceClient.Import(path, out instanceClient);

            if (instanceClient == null || result != ImportResult.Successful)
            {
                MainViewModel.ShowToastMessage("Импорт завершился с ошибкой", result.ToString(), Controls.ToastMessageState.Error);
                return;
            }

            _mainViewModel.ModalWindowVM.IsOpen = false;

            _mainViewModel.Model.LibraryInstances.Add(
                new InstanceFormViewModel(_mainViewModel, instanceClient)
                );

            MainViewModel.ShowToastMessage("Результат импорта", "Импорт завершился успешно, хотите запустить сборку?", Controls.ToastMessageState.Notification);
        }

        private Action<string[]> _importAction;

        public Action<string[]> ImportAction
        {
            get => _importAction;
        }

        public ImportViewModel(MainViewModel mainViewModel, FactoryGeneralViewModel factoryGeneralViewModel)
        {
            _mainViewModel = mainViewModel;
            _factoryGeneralVM = factoryGeneralViewModel;

             _importAction = (string[] files) =>
             {
                 foreach (var file in files)
                 {
                     Console.WriteLine(file);
                     Import(file);
                 }
             };

            UploadedFiles = new ObservableCollection<ImportFile>();
        }
    }
}
