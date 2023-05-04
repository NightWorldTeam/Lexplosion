using Lexplosion.Common.ViewModels.ModalVMs;

namespace Lexplosion.Common.Models.Objects
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

        //TODO: вынести во viewmodel
        private RelayCommand _cancelUploadCommand;
        public RelayCommand CancelUploadCommand
        {
            get => _cancelUploadCommand ?? (_cancelUploadCommand = new RelayCommand(obj =>
            {
                CancelUpload();
            }));
        }

        public void CancelUpload()
        {
            var index = _importVM.UploadedFiles.IndexOf(this);
            _importVM.UploadedFiles.RemoveAt(index);
        }
    }
}
