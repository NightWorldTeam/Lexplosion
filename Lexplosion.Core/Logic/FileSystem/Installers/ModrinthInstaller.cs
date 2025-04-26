using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.FileSystem.Extensions;

namespace Lexplosion.Logic.FileSystem.Installers
{
	class ModrinthInstaller : StandartInstanceInstaller<InstanceManifest>
	{
		private ModrinthApi _modrinthApi;

		public ModrinthInstaller(string instanceId, IModrinthFileServicesContainer servicesContainer) : base(instanceId, servicesContainer) 
		{
			_modrinthApi = servicesContainer.MdApi;
		}

		protected override InstanceManifest LoadManifest(string unzupArchivePath)
		{
			return dataFilesManager.GetFile<InstanceManifest>(unzupArchivePath + "modrinth.index.json");
		}

		protected override void ArchiveHadnle(string unzupArchivePath, out List<string> files)
		{
			files = new List<string>();

			// тут переосим нужные файлы из этого архива

			string sourcePath = unzupArchivePath + "overrides/";
			string destinationPath = withDirectory.GetInstancePath(instanceId);

			if (!Directory.Exists(destinationPath))
			{
				Directory.CreateDirectory(destinationPath);
			}

			if (Directory.Exists(sourcePath))
			{
				foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
				{
					string dir = dirPath.Replace(sourcePath, destinationPath);
					if (!Directory.Exists(dir))
					{
						Directory.CreateDirectory(dir);
					}
				}

				foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
				{
					File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
					files.Add(newPath.Replace(sourcePath, "/").Replace("\\", "/"));
				}
			}
		}

		private struct AddonInstallingInfo
		{
			public ModrinthProjectType Type;
			public string FileName;
		}

