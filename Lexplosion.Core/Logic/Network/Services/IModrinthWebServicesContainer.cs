using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.Network.Services
{
	public interface IModrinthWebServicesContainer : IWebServicesContainer
	{
		public ModrinthApi MdApi { get; }
	}
}
