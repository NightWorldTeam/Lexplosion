namespace Lexplosion.Logic.DTO
{
	public record LauncherInitInfo
	{
		public readonly int LauncherVersion;
		public readonly long LatestNewsId;

		public LauncherInitInfo(int launcherVersion, long latestNewsId)
		{
			LauncherVersion = launcherVersion;
			LatestNewsId = latestNewsId;
		}
	}
}
