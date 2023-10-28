using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceCatalogControllers
{
    public interface IInstanceCatalog
    {
        public IEnumerable<InstanceModelBase> Instances { get; }

        public void Add(InstanceModelBase instanceModelBase);
        public InstanceModelBase GetInstance(InstanceClient instanceClient);
    }
}
