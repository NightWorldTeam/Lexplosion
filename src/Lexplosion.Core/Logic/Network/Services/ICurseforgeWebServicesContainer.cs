using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.Network.Services
{
	public interface ICurseforgeWebServicesContainer : IWebServicesContainer
	{
		public CurseforgeApi CfApi { get; }
	}
}
