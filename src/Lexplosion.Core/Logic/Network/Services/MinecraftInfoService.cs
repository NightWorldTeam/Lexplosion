using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Network.Services
{
	public class MinecraftInfoService
	{
		private readonly ToServer _toServer;

		public MinecraftInfoService(ToServer toServer)
		{
			_toServer = toServer;
		}

		public JavaVersionManifest GetJavaVersions()
		{
			string answer = null;
			try
			{
				try
				{
					answer = _toServer.HttpGet(LaunсherSettings.URL.JavaData);
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite(ex);
				}

				if (answer == null)
				{
					string url = LaunсherSettings.URL.MirrorUrl + LaunсherSettings.URL.JavaData.Replace("https://", "");
					Runtime.DebugWrite("Try mirror, url " + url);
					answer = _toServer.HttpGet(url);
				}

				return JsonConvert.DeserializeObject<JavaVersionManifest>(answer);
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("answer is null " + (answer == null) + ", exception: " + ex);
				return null;
			}
		}

		public JavaFiles GetJavaFilesList(JavaVersion version, out string manifestString)
		{			
			manifestString = null;
			try
			{
				try
				{
					manifestString = _toServer.HttpGet(version.ManifestUrl);
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite(ex);
				}

				if (manifestString == null)
				{
					string url = LaunсherSettings.URL.MirrorUrl + version.ManifestUrl.Replace("https://", "");
					Runtime.DebugWrite("Try mirror, url " + url);
					manifestString = _toServer.HttpGet(url);
				}

				return JsonConvert.DeserializeObject<JavaFiles>(manifestString);
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("answer is null " + (manifestString == null) + ", exception: " + ex);
				return null;
			}

		}

		public List<MCVersionInfo> GetVersionsList()
		{
			bool isMirrorMode = _toServer.IsMirrorModeToNw;
			try
			{
				string answer = _toServer.HttpGet(LaunсherSettings.URL.VersionsData, timeout: 20000);
				if (answer != null)
				{
					List<MCVersionInfo> data = JsonConvert.DeserializeObject<List<MCVersionInfo>>(answer);
					return data ?? new List<MCVersionInfo>();
				}
				else
				{
					if (!isMirrorMode)
					{
						_toServer.ChangeToMirrorMode();
						return GetVersionsList();
					}

					return new List<MCVersionInfo>();
				}
			}
			catch
			{
				return new List<MCVersionInfo>();
			}

		}

		public List<string> GetModloadersList(string gameVersion, ClientType modloaderType)
		{
			string modloader;
			if (modloaderType != ClientType.Vanilla)
			{
				modloader = "/" + modloaderType.ToString().ToLower() + "/";
			}
			else
			{
				return new List<string>();
			}

			try
			{
				string url = LaunсherSettings.URL.VersionsData + gameVersion + modloader;
				string answer = _toServer.HttpGet(url);
				if (answer != null)
				{
					List<string> data = JsonConvert.DeserializeObject<List<string>>(answer);
					Runtime.DebugWrite($"Return {gameVersion} {modloaderType}, Count: {data.Count}");
					return data ?? new List<string>();
				}
				else
				{
					return new List<string>();
				}
			}
			catch
			{
				return new List<string>();
			}
		}

		public async Task<int> GetMcServerOnline(MinecraftServerInstance server)
		{
			int count = -1;
			await Task.Run(() =>
			{
				string result = _toServer.HttpGet($"https://api.mcsrvstat.us/3/{server.Address}");
				if (result == null) return;

				try
				{
					var data = JsonConvert.DeserializeObject<McServerOnlineData>(result);
					if (data?.Players != null)
					{
						count = data.Players.Online;
					}
				}
				catch { }
			});

			return count;
		}

		public List<MinecraftServerInstance> GetMinecraftServersList()
		{
			string result = _toServer.HttpGet(LaunсherSettings.URL.Base + "api/minecraftServers/list");

			if (result == null) return new List<MinecraftServerInstance>();

			try
			{
				var data = JsonConvert.DeserializeObject<List<MinecraftServerInstance>>(result);

				for (int i = 0; i < data.Count; i++)
				{
					MinecraftServerInstance element = data[i];
					if (!element.IsValid()) data.Remove(element);
				}

				Random rand = new Random();
				rand.Shuffle(data);
				return data;
			}
			catch
			{
				return new List<MinecraftServerInstance>();
			}
		}

		public List<string> GetOptifineVersions(string gameVersion)
		{
			try
			{
				string answer = _toServer.HttpGet(LaunсherSettings.URL.InstallersData + gameVersion + "/optifine");
				if (answer != null)
				{
					List<string> data = JsonConvert.DeserializeObject<List<string>>(answer);
					return data ?? new List<string>();
				}
				else
				{
					return new List<string>();
				}
			}
			catch
			{
				return new List<string>();
			}
		}

		public HashSet<string> GetNwClientGameVersions()
		{
			try
			{
				string answer = _toServer.HttpGet(LaunсherSettings.URL.Base + "/minecraft/nwClientVersions");
				var res = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(answer);

				return new HashSet<string>(res.Keys);
			}
			catch
			{
				return new HashSet<string>();
			}
		}

		/// <summary>
		/// Получает маниест для майкрафт версии
		/// </summary>
		/// <param name="version">Версия майнкрафта</param>
		/// <param name="clientType">тип клиента</param>
		/// <param name="isNwClient">Включен ли NightWorldClient</param>
		/// <param name="modloaderVersion">Версия модлоадера, если он передан в clientType</param>
		/// <param name="optifineVersion">Версия оптифайна, если он не нужен то null</param>
		public VersionManifest GetVersionManifest(string version, ClientType clientType, bool isNwClient, string modloaderVersion = null, string optifineVersion = null)
		{
			try
			{
				string modloaderUrl = "";
				if (!string.IsNullOrWhiteSpace(modloaderVersion))
				{
					if (clientType != ClientType.Vanilla)
					{
						modloaderUrl = "/" + clientType.ToString().ToLower() + "/";
						modloaderUrl += modloaderVersion;
					}
				}

				var filesData = _toServer.ProtectedRequest<ProtectedVersionManifest>(LaunсherSettings.URL.VersionsData + WebUtility.UrlEncode(version) + modloaderUrl, 20000);
				if (filesData?.version != null)
				{
					if (isNwClient && filesData.version.NightWorldClientData != null)
					{
						filesData.version.IsNightWorldClient = true;
					}

					Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
					foreach (string lib in filesData.libraries.Keys)
					{
						if (filesData.libraries[lib].os == null || filesData.libraries[lib].os.Contains("windows"))
						{
							libraries[lib] = filesData.libraries[lib].GetLibInfo;
						}
					}

					VersionManifest manifest = new VersionManifest
					{
						version = filesData.version,
						libraries = libraries
					};

					if (optifineVersion != null)
					{
						var optifineData = _toServer.ProtectedRequest<ProtectedInstallerManifest>(LaunсherSettings.URL.InstallersData + WebUtility.UrlEncode(version) + "/optifine/" + optifineVersion, 20000);
						if (optifineData == null) return null;

						foreach (string lib in optifineData.libraries.Keys)
						{
							if (optifineData.libraries[lib].os == null || optifineData.libraries[lib].os.Contains("windows"))
							{
								libraries[lib] = optifineData.libraries[lib].GetLibInfo;
								libraries[lib].additionalInstallerType = AdditionalInstallerType.Optifine;
							}
						}

						optifineData.version.installerVersion = optifineVersion;
						manifest.version.AdditionalInstaller = optifineData.version;
					}

					return manifest;
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
			}

			return null;
		}
	}
}
