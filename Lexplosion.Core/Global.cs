using System;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;

namespace Lexplosion.Global
{
	public static class GlobalData
	{
		public static Settings GeneralSettings { get; private set; } // инициализируется в методе Main

		public static void InitSetting(DataFilesManager dataFilesManager)
		{
			GeneralSettings = Settings.GetDefault();
			var loadedSettings = dataFilesManager.GetSettings();
			GeneralSettings.Merge(loadedSettings);
			Runtime.DebugWrite($"GamePath: {GlobalData.GeneralSettings?.GamePath}, theme: {GlobalData.GeneralSettings?.ThemeName}");
		}
	}

	public static class LaunсherSettings
	{
		public struct URL
		{
			public const string ModpacksData = "https://night-world.org/minecraft/modpacks/";
			public const string VersionsData = "https://night-world.org/minecraft/versions/";
			public const string InstallersData = "https://night-world.org/minecraft/additionalInstallers/";
			public const string JavaData = "https://launchermeta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";
			public const string Upload = "https://night-world.org/minecraft/upload/";
			public const string LauncherParts = "https://night-world.org/assets/launcher/windows/";
			public const string LauncherPartsMirror = "https://mirror.night-world.org/assets/launcher/windows/";
			public const string UserApi = "https://night-world.org/api/user/";
			public const string Base = "https://night-world.org/";
			public const string MirrorBase = "https://mirror.night-world.org/";
			public const string Account = "https://night-world.org/api/account/";
			public const string MirrorUrl = "https://night-world.org/mirror/";
		}

		public const string GAME_FOLDER_NAME = "lexplosion";
		public static string LauncherDataPath = Environment.ExpandEnvironmentVariables("%appdata%") + "/lexplosion-data";
		public static string gamePath = Environment.ExpandEnvironmentVariables("%appdata%") + "/." + GAME_FOLDER_NAME;

		public const string secretWord = "iDRCQxDMwGVCjWVe0ZEJ4u9DeG38BNL52x777trQ"; // на самом деле нихуя не сикрет
		public const string passwordKey = "ZEmMJ0ZaXQXuHu8tUnfdaCLCQaFgRjOP";
		public const int version = 1754607991;
		public const int CommandServerPort = 54352;
		public const string DiscordAppID = "839856058703806484";
		public const string ServerIp = "rtc.night-world.org";
	}
}
