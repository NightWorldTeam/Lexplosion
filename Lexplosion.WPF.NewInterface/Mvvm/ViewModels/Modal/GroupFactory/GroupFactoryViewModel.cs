using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal
{
    public class GroupFactoryModel : ViewModelBase 
    {
        public string Name { get; }
        public IEnumerable<SelecteableItem> AvailableUngroupedInstances { get; }


        public GroupFactoryModel(IEnumerable<InstanceClient> availableInstances = null)
        {
            
        }


        public void CreateGroup() 
        {
            /*var newInstanceGroup = new InstanceGroup();
            
            newInstanceGroup.Save();*/
        }
    }

    internal class GroupFactoryViewModel : ActionModalViewModelBase
    {
        public GroupFactoryModel Model { get; }


        public GroupFactoryViewModel()
        {
            Model = new GroupFactoryModel();
        }
    }
}
