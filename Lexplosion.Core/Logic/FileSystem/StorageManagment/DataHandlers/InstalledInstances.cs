using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;

namespace Lexplosion.Logic.FileSystem.StorageManagment.DataHandlers
{
    class InstalledInstancesHandler : JsonDataStorage<InstalledInstances>, IFileStorage
    {
        public string FilePath { get => WithDirectory.DirectoryPath + "/instanesList.json"; }
    }

    class InstalledInstances : Dictionary<string, InstalledInstance>
    {
    }
}
