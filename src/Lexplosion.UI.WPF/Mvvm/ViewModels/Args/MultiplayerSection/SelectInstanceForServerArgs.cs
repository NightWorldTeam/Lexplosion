using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Args
{
    public class SelectInstanceForServerArgs
    {
        public readonly Func<IEnumerable<InstanceModelBase>> GetLibraryInstances;
        public readonly Func<InstanceClient, InstanceModelBase> PrepareLibraryAndGetInstanceModelBase;

        public SelectInstanceForServerArgs(Func<IEnumerable<InstanceModelBase>> getLibraryInstances, Func<InstanceClient, InstanceModelBase> prepareLibraryAndGetInstanceModelBase)
        {
            GetLibraryInstances = getLibraryInstances;
            PrepareLibraryAndGetInstanceModelBase = prepareLibraryAndGetInstanceModelBase;
        }
    }
}
