using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.FileSystem.StorageManagment.DataHandlers
{
    class InstalledAddonsFormatHandler : JsonDataFile, IDataHandler<InstalledAddonsFormat>
    {
        private string _fileName;
        public InstalledAddonsFormatHandler(string instanceId)
        {
            _fileName = WithDirectory.DirectoryPath + "/instances/" + instanceId + "/installedAddons.json";
        }

        public void SaveToStorage(InstalledAddonsFormat data)
        {
            base.SaveToFile<InstalledAddonsFormat>(data, _fileName);
        }

        public InstalledAddonsFormat LoadFromStorage()
        {
            return base.LoadFromFile<InstalledAddonsFormat>(_fileName) ?? new InstalledAddonsFormat();
        }
    }

    struct InstalledAddonsFormatArgs : IDataHandlerArgs<InstalledAddonsFormat>
    {
        private InstalledAddonsFormatHandler _handler;
        public InstalledAddonsFormatArgs(string instanceId)
        {
            _handler = new InstalledAddonsFormatHandler(instanceId);
        }

        public IDataHandler<InstalledAddonsFormat> Handler { get => _handler; }
    }
}
