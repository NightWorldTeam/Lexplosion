using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class ModrinthAddonPageViewModel : ViewModelBase
    {
        public ModrinthAddonPageModel Model { get; }

        public ModrinthAddonPageViewModel(InstanceAddon instanceAddon)
        {
            Model = new ModrinthAddonPageModel(instanceAddon);
        }
    }
}
