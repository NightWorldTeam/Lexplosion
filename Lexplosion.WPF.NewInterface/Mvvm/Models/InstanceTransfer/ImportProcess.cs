using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public class ImportProcess : ObservableObject
    {
        public event Action<Guid> ImportCancelled;


        private readonly Action _cancelImport;


        #region Properties


        public Guid Id { get; }
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


        private InstanceClient _targetInstanceClient;
        public InstanceClient TargetInstanceClient 
        {
            get => _targetInstanceClient; set
            {
                if (_targetInstanceClient != null)
                {
                    return;
                }

                _targetInstanceClient = value;
            }
        }


        #endregion Properties 


        #region Constructors


        public ImportProcess(Guid id, string path, Action cancel, bool isImporting = true, bool isSuccessful = false)
        {
            Id = id;
            Name = System.IO.Path.GetFileName(path);
            Path = path;
            IsImporing = isImporting;
            IsSuccessful = isSuccessful;
            _cancelImport = cancel;
        }


        #endregion Constructors


        public void Cancel() 
        {
            _cancelImport?.Invoke();
            ImportCancelled?.Invoke(Id);
        }
    }
}
