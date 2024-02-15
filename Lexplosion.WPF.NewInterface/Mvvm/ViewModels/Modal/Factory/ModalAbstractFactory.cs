using Lexplosion.WPF.NewInterface.Core.Modal;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.Factory
{
    public abstract class ModalAbstractFactory
    {
        public enum ModalPage
        {
            InstanceFactory,
            InstanceExport,
            ServerOverview
        }

        public abstract IModalViewModel Create();
    }
}
