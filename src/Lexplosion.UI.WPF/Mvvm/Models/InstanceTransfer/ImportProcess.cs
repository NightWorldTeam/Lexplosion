using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;

namespace Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer
{
    public class ImportProcess : ObservableObject
    {
        public event Action<ImportProcess> ImportCancelled;


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


        public ImportProcess(Guid id, string path, bool isImporting = true, bool isSuccessful = false)
        {
            Id = id;
            Name = System.IO.Path.GetFileName(path);
            Path = path;
            IsImporing = isImporting;
            IsSuccessful = isSuccessful;
        }

        public ImportProcess(Guid id, Uri url, bool isImporting = true, bool isSuccessful = false)
        {
            Id = id;
            Name = System.IO.Path.GetFileName(url.LocalPath);
            Path = url.OriginalString;
            IsImporing = isImporting;
            IsSuccessful = isSuccessful;
        }


        #endregion Constructors


        public void Cancel()
        {
            ImportCancelled?.Invoke(this);
        }
    }
}
