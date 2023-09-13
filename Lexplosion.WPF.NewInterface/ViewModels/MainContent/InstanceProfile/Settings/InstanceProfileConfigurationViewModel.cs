using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileConfigurationModel : ViewModelBase 
    {
        public InstanceProfileConfigurationModel(InstanceModelBase instanceModelBase)
        {

        }
    }

    public sealed class InstanceProfileConfigurationViewModel : ViewModelBase
    {
        public InstanceProfileConfigurationModel Model { get; }

        public InstanceProfileConfigurationViewModel(InstanceModelBase instanceModelBase)
        {
            Model = new InstanceProfileConfigurationModel(instanceModelBase);
        }
    }
}
