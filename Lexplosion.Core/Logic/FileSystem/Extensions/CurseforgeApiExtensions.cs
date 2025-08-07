using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.FileSystem.Extensions
{
	public static class CurseforgeApiExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static SetValues<InstalledAddonInfo, DownloadAddonRes> InstallAddon(AddonType addonType, string fileUrl, string fileName, string path, string folderName, string projectID, string fileID, WithDirectory withDirecory, TaskArgs taskArgs)
		{
			if (addonType != AddonType.Maps)
			{
				if (!withDirecory.InstallFile(fileUrl, fileName, path + folderName, taskArgs))
				{
					return new SetValues<InstalledAddonInfo, DownloadAddonRes>
					{
						Value1 = null,
						Value2 = taskArgs.CancelToken.IsCancellationRequested ? DownloadAddonRes.IsCanselled : DownloadAddonRes.DownloadError
					};
				}

				Runtime.DebugWrite("SYS " + fileUrl);
			}
			else
			{
				if (!withDirecory.InstallZipContent(fileUrl, fileName, path + folderName, taskArgs))
				{
					return new SetValues<InstalledAddonInfo, DownloadAddonRes>
					{
						Value1 = null,
						Value2 = taskArgs.CancelToken.IsCancellationRequested ? DownloadAddonRes.IsCanselled : DownloadAddonRes.DownloadError
					};
				}

				Runtime.DebugWrite("SYS " + fileUrl);
			}

			return new SetValues<InstalledAddonInfo, DownloadAddonRes>
			{
				Value1 = new InstalledAddonInfo
				{
					ProjectID = projectID,
					FileID = fileID,
					Path = (addonType != AddonType.Maps) ? (folderName + "/" + fileName) : (folderName + "/"),
					Type = addonType,
					Source = ProjectSource.Curseforge

				},
				Value2 = DownloadAddonRes.Successful
			};
		}

		public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(this CurseforgeApi api, CurseforgeFileInfo addonInfo, AddonType addonType, string path, WithDirectory withDirectory, TaskArgs taskArgs)
		{
			Runtime.DebugWrite("PR ID " + addonInfo.id);
			string projectID = addonInfo.modId;
			string fileID = addonInfo.id.ToString();
			try
			{
				Runtime.DebugWrite("fileData " + addonInfo.downloadUrl + " " + projectID + " " + fileID);

				string fileUrl = addonInfo.downloadUrl;
				string fileName = addonInfo.fileName;

				if (String.IsNullOrWhiteSpace(addonInfo.downloadUrl))
				{
					return new SetValues<InstalledAddonInfo, DownloadAddonRes>
					{
						Value1 = null,
						Value2 = DownloadAddonRes.UrlError
					};
				}

				Runtime.DebugWrite(fileUrl);

				// проверяем имя файла на валидность
				char[] invalidFileChars = Path.GetInvalidFileNameChars();
				bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

				if (isInvalidFilename)
				{
					return new SetValues<InstalledAddonInfo, DownloadAddonRes>
					{
						Value1 = null,
						Value2 = DownloadAddonRes.FileNameError
					};
				}

				// определяем папку в которую будет установлен данный аддон
				string folderName = "";
				switch (addonType)
				{
					case AddonType.Mods:
						folderName = "mods";
						break;
					case AddonType.Maps:
						folderName = "saves";
						break;
					case AddonType.Resourcepacks:
					case AddonType.DataPacks:
						folderName = "resourcepacks";
						break;
					case AddonType.Shaders:
						folderName = "shaderpacks";
						break;
					default:
						return new SetValues<InstalledAddonInfo, DownloadAddonRes>
						{
							Value1 = null,
							Value2 = DownloadAddonRes.unknownAddonType
						};
				}

				// устанавливаем
				return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, withDirectory, taskArgs);
			}
			catch
			{
				return new SetValues<InstalledAddonInfo, DownloadAddonRes>
				{
					Value1 = null,
					Value2 = DownloadAddonRes.unknownError
				};
			}
		}

		public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(this CurseforgeApi api, CurseforgeAddonInfo addonInfo, string fileID, string path, WithDirectory withDirectory, TaskArgs taskArgs)
		{
			try
			{
				string projectID = addonInfo.id;
				Runtime.DebugWrite("");
				Runtime.DebugWrite("PR ID " + projectID);

				if (addonInfo.latestFiles == null)
				{
					return new SetValues<InstalledAddonInfo, DownloadAddonRes>
					{
						Value1 = null,
						Value2 = DownloadAddonRes.ProjectDataError
					};
				}

				// получем информацию о файле
				CurseforgeFileInfo fileData = null;
				//ищем нужный файл
				foreach (CurseforgeFileInfo data in addonInfo.latestFiles)
				{
					if (data.id.ToString() == fileID)
					{
						fileData = data;
						break;
					}
				}
				//не нашли, делаем дополнительный запрос и получаем его
				if (fileData == null)
				{
					fileData = api.GetProjectFile(projectID, fileID);
				}

				Runtime.DebugWrite("fileData " + fileData.downloadUrl + " " + projectID + " " + fileID);

				string fileUrl = fileData.downloadUrl;
				if (String.IsNullOrWhiteSpace(fileUrl))
				{
					// пробуем второй раз
					fileData = api.GetProjectFile(projectID, fileID);
					if (String.IsNullOrWhiteSpace(fileData.downloadUrl))
					{
						Runtime.DebugWrite("URL ERROR - " + fileData.downloadUrl + " - " + fileData.fileName);
						return new SetValues<InstalledAddonInfo, DownloadAddonRes>
						{
							Value1 = null,
							Value2 = DownloadAddonRes.UrlError
						};
					}
				}

				Runtime.DebugWrite(fileUrl);

				string fileName = fileData.fileName;

				// проверяем имя файла на валидность
				char[] invalidFileChars = Path.GetInvalidFileNameChars();
				bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

				if (isInvalidFilename)
				{
					return new SetValues<InstalledAddonInfo, DownloadAddonRes>
					{
						Value1 = null,
						Value2 = DownloadAddonRes.FileNameError
					};
				}

				// определяем папку в которую будет установлен данный аддон
				string folderName = "";
				AddonType addonType = (AddonType)(addonInfo.classId ?? 0);
				switch (addonType)
				{
					case AddonType.Mods:
						folderName = "mods";
						break;
					case AddonType.Maps:
						folderName = "saves";
						break;
					case AddonType.Resourcepacks:
					case AddonType.DataPacks:
						folderName = "resourcepacks";
						break;
					case AddonType.Shaders:
						folderName = "shaderpacks";
						break;
					default:
						return new SetValues<InstalledAddonInfo, DownloadAddonRes>
						{
							Value1 = null,
							Value2 = DownloadAddonRes.unknownAddonType
						};
				}

				// устанавливаем
				return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, withDirectory, taskArgs);
			}
			catch
			{
				return new SetValues<InstalledAddonInfo, DownloadAddonRes>
				{
					Value1 = null,
					Value2 = DownloadAddonRes.unknownError
				};
			}
		}
	}
}
