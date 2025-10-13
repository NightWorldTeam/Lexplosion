using Lexplosion.Logic.Network.Services;

namespace Lexplosion.Logic.FileSystem.Services
{
    public interface IFileServicesContainer : IWebServicesContainer
    {
        public WithDirectory DirectoryService { get; }
        public DataFilesManager DataFilesService { get; }
    }
}
