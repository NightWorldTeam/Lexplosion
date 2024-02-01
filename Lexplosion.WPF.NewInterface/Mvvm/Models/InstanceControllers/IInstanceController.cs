using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public interface IInstanceController
    {
        public IReadOnlyCollection<InstanceModelBase> Instances { get; }

        public void Add(InstanceModelBase instanceModelBase);
        public void Add(InstanceClient instanceClient);
        public void Remove(InstanceModelBase instanceModelBase);
        public void Clear();
    }
}
