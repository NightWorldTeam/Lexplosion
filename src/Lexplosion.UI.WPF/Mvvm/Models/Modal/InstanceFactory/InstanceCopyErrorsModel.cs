using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public class InstanceCopyErrorsModel : ObservableObject 
    {
        public string InstanceName { get; }
        public string Description { get; }
        public IEnumerable<InstanceAddon> UncopiedAddons { get; private set; }

        public InstanceCopyErrorsModel(AppCore appCore, string instanceName, IEnumerable<InstanceAddon> uncopiedAddons)
        {
            InstanceName = instanceName;
            Description = string.Format(appCore.Resources["InstanceCopyUncopiedAddonsDescription"] as string, instanceName);
            UncopiedAddons = uncopiedAddons;
        }
    }
}
