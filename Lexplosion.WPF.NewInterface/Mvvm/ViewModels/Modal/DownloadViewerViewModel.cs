using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class InstanceFile : VMBase
    {
        public string Name { get; }
        private int _procents;
        public int Procents
        {
            get => _procents; set
            {
                _procents = value;
                OnPropertyChanged();
            }
        }

        public InstanceFile(string name, int procents)
        {
            Name = name;
            Procents = procents;
        }

        /// Выполняется O(n).
        public static InstanceFile GetInstanceFile(IEnumerable<InstanceFile> files, string name)
        {
            foreach (var file in files)
            {
                if (file.Name == name)
                    return file;
            }
            return null;
        }
    }

    public sealed class DownloadViewerModel : ObservableObject
    {
        public ObservableCollection<InstanceFile> DownloadingFiles { get; } = new ObservableCollection<InstanceFile>();
        public ObservableCollection<InstanceFile> InstalledFiles { get; } = new ObservableCollection<InstanceFile>();
        public ObservableCollection<InstanceFile> ErrorsFiles { get; } = new ObservableCollection<InstanceFile>();


        public DownloadViewerModel(InstanceModelBase instanceFormViewModel)
        {
            instanceFormViewModel.InstanceClient.FileDownloadEvent += OnFileDownload;
            instanceFormViewModel.InstanceClient.StateChanged += OnStateChanged; ;
        }

        private void OnFileDownload(string name, int procents, DownloadFileProgress process)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var instanceFile = InstanceFile.GetInstanceFile(DownloadingFiles, name);
                if (instanceFile == null)
                {
                    instanceFile = new InstanceFile(name, procents);
                    DownloadingFiles.Add(instanceFile);
                }
                else
                {
                    if (process == DownloadFileProgress.Successful)
                    {
                        DownloadingFiles.Remove(instanceFile);
                        InstalledFiles.Add(instanceFile);
                    }
                    else if (process == DownloadFileProgress.Error)
                    {
                        DownloadingFiles.Remove(instanceFile);
                        ErrorsFiles.Add(instanceFile);
                    }
                    else
                    {
                        instanceFile.Procents = procents;
                    }
                }
            });
        }

        private void OnStateChanged(StateType obj)
        {
            if (obj == StateType.Default)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    DownloadingFiles.Clear();
                });
            }
        }
    }

    public sealed class DownloadViewerViewModel : ModalViewModelBase
    {
        public DownloadViewerModel Model { get; }

        public DownloadViewerViewModel(InstanceModelBase instanceModelBase)
        {
            Model = new(instanceModelBase);
        }
    }
}
