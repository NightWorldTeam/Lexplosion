namespace Lexplosion.Logic.FileSystem.StorageManagment.DataHandlers
{
    class MinecraftServerHandler : JsonDataFile, IDataHandler<MinecraftServer>
    {
        private string _instanceId;

        public MinecraftServerHandler(string instanceId)
        {
            _instanceId = instanceId;
        }

        public void SaveToStorage(MinecraftServer data)
        {
            string dir = WithDirectory.DirectoryPath;
            base.SaveToFile<MinecraftServer>(data, dir + "/instances/" + _instanceId + "/" + "servers.json");
        }

        public MinecraftServer LoadFromStorage()
        {
            string dir = WithDirectory.DirectoryPath;
            return base.LoadFromFile<MinecraftServer>(dir + "/instances/" + _instanceId + "/" + "servers.json");
        }
    }

    class MinecraftServer
    {
        public string Name;
        public string Ip;
        public string Port;
    }

    struct MinecraftServerArgs : IDataHandlerArgs<MinecraftServer>
    {
        private MinecraftServerHandler _handler;
        public MinecraftServerArgs(string instanceId)
        {
            _handler = new MinecraftServerHandler(instanceId);
        }

        public IDataHandler<MinecraftServer> Handler { get => _handler; }
    }
}
