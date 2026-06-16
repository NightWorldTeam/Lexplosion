using Lexplosion.Logic.Management.Localization;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Services;

namespace Lexplosion.Logic.FileSystem.Services
{
	public interface IFileServicesContainer : IWebServicesContainer, ILocalizationServicesContainer
	{
		public WithDirectory DirectoryService { get; }
		public DataFilesManager DataFilesService { get; }
	}
}
