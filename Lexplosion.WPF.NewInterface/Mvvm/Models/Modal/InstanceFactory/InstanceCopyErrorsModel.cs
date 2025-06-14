using Lexplosion.Logic.Management.Addons;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class InstanceCopyErrorsModel : ObservableObject 
    {
        public IEnumerable<InstanceAddon> UncopiedAddons { get; private set; }

        public InstanceCopyErrorsModel(IEnumerable<InstanceAddon> uncopiedAddons)
        {
            UncopiedAddons = uncopiedAddons;
        }
    }
}
