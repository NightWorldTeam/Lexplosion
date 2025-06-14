using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    public class InstanceModelArgs 
    {
        public AppCore AppCore;
        public InstanceClient InstanceClient;
        public Action<InstanceClient> ExportFunc; 
        public Action<InstanceModelBase> SetRunningGame;
        public InstanceDistribution InstanceDistribution = null; 
        public ImportData? ImportData = null;
        public InstancesGroup? Group;
        public InstanceLocation Location;
        public Action<InstanceClient>? AddToLibraryByInstanceClient = null;

        public InstanceModelArgs()
        {
            
        }

        public InstanceModelArgs(
            AppCore appCore,
            InstanceClient instanceClient, 
            Action<InstanceClient> exportFunc, 
            Action<InstanceModelBase> setRunningGame,
            InstanceDistribution instanceDistribution = null, 
            ImportData? importData = null, 
            InstancesGroup? group = null, 
            InstanceLocation instanceLocation = InstanceLocation.Library,
            Action<InstanceClient> addByInstanceClient = null)
        {
            AppCore = appCore;
            InstanceClient = instanceClient;
            ExportFunc = exportFunc;
            SetRunningGame = setRunningGame;
            InstanceDistribution = instanceDistribution;
            ImportData = importData;
            Group = group;
            Location = instanceLocation;
            AddToLibraryByInstanceClient = addByInstanceClient;
        }
    }
}