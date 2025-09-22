using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public sealed class InstanceGroupsConfiguratorViewModel : ActionModalViewModelBase
    {
        public InstanceGroupsConfiguratorModel Model { get; }


        public InstanceGroupsConfiguratorViewModel(InstanceModelBase instanceModel, ClientsManager clientsManager)
        {
            Model = new InstanceGroupsConfiguratorModel(instanceModel, clientsManager);
            ActionCommandExecutedEvent += OnActionCommandExecutedEvent;
        }


        private void OnActionCommandExecutedEvent(object obj)
        {
            Model.SaveChanges();
        }
    }
}
