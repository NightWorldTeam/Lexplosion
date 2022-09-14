using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class InstanceDownloadProcess
    {
        public ObservableCollection<InstanceFile> DownloadFiles { get; } = new ObservableCollection<InstanceFile>();
        public ObservableCollection<InstanceFile> InstalledFiles { get; } = new ObservableCollection<InstanceFile>();
        public ObservableCollection<InstanceFile> DownloadErrorFiles { get; } = new ObservableCollection<InstanceFile>();

        private object locker = new();

        private InstanceFormViewModel _viewModel;

        public bool IsEquals(InstanceFormViewModel instance)
        {
            return _viewModel == instance;
        }

        public InstanceDownloadProcess(InstanceFormViewModel instanceFormViewModel)
        {
            if (instanceFormViewModel != null)
            {
                _viewModel = instanceFormViewModel;
                instanceFormViewModel.Client.FileDownloadEvent += OnFileDownload;
                instanceFormViewModel.Client.ComplitedDownload += OnDownloadFinished;
            }
        }

        private void OnDownloadFinished(InstanceInit result, List<string> downloadErrors, bool launchGame)
        {
            if (result == InstanceInit.Successful)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    DownloadFiles.Clear();
                });
            }
        }

        private void OnFileDownload(string name, int procents, DownloadFileProgress process)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var instanceFile = InstanceFile.GetInstanceFile(DownloadFiles, name);
                if (instanceFile == null)
                {
                    instanceFile = new InstanceFile(name, procents);
                    DownloadFiles.Add(instanceFile);
                }
                else
                {
                    if (process == DownloadFileProgress.Successful)
                    {
                        DownloadFiles.Remove(instanceFile);
                        InstalledFiles.Add(instanceFile);
                    }
                    else if (process == DownloadFileProgress.Error)
                    {
                        DownloadFiles.Remove(instanceFile);
                        DownloadErrorFiles.Add(instanceFile);
                    }
                    else
                    {
                        instanceFile.Procents = procents;
                    }
                }
            });
        }

        public static bool Contains(IList<InstanceDownloadProcess> list, InstanceFormViewModel instance)
        {
            foreach (var item in list)
            {
                if (item.IsEquals(instance))
                    return true;
            }
            return false;
        }
    }
}
