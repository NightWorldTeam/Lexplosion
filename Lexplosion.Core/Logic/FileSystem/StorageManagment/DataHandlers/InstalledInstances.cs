using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;

namespace Lexplosion.Logic.FileSystem.StorageManagment.DataHandlers
{
    class InstalledInstancesHandler : JsonDataFile, IDataHandler<InstalledInstances>
    {
        private string _fileName = WithDirectory.DirectoryPath + "/instanesList.json";

        public void SaveToStorage(InstalledInstances data)
        {
            base.SaveToFile<InstalledInstances>(data, _fileName);
        }

        public InstalledInstances LoadFromStorage()
        {
            return base.LoadFromFile<InstalledInstances>(_fileName);
        }
    }

    class InstalledInstances : Dictionary<string, InstalledInstance>
    {
    }

    struct InstalledInstancesArgs : IDataHandlerArgs<InstalledInstances>
    {
        private InstalledInstancesHandler _handler;
        public InstalledInstancesArgs()
        {
            _handler = new InstalledInstancesHandler();
        }

        public IDataHandler<InstalledInstances> Handler { get => _handler; }
    }
}
