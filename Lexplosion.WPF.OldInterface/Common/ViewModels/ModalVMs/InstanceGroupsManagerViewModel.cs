using Lexplosion.Common.ModalWindow;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public class InstanceGroupsManagerViewModel : ModalVMBase
    {
        public override RelayCommand CloseModalWindowCommand => new RelayCommand(obj =>
        {
            ModalWindowViewModelSingleton.Instance.Close();
        });
    }
}
