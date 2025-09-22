using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Mvvm.Models.Modal;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
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