		/// <summary>
		/// Скачивает все аддоны модпака из спика
		/// </summary>
		/// <returns>
		/// Возвращает список ошибок.
		/// </returns>
		public override List<string> Install(InstanceManifest data, InstanceContent localFiles, CancellationToken cancelToken)
		{
			InstalledAddonsFormat installedAddons = null;
			installedAddons = localFiles.InstalledAddons;

			var errors = new List<string>();

			try
			{
				InstanceContent compliteDownload = new InstanceContent
				{
					InstalledAddons = new InstalledAddonsFormat(),
					Files = localFiles.Files
				};

				// проходимя по весм файлам из манифеста и формируем список с хэшами.
				var filesHashes = new List<string>();
				foreach (InstanceManifest.FileData file in data.files)
				{
					if (file.hashes != null && file.hashes.ContainsKey("sha512"))
					{
						filesHashes.Add(file.hashes["sha512"]);
					}
				}

				var notableProjects = new Dictionary<InstanceManifest.FileData, SetValues<ModrinthProjectFile, AddonInstallingInfo>>();
				var unknownProjects = new List<InstanceManifest.FileData>(); // тут харним неизтные аддоны, которые потом установим по прямой ссылке
				{
					// получем спсиок с проектами по списоку хэшэй
					Dictionary<string, ModrinthProjectFile> projectFiles = _modrinthApi.GetFilesFromHashes(filesHashes);

					foreach (InstanceManifest.FileData file in data.files)
					{
						if ((file.hashes?.ContainsKey("sha512") ?? false) && projectFiles.ContainsKey(file.hashes["sha512"]))
						{
							// проверяем path файла
							if (file.Path != null)
							{
								string[] segments = file.Path.Split('/');
								if (segments.Length == 2) // path должен быть в виде: имя_папки/имя_файла
								{
									// проверяем имя файла на валидность
									if (Path.GetInvalidFileNameChars().Any(s => segments[1].Contains(s))) // если имя файла на валидно возращаем ошибку
									{
										// тут ошибка
										errors.Add("File: " + file.Path);
										Runtime.DebugWrite("ERROR " + file.Path);
										_fileDownloadHandler?.Invoke(file.Path, 100, DownloadFileProgress.Error);
									}

									string folderName = segments[0];
									// определяем тип аддона по папке, в которой он должен лежать.
									ModrinthProjectType addontype;
									switch (folderName)
									{
										case "mods":
											addontype = ModrinthProjectType.Mod;
											break;
										case "resourcepacks":
											addontype = ModrinthProjectType.Resourcepack;
											break;
										case "shaderpacks":
											addontype = ModrinthProjectType.Shader;
											break;
										default:
											// неизвестный тип аддона.
											unknownProjects.Add(file);
											continue;
									}

									notableProjects[file] = new SetValues<ModrinthProjectFile, AddonInstallingInfo>
									{
										Value1 = projectFiles[file.hashes["sha512"]],
										Value2 = new AddonInstallingInfo
										{
											Type = addontype,
											FileName = segments[1]
										}
									};
								}
								else
								{
									unknownProjects.Add(file);
								}
							}
							else
							{
								errors.Add("File: " + file.Path);
								Runtime.DebugWrite("ERROR " + file.Path);
								_fileDownloadHandler?.Invoke(file.Path, 100, DownloadFileProgress.Error);
							}
						}
						else
						{
							bool fileNameIsValid = false;

							try
							{
								if (file.Path != null)
								{
									string fileName = Path.GetFileName(file.Path);
									fileNameIsValid = !Path.GetInvalidFileNameChars().Any(s => fileName.Contains(s));
								}
							}
							catch { }

							if (fileNameIsValid && file.downloads != null && file.downloads.Count > 0)
							{
								unknownProjects.Add(file);
							}
							else
							{
								errors.Add("File: " + file.Path);
								Runtime.DebugWrite("ERROR " + file.Path);
								_fileDownloadHandler?.Invoke(file.Path, 100, DownloadFileProgress.Error);
							}
						}
					}
				}

				var downloadList = new List<SetValues<ModrinthProjectFile, AddonInstallingInfo>>();
				if (installedAddons != null)
				{
					var existsAddons = new HashSet<string>(); // этот список содержит айдишники аддонов, что действительно установлены и есть в списке с курсфорджа
					foreach (var file in notableProjects) // проходимся по списку адднов, полученному с курсфорджа
					{
						SetValues<ModrinthProjectFile, AddonInstallingInfo> addonData = file.Value;
						InstanceManifest.FileData key = file.Key;

						string projectId = addonData.Value1.ProjectId;
						if (!installedAddons.ContainsKey(projectId)) // если этого аддона нету в списке уже установленных, то тогда кидаем на обновление
						{
							downloadList.Add(addonData);
						}
						else
						{
							InstalledAddonInfo addonInfo = installedAddons[projectId];
							if (addonInfo.FileID != addonData.Value1.FileId || !addonInfo.IsExists(withDirectory.GetInstancePath(instanceId)))
							{
								// версия не сходится или нет файла. Тоже кидаем на обновление
								downloadList.Add(addonData);
							}
							else
							{
								existsAddons.Add(projectId); // Аддон есть в списке установленых. Добавляем его айдишник в список
							}
						}
					}

					foreach (string addonId in installedAddons.Keys) // проходимя по списку установленных аддонов
					{
						if (!existsAddons.Contains(addonId)) // если аддона нету в этом списке, значит его нету в списке, полученном с курсфорджа (ну или нам не подходит его версия, или же файла нету). Поэтому удаляем
						{
							if (installedAddons[addonId].ActualPath != null)
							{
								Runtime.DebugWrite("Delete file: " + withDirectory.GetInstancePath(instanceId) + installedAddons[addonId].ActualPath);
								withDirectory.DelFile(withDirectory.GetInstancePath(instanceId) + installedAddons[addonId].ActualPath);
							}
						}
						else
						{
							compliteDownload.InstalledAddons[addonId] = installedAddons[addonId];
						}
					}
				}
				else
				{
					downloadList = notableProjects.Values.ToList();
				}

				int filesCount = downloadList.Count;
				int downloadedCount = 0;
				int totalFilesCount = filesCount + unknownProjects.Count;

				AddonsDownloadEventInvoke(totalFilesCount, 0);

				if (filesCount != 0)
				{
					SaveInstanceContent(compliteDownload);

					object fileBlock = new object(); // этот объект блокировщик нужен что бы синхронизировать работу с json файлами

					TasksPerfomer perfomer = null;
					if (filesCount > 0)
						perfomer = new TasksPerfomer(3, filesCount);

					var noDownloaded = new List<SetValues<ModrinthProjectFile, AddonInstallingInfo>>();

					Runtime.DebugWrite("СКАЧАТЬ БЛЯТЬ НАДО " + downloadList.Count + " ЗЛОЕБУЧИХ МОДОВ");
					foreach (SetValues<ModrinthProjectFile, AddonInstallingInfo> addonData in downloadList)
					{
						perfomer.ExecuteTask(delegate ()
						{
							Runtime.DebugWrite("ADD MOD TO PERFOMER");

							var taskArgs = new TaskArgs
							{
								PercentHandler = delegate (int percent)
								{
									_fileDownloadHandler?.Invoke(addonData.Value2.FileName, percent, DownloadFileProgress.PercentagesChanged);
								},
								CancelToken = cancelToken
							};

							ModrinthProjectFile projectFile = addonData.Value1;
							var result = _modrinthApi.DownloadAddon(projectFile, addonData.Value2.Type, "/instances/" + instanceId + "/", addonData.Value2.FileName, withDirectory, taskArgs);

							_fileDownloadHandler?.Invoke(addonData.Value2.FileName, 100, DownloadFileProgress.Successful);

							if (result.Value2 == DownloadAddonRes.Successful)
							{
								downloadedCount++;
								AddonsDownloadEventInvoke(totalFilesCount, downloadedCount);

								lock (fileBlock)
								{
									compliteDownload.InstalledAddons[projectFile.ProjectId] = result.Value1;
									SaveInstanceContent(compliteDownload);
								}
							}
							else //скачивание мода не удалось.
							{
								Runtime.DebugWrite("ERROR " + result.Value2 + " " + result.Value1);
								noDownloaded.Add(addonData);
							}

							Runtime.DebugWrite("EXIT PERFOMER");
						});

						if (cancelToken.IsCancellationRequested) break;
					}

					if (!cancelToken.IsCancellationRequested)
					{
						perfomer?.WaitEnd();

						Runtime.DebugWrite("ДОКАЧИВАЕМ " + noDownloaded.Count);
						foreach (SetValues<ModrinthProjectFile, AddonInstallingInfo> addonData in noDownloaded)
						{
							if (cancelToken.IsCancellationRequested) break;

							var taskArgs = new TaskArgs
							{
								PercentHandler = delegate (int percent)
								{
									_fileDownloadHandler?.Invoke(addonData.Value2.FileName, percent, DownloadFileProgress.PercentagesChanged);
								},
								CancelToken = cancelToken
							};

							int count = 0;

							ModrinthProjectFile projectFile = addonData.Value1;
							SetValues<InstalledAddonInfo, DownloadAddonRes> result = _modrinthApi.DownloadAddon(projectFile, addonData.Value2.Type, "/instances/" + instanceId + "/", addonData.Value2.FileName, withDirectory, taskArgs);

							while (count < 4 && result.Value2 != DownloadAddonRes.Successful && !cancelToken.IsCancellationRequested)
							{
								Thread.Sleep(1000);
								Runtime.DebugWrite("REPEAT DOWNLOAD " + addonData.Value2.FileName);
								result = _modrinthApi.DownloadAddon(projectFile, addonData.Value2.Type, "/instances/" + instanceId + "/", addonData.Value2.FileName, withDirectory, taskArgs);

								count++;
							}

							if (result.Value2 != DownloadAddonRes.Successful)
							{
								Runtime.DebugWrite("ХУЙНЯ, НЕ СКАЧАЛОСЬ " + addonData.Value2.FileName + " " + result.Value2);
								errors.Add("File: " + addonData.Value2.FileName);

								_fileDownloadHandler?.Invoke(addonData.Value2.FileName, 100, DownloadFileProgress.Error);
							}
							else
							{
								compliteDownload.InstalledAddons[projectFile.ProjectId] = result.Value1;
								SaveInstanceContent(compliteDownload);

								_fileDownloadHandler?.Invoke(addonData.Value2.FileName, 100, DownloadFileProgress.Successful);
							}

							downloadedCount++;
							AddonsDownloadEventInvoke(totalFilesCount, downloadedCount);
						}
					}
				}

				var existsFiles = new HashSet<string>(compliteDownload.Files);
				foreach (InstanceManifest.FileData fileData in unknownProjects)
				{
					if (cancelToken.IsCancellationRequested) break;

					string fileName = Path.GetFileName(fileData.Path);
					string folderName = "/instances/" + instanceId + "/" + Path.GetDirectoryName(fileData.Path) + "/";

					var taskArgs = new TaskArgs
					{
						PercentHandler = delegate (int percent)
						{
							_fileDownloadHandler?.Invoke(fileName, percent, DownloadFileProgress.PercentagesChanged);
						},
						CancelToken = cancelToken
					};

					if (withDirectory.InstallFile(fileData.downloads[0], fileName, folderName, taskArgs))
					{
						string filePath = "/" + fileData.Path;
						if (!existsFiles.Contains(filePath))
						{
							compliteDownload.Files.Add(filePath);
							SaveInstanceContent(compliteDownload);
						}
					}
					else
					{
						errors.Add("File: " + fileName);
						Runtime.DebugWrite("ERROR " + fileName);
						_fileDownloadHandler?.Invoke(fileName, 100, DownloadFileProgress.Error);
					}

					downloadedCount++;
					AddonsDownloadEventInvoke(totalFilesCount, downloadedCount);
					_fileDownloadHandler?.Invoke(fileName, 100, DownloadFileProgress.Successful);
				}

				if (errors.Count == 0 && !cancelToken.IsCancellationRequested)
				{
					compliteDownload.FullClient = true;
				}

				SaveInstanceContent(compliteDownload);
				Runtime.DebugWrite("END INSTALL INSTANCE");

				return errors;
			}
			catch
			{
				errors.Add("unknownError");
				return null;
			}
		}
	}
}
