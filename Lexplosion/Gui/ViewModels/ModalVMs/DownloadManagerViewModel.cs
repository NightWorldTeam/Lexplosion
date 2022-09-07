using Lexplosion.Gui.ModalWindow;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class DownloadManagerViewModel : ModalVMBase
    {
        private MainViewModel _mainViewModel;
        public ObservableCollection<InstanceDownloadProcess> InstanceDownloadProcessList { get; } = new ObservableCollection<InstanceDownloadProcess>();

        public override double Width => 500;
        public override double Height => base.Height + 30;

        public override RelayCommand CloseModalWindowCommand => new RelayCommand(obj => 
        {
            _mainViewModel.ModalWindowVM.CloseWindow();
        });

        public DownloadManagerViewModel(MainViewModel mainViewModel, bool IsTest = false)
        {
            _mainViewModel = mainViewModel;
        }

        public void AddProcess(InstanceFormViewModel instanceForm) 
        {
            if (!InstanceDownloadProcess.Contains(InstanceDownloadProcessList, instanceForm))
                InstanceDownloadProcessList.Add(new InstanceDownloadProcess(instanceForm));
        }
    }
}
