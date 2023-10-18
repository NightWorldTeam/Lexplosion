using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewModel : ViewModelBase 
    {
        public InstanceProfileOverviewModel(InstanceModelBase instanceModel)
        {

        }
    }

    public sealed class InstanceProfileOverviewViewModel : ViewModelBase
    {
        public InstanceProfileOverviewModel Model { get; }

        public InstanceProfileOverviewViewModel(InstanceModelBase instanceModel)
        {
            Model = new InstanceProfileOverviewModel(instanceModel);
        }
    }
}
