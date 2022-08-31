using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public class ImportModel : VMBase 
    {
        
    }

    public sealed class ImportViewModel : VMBase
    {
        private MainViewModel _mainViewModel;
        private FactoryGeneralViewModel _factoryGeneralVM;

        private string[] _uploadedFiles;
        /// <summary>
        /// Коллекция с испортируемыми файлами.
        /// </summary>
        public string[] UploadedFiles
        {
            get => _uploadedFiles; set 
            {
                _uploadedFiles = value;
                foreach (var file in value) 
                {
                    Import(file);
                }
                OnPropertyChanged();
            }
        }

        public FactoryGeneralViewModel FactoryGeneralVM { get => _factoryGeneralVM; }

        public RelayCommand ImportCommand 
        {
            get => new RelayCommand(obj => 
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();

                // Process open file dialog box results
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Import(dialog.FileName);
                }
            }); 
        }

        public void Import(string path) 
        {
            #nullable enable
            InstanceClient? instanceClient;
            ImportResult result = InstanceClient.Import(path, out instanceClient);

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

        public ImportViewModel(MainViewModel mainViewModel, FactoryGeneralViewModel factoryGeneralViewModel)
        {
            _mainViewModel = mainViewModel;
            _factoryGeneralVM = factoryGeneralViewModel;
        }
    }
}
