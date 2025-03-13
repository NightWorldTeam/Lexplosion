using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public class ImportProcess : ObservableObject
    {
        #region Properties


        public Guid Id { get; } = new Guid();
        public string Name { get; }
        public string Path { get; }


        private bool _isImporting;
        public bool IsImporing 
        {
            get => _isImporting; set 
            {
                _isImporting = value;
                OnPropertyChanged();
            }
        }

        private bool _isSuccessful;
        public bool IsSuccessful
        {
            get => _isSuccessful; set 
            {
                _isSuccessful = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties 


        #region Constructors


        public ImportProcess(string path, bool isImporting = true, bool isSuccessful = false)
        {
            Name = System.IO.Path.GetFileName(path);
            Path = path;
            IsImporing = isImporting;
            IsSuccessful = isSuccessful;
        }


        #endregion Constructors
    }
}
