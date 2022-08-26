using Lexplosion.Gui.ModalWindow;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public class InstanceFile : VMBase
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

        public static InstanceFile? GetInstanceFile(IList<InstanceFile> files, string name) 
        {
            foreach (var file in files) 
            {
                if (file.Name == name)
                    return file;
            }
            return null;
        }
    }

    public class InstanceDownloadProcess
    {
        public ObservableCollection<InstanceFile> DownloadFiles { get; } = new ObservableCollection<InstanceFile>();
        public ObservableCollection<InstanceFile> InstalledFiles { get;  } = new ObservableCollection<InstanceFile>();

        private InstanceFormViewModel _viewModel;

        public bool IsEquals(InstanceFormViewModel instance) 
        {
            return _viewModel == instance;
        }

        public InstanceDownloadProcess(InstanceFormViewModel instanceFormViewModel)
        {
            //DonwloadFiles.Add(new InstanceFile("TestFile90", 20));
            //DonwloadFiles.Add(new InstanceFile("TestFile12", 76));
            //DonwloadFiles.Add(new InstanceFile("TestFile23", 15));
            //DonwloadFiles.Add(new InstanceFile("TestFile24", 93));
            //DonwloadFiles.Add(new InstanceFile("TestFile21", 50));
            //DonwloadFiles.Add(new InstanceFile("TestFile22", 20));
            //DonwloadFiles.Add(new InstanceFile("TestFile20", 21));

            //InstalledFiles.Add(new InstanceFile("TestFile", 20));
            //InstalledFiles.Add(new InstanceFile("TestFile1", 76));
            //InstalledFiles.Add(new InstanceFile("TestFile2", 15));
            //InstalledFiles.Add(new InstanceFile("TestFile2", 93));
            //InstalledFiles.Add(new InstanceFile("TestFile2", 50));
            //InstalledFiles.Add(new InstanceFile("TestFile2", 20));
            //InstalledFiles.Add(new InstanceFile("TestFile2", 21));

            if (instanceFormViewModel != null) 
            {
                Console.WriteLine("=====================================================================");
                _viewModel = instanceFormViewModel;
                instanceFormViewModel.Client.FileDownloadEvent += OnFileDownload;
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
                    Console.WriteLine(name + "  " + procents);
                }
                else 
                { 
                    if (process == DownloadFileProgress.Successful || process == DownloadFileProgress.Error)
                    {
                        DownloadFiles.Remove(instanceFile);
                        InstalledFiles.Add(instanceFile);
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

    public class DownloadManagerViewModel : ModalVMBase
    {
        private MainViewModel _mainViewModel;
        public ObservableCollection<InstanceDownloadProcess> InstanceDownloadProcessList { get; } = new ObservableCollection<InstanceDownloadProcess>();

        public override double Width => 500;
        public override double Height => base.Height + 30;

        public override RelayCommand CloseModalWindow => new RelayCommand(obj => 
        {
            _mainViewModel.ModalWindowVM.CloseWindow();
        });

        public DownloadManagerViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            //InstanceDownloadProcessList.Add(new InstanceDownloadProcess(null));
        }

        public void AddProcess(InstanceFormViewModel instanceForm) 
        {
            if (!InstanceDownloadProcess.Contains(InstanceDownloadProcessList, instanceForm))
                InstanceDownloadProcessList.Add(new InstanceDownloadProcess(instanceForm));
        }
    }
}
