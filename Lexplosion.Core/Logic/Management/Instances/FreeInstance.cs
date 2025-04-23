using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.FreeSource;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Instances
{
	class FreeInstance : PrototypeInstance
	{
		private Func<FreeSourcePlatformData, SourceMap> _urlGetter;

		public FreeInstance(Func<FreeSourcePlatformData, SourceMap> urlGetter)
		{
			_urlGetter = urlGetter;
		}

		public override bool CheckUpdates(string localId)
		{
			try
			{
				var infoData = DataFilesManager.GetExtendedPlatfromData<FreeSourcePlatformData>(localId);
				if (infoData == null || !infoData.IsValid())
				{
					return false;
				}

				string url = _urlGetter(infoData)?.GetModpackVersionsListUrl(infoData.id);
				if (url == null)
				{
					return false;
				}

				string result = ToServer.HttpPost(url);
				if (result == null)
				{
					return false;
				}

				var version = JsonConvert.DeserializeObject<MidpackVersionsList>(result);
				if (string.IsNullOrWhiteSpace(version?.LatestVersion) || infoData.instanceVersion == version.LatestVersion)
				{
					return false;
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		public override InstanceData GetFullInfo(string localId, string externalId)
		{
			if (localId == null) return null;

			var content = DataFilesManager.GetExtendedPlatfromData<FreeSourcePlatformData>(localId);
			string url = _urlGetter(content)?.GetModpackManifestUrl(externalId);

			ModpackManifest manifest = null;
			try
			{
				string result = ToServer.HttpGet(url);
				manifest = JsonConvert.DeserializeObject<ModpackManifest>(result);
			}
			catch { }

			var images = new List<byte[]>();
			if (manifest?.Images != null && manifest.Images.Count > 0)
			{
				var perfomer = new TasksPerfomer(3, manifest.Images.Count);
				foreach (var item in manifest.Images)
				{
					perfomer.ExecuteTask(delegate ()
					{
						using (var webClient = new WebClient())
						{
							try
							{
								webClient.Proxy = null;
								images.Add(webClient.DownloadData(item));
							}
							catch { }
						}
					});
				}

				perfomer.WaitEnd();
			}

			return new InstanceData
			{
				Source = InstanceSource.FreeSource,
				Categories = new List<IProjectCategory>(),
				Description = manifest?.Description,
				Summary = manifest?.Summary,
				TotalDownloads = 0,
				GameVersion = manifest?.GameVersion,
				LastUpdate = null,
				Modloader = manifest?.Core ?? ClientType.Vanilla,
				Images = images
			};
		}

		public override List<InstanceVersion> GetVersions(string externalId)
		{
			return null;
		}
	}
}
