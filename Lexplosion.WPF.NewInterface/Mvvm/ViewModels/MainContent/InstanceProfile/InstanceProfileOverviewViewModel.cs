using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewModel : ViewModelBase
    {
        public InstanceModelBase InstanceModel { get; }
        public InstanceData InstanceData { get => InstanceModel.PageData; }
        public BaseInstanceData BaseInstanceData { get; }

        public InstanceProfileOverviewModel(InstanceModelBase instanceModel)
        {
            InstanceModel = instanceModel;
            BaseInstanceData = instanceModel.InstanceData;
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
