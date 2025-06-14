using Lexplosion.Logic.Management.Addons;
using Lexplosion.WPF.NewInterface.Core.Modal;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{

    public sealed class InstanceCopyErrorsViewModel : ModalViewModelBase
    {
        public InstanceCopyErrorsModel Model { get; }

        public InstanceCopyErrorsViewModel(IEnumerable<InstanceAddon> uncopiedAddons)
        {
            Model = new(uncopiedAddons);
        }
    }
}
