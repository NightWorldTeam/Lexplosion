using Lexplosion.Gui.ModalWindow;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.ViewModels.ModalVMs
{
    public sealed class DownloadManagerViewModel : ModalVMBase
    {
        public ObservableCollection<InstanceDownloadProcess> InstanceDownloadProcessList { get; } = new ObservableCollection<InstanceDownloadProcess>();

        public override double Width => 500;
        public override double Height => base.Height + 30;

        public override RelayCommand CloseModalWindowCommand => new RelayCommand(obj =>
        {
            ModalWindowViewModelSingleton.Instance.Close();
        });

        public DownloadManagerViewModel(bool IsTest = false)
        {
        }

        public void AddProcess(InstanceFormViewModel instanceForm)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!InstanceDownloadProcess.Contains(InstanceDownloadProcessList, instanceForm))
                    InstanceDownloadProcessList.Add(new InstanceDownloadProcess(instanceForm));
            });
        }
    }
}
