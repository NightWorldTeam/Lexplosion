using System;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class ImportFile : VMBase
    {
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

        private bool _isImportSuccessful = true;
        public bool IsImportSuccessful
        {
            get => _isImportSuccessful; set
            {
                _isImportSuccessful = value;
                OnPropertyChanged();
            }
        }

        public ImportFile(string path)
        {
            Name = System.IO.Path.GetFileName(path);
            Path = path;
            IsImportFinished = false;
        }

        public ImportFile(Uri fileURL)
        {
            Name = System.IO.Path.GetFileName(fileURL.LocalPath);
            Path = fileURL.OriginalString;
            IsImportFinished = false;
        }
    }
}
