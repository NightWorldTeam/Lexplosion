using Lexplosion.Logic.Network.Services;

namespace Lexplosion.Logic
{
	public class CoreServicesManager
	{
		public static MinecraftInfoService MinecraftInfo { get => NetworkServicesManager.MinecraftInfo; }
	}
}
