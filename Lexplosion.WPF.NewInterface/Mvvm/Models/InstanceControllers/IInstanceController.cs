using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public interface IInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;

        public IReadOnlyCollection<InstanceModelBase> Instances { get; }

        public void Add(InstanceModelBase instanceModelBase, [CallerMemberName] string member = "");
        public void Add(InstanceClient instanceClient, [CallerMemberName] string member = "");
        public void Remove(InstanceModelBase instanceModelBase);
        public void Remove(InstanceClient instanceClient);
        public void Clear();
    }
}
