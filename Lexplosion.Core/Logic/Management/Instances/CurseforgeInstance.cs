using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Instances
{
	class CurseforgeInstance : PrototypeInstance
	{
		private readonly ICurseforgeFileServicesContainer _services;

		public CurseforgeInstance(ICurseforgeFileServicesContainer services)
		{
			_services = services;
		}

		public override bool CheckUpdates(string localId)
		{
			var infoData = _services.DataFilesService.GetPlatfromData(localId);
			if (string.IsNullOrWhiteSpace(infoData?.id))
			{
				return false;
			}

			var content = _services.DataFilesService.GetInstanceContent(localId);
			if (content != null && !content.FullClient)
			{
				return true;
			}

			if (!Int32.TryParse(infoData.id, out _))
			{
				return true;
			}

			List<CurseforgeFileInfo> instanceVersionsInfo = _services.CfApi.GetProjectFiles(infoData.id); //получем информацию об этом модпаке

			//проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии 
			foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
			{
				if (ver.id > infoData.instanceVersion.ToInt32())
				{
					return true;
				}
			}

			return false;
		}

		public override InstanceData GetFullInfo(string localId, string externalId)
		{
			var data = _services.CfApi.GetInstance(externalId);

			if (data == null)
			{
				return null;
			}

			var images = new List<byte[]>();
			if (data.screenshots != null && data.screenshots.Count > 0)
			{
				var perfomer = new TasksPerfomer(3, data.screenshots.Count);
				foreach (var item in data.screenshots)
				{
					perfomer.ExecuteTask(delegate ()
					{
						using (var webClient = new WebClient())
						{
							try
							{
								webClient.Proxy = null;
								images.Add(webClient.DownloadData(item.url));
							}
							catch { }
						}
					});
				}

				perfomer.WaitEnd();
			}

			int? projectFileId = null;
			if (data.latestFilesIndexes?.Count > 0)
			{
				projectFileId = data.latestFilesIndexes[0]?.fileId;
			}

			string date;
			try
			{
				date = (data.dateModified != null) ? DateTime.Parse(data.dateModified).ToString("dd MMM yyyy") : "";
			}
			catch
			{
				date = "";
			}

			return new InstanceData
			{
				Source = InstanceSource.Curseforge,
				Categories = data.categories,
				Description = data.summary,
				Summary = data.summary,
				TotalDownloads = (long)data.downloadCount,
				GameVersion = (data.latestFilesIndexes != null && data.latestFilesIndexes.Count > 0) ? data.latestFilesIndexes[0].gameVersion : "",
				LastUpdate = date,
				Modloader = data.ModloaderType,
				Images = images,
				WebsiteUrl = data.links?.websiteUrl,
				Changelog = (projectFileId != null) ? (_services.CfApi.GetProjectChangelog(externalId, projectFileId?.ToString()) ?? "") : ""
			};
		}

		public override List<InstanceVersion> GetVersions(string externalId)
		{
			List<CurseforgeFileInfo> files = _services.CfApi.GetProjectFiles(externalId);

			HashSet<string> clientTypes = new(Enum.GetNames(typeof(ClientType)).Select(x => x.ToLower()));

			if (files != null)
			{
				var versions = new List<InstanceVersion>();
				foreach (var file in files)
				{
					ReleaseType status;
					if (file.releaseType == 1)
					{
						status = ReleaseType.Release;
					}
					else if (file.releaseType == 2)
					{
						status = ReleaseType.Beta;
					}
					else
					{
						status = ReleaseType.Alpha;
					}

					// Example: 1.20.1, Fabric
					var gameVersions = file.gameVersions.OrderBy(s => s);

					string allegedModloader = gameVersions.LastOrDefault();
					string modloader = clientTypes.Contains(allegedModloader?.ToLower()) ? allegedModloader : "—";

					versions.Add(new InstanceVersion
					{
						FileName = file.fileName,
						Id = file.id.ToString(),
						Status = status,
						Date = file.fileDate,
						GameVersion = gameVersions.FirstOrDefault() ?? "—",
						Modloader = modloader
					});

					Runtime.DebugWrite(file.fileName + " " + file.id);
				}

				return versions;
			}
			else
			{
				return null;
			}
		}
	}
}
