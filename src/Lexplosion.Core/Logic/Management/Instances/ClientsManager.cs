﻿using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Import.Importers;
using Lexplosion.Logic.Management.Sources;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Lexplosion.Logic.Management.Instances
{
	public class ClientsManager
	{
		public event Action<InstancesGroup> GroupAdded;
		public event Action<InstancesGroup> GroupDeleted;

		private ConcurrentDictionary<string, InstanceClient> _installedInstances = new();
		private List<InstancesGroup> _existsGroups = new(); //первая группа всегда all

		/// <summary>
		/// Содержит пары состоящие из внешнего и внутреннего id.
		/// </summary>
		private Dictionary<string, string> _idsPairs = new();
		private readonly AppServiceContainer _services;

		public int LibrarySize { get => _installedInstances.Count; }

		internal ClientsManager(AppServiceContainer services)
		{
			_services = services;
		}

		/// <summary>
		/// Этот метод создаёт локальную сборку. Должен использоваться только при создании локальной сборки.
		/// </summary>
		/// <param name="name">Название сборки</param>
		/// <param name="gameVersion">Версия игры</param>
		/// <param name="modloader">Тип модлоадера</param>
		/// <param name="logoPath">Путь до логотипа. Если устанавливать не надо, то null.</param>
		/// <param name="modloaderVersion">Версия модлоадера. Это поле необходимо только если есть модлоадер</param>
		/// <param name="optifineVersion">Версия оптифайна. Если оптифайн не нужен - то null.</param>
		/// <param name="sodium">Устанавливать ли sodium</param>
		public InstanceClient CreateClient(string name, InstanceSource type, MinecraftVersion gameVersion, ClientType modloader, bool isNwClient, string logoPath = null, string modloaderVersion = null, string optifineVersion = null, bool sodium = false)
		{
			return CreateClient(CreateSourceFactory(type), name, type, gameVersion, modloader, isNwClient, logoPath: logoPath, modloaderVersion: modloaderVersion, optifineVersion: optifineVersion, sodium: sodium);
		}

		private InstanceClient CreateClient(IInstanceSource source, string name, InstanceSource type, MinecraftVersion gameVersion, ClientType modloader, bool isNwClient, string logoPath = null, string modloaderVersion = null, string optifineVersion = null, bool sodium = false, string externalId = null, string profileVersion = null)
		{
			var localId = GenerateInstanceId(name);
			var client = new InstanceClient(name, source, _services, SaveInstalledInstancesList, gameVersion, externalId, localId);

			client.DeployLocally(modloader, isNwClient, logoPath, modloaderVersion, optifineVersion, sodium, profileVersion);

			AddToDefaultGroup(client);
			_installedInstances[client.LocalId] = client;
			SaveInstalledInstancesList();

			return client;
		}

		public InstanceClient CreateClient(MinecraftServerInstance server, bool autoLogin)
		{
			string name = server.Name;
			var minecraftVersion = new MinecraftVersion(server.GameVersion);

			IInstanceSource source;
			string externalId = null;
			string modpackVersion = null;
			if (server.InstanceSource == InstanceSource.FreeSource)
			{
				source = new FreeSource(server.ModpackInfo.SourceId, null, _services);
			}
			else
			{
				source = CreateSourceFactory(server.InstanceSource);
			}

			externalId = server.ModpackInfo?.ModpackId;
			modpackVersion = server.ModpackInfo?.Version;

			bool isNwClient = GlobalData.GeneralSettings.NwClientByDefault == true;

			var client = CreateClient(source, name, server.InstanceSource, minecraftVersion, ClientType.Vanilla, isNwClient, externalId: externalId, profileVersion: modpackVersion);
			client.UpdateInfo(server, autoLogin);

			return client;
		}

		public InstancesGroup CreateGroup(string name, string summary = "")
		{
			var group = new InstancesGroup(name, summary, _services);
			_existsGroups.Add(group);
			group.SaveGroupInfo();

			GroupAdded?.Invoke(group);
			return group;
		}

		public void DeleteGroup(InstancesGroup instancesGroup)
		{
			_existsGroups.Remove(instancesGroup);
			SaveAllGroups();
			GroupDeleted?.Invoke(instancesGroup);
		}

		/// <summary>
		/// Сохраняем список установленных сборок (библиотеку) в файл instanesList.json.
		/// </summary>
		public void SaveInstalledInstancesList()
		{
			// деаем список всех установленных сборок
			var list = new InstalledInstancesFormat();
			foreach (var inst in _installedInstances.Keys)
			{
				list[inst] = new InstalledInstance
				{
					Name = _installedInstances[inst].Name,
					Type = _installedInstances[inst].Type,
					IsInstalled = _installedInstances[inst].IsInstalled,
				};
			}

			// сохраняем этот список
			_services.DataFilesService.SaveFile(_services.DirectoryService.DirectoryPath + "/instanesList.json", JsonConvert.SerializeObject(list));
		}

		/// <summary>
		/// Заполняет список установленных сборок. Вызывается 1 раз, в Main при запуске лаунчера
		/// </summary>
		internal void DefineInstalledInstances()
		{
			var list = _services.DataFilesService.GetFile<InstalledInstancesFormat>(_services.DirectoryService.DirectoryPath + "/instanesList.json");

			if (list != null)
			{
				foreach (string localId in list.Keys)
				{
					VersionManifest instanceManifest = _services.DataFilesService.GetManifest(localId, false);
					bool manifestIsCorrect =
						(instanceManifest != null && instanceManifest.version != null && instanceManifest.version.GameVersion != null);

					//проверяем имеется ли манифест, не содержит ли его id запрещенных символов
					if (manifestIsCorrect && !ForbiddenIsCharsExists(localId))
					{
						IInstanceSource sourceFactory = CreateSourceFactory(list[localId].Type);
						if (sourceFactory == null)
						{
							continue;
						}

						string externalID = null;
						string instanceVersion = null;
						byte[] logo = null;

						//получаем вншний айдшник и версию, если этот модпак не локлаьный
						if (list[localId].Type != InstanceSource.Local)
						{
							InstancePlatformData data = _services.DataFilesService.GetPlatfromData(localId);
							if (data?.instanceVersion != null && data.id != null)
							{
								externalID = data.id;
								instanceVersion = data.instanceVersion;
								_idsPairs[externalID] = localId;
							}
						}

						var instance = new InstanceClient(sourceFactory, _services, SaveInstalledInstancesList, externalID, localId);
						instance.UpdateInfo(list[localId].Name, instanceManifest.version?.GameVersionInfo, logo, instanceVersion, list[localId].IsInstalled);

						_installedInstances[localId] = instance;
					}
				}
			}
		}

		internal void DefineExistsGroups()
		{
			var groups = _services.DataFilesService.GetGroups();

			using (var enumerator = groups.GetEnumerator())
			{
				// если нет групп, то создаем группу all и выходим
				if (!enumerator.MoveNext())
				{
					_existsGroups.Add(new InstancesGroup(_installedInstances.Values, _services));
					return;
				}

				var groupInfo = enumerator.Current;
				if (groupInfo.Id != InstancesGroup.AllInctancesGroupId)
				{
					// если первая группа не all, то сначала добавляем группу all, а потом текущую группу
					_existsGroups.Add(new InstancesGroup(_installedInstances.Values, _services));
					_existsGroups.Add(new InstancesGroup(groupInfo, _installedInstances, _services));
				}
				else
				{
					//если первая группа all, то добавляем в нее сборки, которых не хватает, и закидываем группу в список
					var group = new InstancesGroup(groupInfo, _installedInstances, _services);
					group.AddIfNotExists(_installedInstances.Values);
					_existsGroups.Add(group);
				}

				// добавляем все остальные группы
				while (enumerator.MoveNext())
				{
					groupInfo = enumerator.Current;
					if (groupInfo.Id == InstancesGroup.AllInctancesGroupId) continue; // группу all мы уже добавили в начале
					_existsGroups.Add(new InstancesGroup(groupInfo, _installedInstances, _services));
				}
			}
		}

		/// <summary>
		/// Возвращает список модпаков для библиотеки.
		/// </summary>
		/// <returns>Список установленных модпаков.</returns>
		public List<InstanceClient> GetInstalledInstances()
		{
			return new List<InstanceClient>(_installedInstances.Values);
		}

		public IReadOnlyCollection<InstancesGroup> GetExistsGroups()
		{
			return _existsGroups;
		}

		/// <summary>
		/// Возвращает список модпаков для каталога.
		/// </summary>
		/// <returns>Список внешних модпаков.</returns>
		public CatalogResult<InstanceClient> GetOutsideInstances(InstanceSource type, ISearchParams searchParams)
		{
			Runtime.DebugWrite("UploadInstances " + searchParams.PageIndex);

			IInstanceSource source = CreateSourceFactory(type);

			var instances = new List<InstanceClient>();
			CatalogResult<InstanceInfo> catalog = source.GetCatalog(type, searchParams);

			foreach (var instance in catalog)
			{
				if (string.IsNullOrWhiteSpace(instance.ExternalId)) continue;

				InstanceClient instanceClient;
				if (_idsPairs.ContainsKey(instance.ExternalId))
				{
					instanceClient = _installedInstances[_idsPairs[instance.ExternalId]];
					instanceClient.CheckUpdates();
					instanceClient.UpdateInfo(instance.Name, null, instance.Categories, instance.Summary, instance.Description, instance.Author, instance.WebsiteUrl);
					instanceClient.DownloadLogo(instance.LogoUrl, instanceClient.SaveAssets);
				}
				else
				{
					instanceClient = new InstanceClient(instance.Name, source, _services, SaveInstalledInstancesList, instance.GameVersion, instance.ExternalId, null);
					instanceClient.UpdateInfo(instance.Name, null, instance.Categories, instance.Summary, instance.Description, instance.Author, instance.WebsiteUrl);
					instanceClient.DownloadLogo(instance.LogoUrl, () => { });
				}

				instances.Add(instanceClient);

				instanceClient.DownloadCounts = instance.DownloadCounts;
			}

			return new(instances, catalog.TotalCount);
		}

		/// <summary>
		/// Получает внешнюю сборку по id
		/// </summary>
		/// <returns>Экземпляр клиента.</returns>
		public InstanceClient GetInstance(InstanceSource type, string instanceId)
		{
			PrototypeInstance.Info instance = PrototypeInstance.GetInstance(type, _services, instanceId);

			InstanceClient instanceClient;
			if (instance != null)
			{
				// TODO: тут пока нет необходимости получать лого, но потом она может появиться
				if (_idsPairs.ContainsKey(instance.ExternalId))
				{
					instanceClient = _installedInstances[_idsPairs[instance.ExternalId]];
					instanceClient.CheckUpdates();
				}
				else
				{
					instanceClient = new InstanceClient(instance.Name, CreateSourceFactory(type), _services, SaveInstalledInstancesList, new MinecraftVersion(instance.GameVersion), instance.ExternalId, null);
				}

				instanceClient.UpdateInfo(instance.Name, null, instance.Categories, instance.Summary, instance.Description, instance.Author, instance.WebsiteUrl);

				return instanceClient;
			}

			return null;
		}

		private void AddToDefaultGroup(InstanceClient client)
		{
			_existsGroups[0].AddInstance(client);
		}

		public void SaveAllGroups()
		{
			_services.DataFilesService.RewriteGroupsInfo(_existsGroups.Select(x => x.BuildInstalledInstanceGroup()));
		}

		/// <summary>
		/// Добавляет сборку в библиотеку
		/// </summary>
		public void AddToLibrary(InstanceClient client)
		{
			if (!client.CreatedLocally)
			{
				string localId = GenerateInstanceId(client.Name);
				client.CreateLocalStruct(localId);

				_installedInstances[localId] = client;
				_idsPairs[client.ExternalId] = localId;

				AddToDefaultGroup(client);
				SaveInstalledInstancesList();
			}
		}

		/// <summary>
		/// Удаляет сборку к хуям.
		/// </summary>
		public void DeleteFromLibrary(InstanceClient client)
		{
			if (client.LocalId == null) return;

			_installedInstances.TryRemove(client.LocalId, out _);
			if (client.ExternalId != null)
			{
				_idsPairs.Remove(client.ExternalId);
			}

			foreach (var group in _existsGroups)
			{
				group.RemoveInstance(client);
			}

			client.DeleteLocalStruct();
			SaveAllGroups();
			SaveInstalledInstancesList();
		}

		private string GenerateInstanceId(string clientName)
		{
			string instanceId = clientName.ToLower();

			// переводим русские символы в транслит
			string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
			string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
			string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
			for (int i = 0; i <= 32; i++)
			{
				instanceId = instanceId.Replace(rus_up[i], lat_low[i]).Replace(rus_low[i], lat_low[i]);
			}

			instanceId = instanceId.Replace("&", "AND");

			if (ForbiddenIsCharsExists(instanceId))
			{
				int j = 0;
				while (j < instanceId.Length)
				{
					if (ForbiddenIsCharsExists(instanceId[j].ToString()))
					{
						instanceId = instanceId.Replace(instanceId[j], '_');
					}
					j++;
				}

				if (_installedInstances.ContainsKey(instanceId) || DirectoryIsExists(_services.DirectoryService.GetInstancePath(instanceId)))
				{
					string instanceId_ = instanceId;
					int i = 0;
					do
					{
						if (i > 0)
						{
							instanceId_ = instanceId + " (" + i + ")";
						}
						i++;
					}
					while (_installedInstances.ContainsKey(instanceId_) || DirectoryIsExists(_services.DirectoryService.GetInstancePath(instanceId_)));
					instanceId = instanceId_;
				}
			}
			else if (_installedInstances.ContainsKey(instanceId) || DirectoryIsExists(_services.DirectoryService.GetInstancePath(instanceId)))
			{
				string instanceId_ = instanceId;
				int i = 0;
				do
				{
					instanceId_ = instanceId + " (" + i + ")";
					i++;
				}
				while (_installedInstances.ContainsKey(instanceId_) || DirectoryIsExists(_services.DirectoryService.GetInstancePath(instanceId_)));

				instanceId = instanceId_;
			}

			return instanceId;
		}

		private void Import(in InstanceClient client, string fileAddr, bool fileAddrIsLocal, ImportData importData)
		{
			client.State = StateType.DownloadPrepare;

			var executor = new ImportExecutor(fileAddr, fileAddrIsLocal, GlobalData.GeneralSettings, _services, client.DownloadStateHandler, importData);
			InstanceInit res = executor.Prepeare(out PrepeareResult result);

			if (res != InstanceInit.Successful)
			{
				importData.ResultHandler(new ClientInitResult(res));
				client.State = StateType.Default;

				return;
			}

			string localId = GenerateInstanceId(result.Name);

			client.UpdateInfo(result.Name, result.GameVersionInfo, categories: null, result.Summary, result.Description, result.Author, null);
			client.SetLogo(result.LogoPath);
			client.CreateLocalStruct(localId);

			InstanceInit importRes = executor.Import(localId, out IReadOnlyCollection<string> errors);

			if (importRes != InstanceInit.Successful)
			{
				client.CompleteInitialization(importRes);
				client.DeleteLocalStruct();

				importData.ResultHandler(new ClientInitResult(res, errors));
				client.State = StateType.Default;

				return;
			}

			_installedInstances[localId] = client;
			SaveInstalledInstancesList();

			client.CompleteInitialization(InstanceInit.Successful);
			client.State = StateType.Default;
			importData.ResultHandler(new ClientInitResult(InstanceInit.Successful, errors));
		}

		public InstanceClient Import(string zipFile, ImportData importData)
		{
			var client = new InstanceClient(CreateSourceFactory(InstanceSource.Local), _services, SaveInstalledInstancesList);
			client.MakeFictitiousClient("Importing...");

			new Thread(() =>
			{
				client.State = StateType.DownloadPrepare;
				Import(client, zipFile, true, importData);
			}).Start();

			return client;
		}

		public InstanceClient Import(FileReceiver reciver, ImportData importData)
		{
			var client = new InstanceClient(CreateSourceFactory(InstanceSource.Local), _services, SaveInstalledInstancesList);
			client.MakeFictitiousClient("Importing...");

			new Thread(() =>
			{
				client.State = StateType.DownloadPrepare;

				reciver.DownloadCanceled += () => { client.State = StateType.DownloadInCancellation; };
				reciver.StateChanged += () =>
				{
					var state = reciver.State;
					StateType resState = StateType.InQueue;
					switch (state)
					{
						case FileReceiver.DistributionState.InQueue:
							resState = StateType.InQueue;
							break;
						case FileReceiver.DistributionState.InConnect:
							resState = StateType.InConnect;
							break;
						case FileReceiver.DistributionState.InProcess:
							resState = StateType.DownloadClient;
							break;
					}

					client.State = resState;
				};

				FileRecvResult result = _services.DirectoryService.ReceiveFile(reciver, out string file);
				if (result == FileRecvResult.Successful)
				{
					client.State = StateType.PostProcessing;
					Import(in client, file, true, importData);
				}
				else
				{
					importData.ResultHandler(new ClientInitResult(result == FileRecvResult.Canceled ? InstanceInit.IsCancelled : InstanceInit.DownloadFilesError, new List<string>() { "modpack file" }));
				}
			}).Start();

			return client;
		}

		public InstanceClient Import(Uri fileURL, ImportData importData)
		{
			var client = new InstanceClient(CreateSourceFactory(InstanceSource.Local), _services, SaveInstalledInstancesList);

			client.MakeFictitiousClient("Importing...");

			new Thread(() =>
			{
				client.State = StateType.DownloadPrepare;
				string downloadUrl = null;
				try
				{
					if (fileURL.Host == "drive.google.com")
					{
						string[] parts = fileURL.PathAndQuery.Split('/');
						if (parts.Length < 4 || string.IsNullOrWhiteSpace(parts[3]))
						{
							importData.ResultHandler(new ClientInitResult(InstanceInit.WrongClientFileUrl));
							return;
						}

						downloadUrl = "https://drive.google.com/uc?export=download&confirm=no_antivirus&id=" + parts[3];

						if (_services.WebService.IsHtmlPage(downloadUrl))
						{
							string data = _services.WebService.HttpGet(downloadUrl);
							if (string.IsNullOrWhiteSpace(data))
							{
								importData.ResultHandler(new ClientInitResult(InstanceInit.WrongClientFileUrl));
								return;
							}

							IEnumerable<string> GetSubStrings(string input, string start, string end)
							{
								Regex r = new Regex(Regex.Escape(start) + "(.*?)" + Regex.Escape(end));
								MatchCollection matches = r.Matches(input);
								foreach (Match match in matches)
									yield return match.Groups[1].Value;
							}

							string GetStrBetweenStrings(string input, string start, string end)
							{
								return Regex.Match(input, Regex.Escape(start) + "(.*?)" + Regex.Escape(end)).Groups[1].Value;
							}

							string urlBase = null;
							string formHead = null;
							foreach (string pageForm in GetSubStrings(data, "<form", ">"))
							{
								if (pageForm.Contains("id=\"download-form\"") && pageForm.Contains("action=\""))
								{
									urlBase = GetStrBetweenStrings(pageForm, "action=\"", "\"");
									formHead = "<form" + pageForm + ">";
									break;
								}
							}

							if (urlBase == null)
							{
								importData.ResultHandler(new ClientInitResult(InstanceInit.WrongClientFileUrl));
								return;
							}

							urlBase += "?";
							data = GetStrBetweenStrings(data, formHead, "</form>");
							foreach (string htmlInput in GetSubStrings(data, "<input", ">"))
							{
								if (htmlInput.Contains("name=\"") && htmlInput.Contains("value=\""))
								{
									string name = GetStrBetweenStrings(htmlInput, "name=\"", "\"");
									string value = GetStrBetweenStrings(htmlInput, "value=\"", "\"");
									urlBase += Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value) + "&";
								}
							}

							downloadUrl = urlBase;
						}
					}
					else if (fileURL.Host == "yadi.sk" || fileURL.Host == "disk.yandex.ru" || fileURL.Host == "disk.yandex.com" || fileURL.Host == "disk.yandex.by")
					{
						string queryUrl = "https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=" + Uri.EscapeDataString(fileURL.ToString());

						string result = _services.WebService.HttpGet(queryUrl);
						if (result == null)
						{
							importData.ResultHandler(new ClientInitResult(InstanceInit.WrongClientFileUrl));
							return;
						}

						var data = JsonConvert.DeserializeObject<JToken>(result);
						downloadUrl = data["href"].ToString();
					}
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite("Exception " + ex);
					importData.ResultHandler(new ClientInitResult(InstanceInit.WrongClientFileUrl));
					return;
				}

				Import(client, downloadUrl, false, importData);
			}).Start();

			return client;
		}

		private InstanceClient CopyClient(InstanceClient client, BaseInstanceData newClientData, ImportData importData)
		{
			string name = client.Name + " (Copy)";
			string id = GenerateInstanceId(name);

			IInstanceSource source = CreateSourceFactory(client.Type);
			var newClient = new InstanceClient(name, source, _services, SaveInstalledInstancesList, client.GameVersion, client.ExternalId, id);

			BaseInstanceData baseData = newClientData ?? client.GetBaseData;
			newClient.UpdateInfo(null, null, client.Categories, client.Summary, client.Description, client.Author, client.WebsiteUrl);
			newClient.SetLogo(client.Logo);
			newClient.IsInstalled = client.IsInstalled;
			newClient.DeployLocally(baseData.Modloader, baseData.IsNwClient, null, baseData.ModloaderVersion, baseData.OptifineVersion, false);

			_installedInstances[id] = newClient;
			SaveInstalledInstancesList();

			new Thread(() =>
			{
				var result = InstanceInit.Successful;
				try
				{
					newClient.State = StateType.DownloadPrepare;

					WithDirectory directoryService = _services.DirectoryService;
					string from = directoryService.GetInstancePath(client.LocalId);
					string to = directoryService.GetInstancePath(newClient.LocalId);

					foreach (string dirPath in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
					{
						if (importData.CancelToken.IsCancellationRequested)
						{
							result = InstanceInit.IsCancelled;
							newClient.State = StateType.DownloadInCancellation;
							DeleteFromLibrary(newClient);
							return;
						}

						try
						{
							Directory.CreateDirectory(dirPath.Replace(from, to));
						}
						catch (Exception ex)
						{
							Runtime.DebugWrite("Exception " + ex);
						}
					}

					foreach (string sourcePath in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
					{
						if (importData.CancelToken.IsCancellationRequested)
						{
							result = InstanceInit.IsCancelled;
							newClient.State = StateType.DownloadInCancellation;
							DeleteFromLibrary(newClient);
							return;
						}

						string fileName = Path.GetFileName(sourcePath);
						if (fileName == DataFilesManager.MANIFEST_FILE || fileName == DataFilesManager.MANIFEST_FILE_OLD) continue;

						try
						{
							File.Copy(sourcePath, sourcePath.Replace(from, to), true);
						}
						catch (Exception ex)
						{
							Runtime.DebugWrite("Exception " + ex);
						}
					}
				}
				catch (Exception ex)
				{
					Runtime.DebugWrite("Exception " + ex);
				}
				finally
				{
					newClient.State = StateType.Default;
					importData.ResultHandler(new ClientInitResult(result));
				}
			}).Start();

			return newClient;
		}

		public InstanceClient CopyClient(InstanceClient client, ImportData importData)
		{
			return CopyClient(client, null, importData);
		}

		public InstanceClient CopyClient(InstanceClient client, MinecraftVersion gameVersion, ClientType clientType, string modloaderVersion, Action<List<InstanceAddon>> getUncopiedAddons, ImportData importData)
		{
			BaseInstanceData baseData = client.GetBaseData;

			if (baseData.Modloader == clientType && baseData.GameVersion == gameVersion)
			{
				baseData.ModloaderVersion = modloaderVersion;
				return CopyClient(client, baseData, importData);
			}

			string name = client.Name + " (Copy)";
			string id = GenerateInstanceId(name);

			IInstanceSource source = CreateSourceFactory(InstanceSource.Local);
			var newClient = new InstanceClient(name, source, _services, SaveInstalledInstancesList, gameVersion, null, id);

			newClient.UpdateInfo(null, null, client.Categories, client.Summary, client.Description, client.Author, client.WebsiteUrl);
			newClient.SetLogo(client.Logo);
			newClient.IsInstalled = client.IsInstalled;
			newClient.DeployLocally(clientType, baseData.IsNwClient, null, modloaderVersion, baseData.OptifineVersion, false);

			_installedInstances[id] = newClient;
			SaveInstalledInstancesList();

			new Thread(() =>
			{
				var initRes = InstanceInit.Successful;
				try
				{
					newClient.State = StateType.DownloadPrepare;

					WithDirectory directoryService = _services.DirectoryService;

					string from = directoryService.GetInstancePath(client.LocalId);
					string to = directoryService.GetInstancePath(newClient.LocalId);

					var directoriesBlackList = new string[]
					{
						$"{from}{AddonsUtils.GetFolderName(AddonType.Mods)}",
						$"{from}{AddonsUtils.GetFolderName(AddonType.Resourcepacks)}",
						$"{from}{AddonsUtils.GetFolderName(AddonType.Shaders)}"
					};

					foreach (string dirPath in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
					{
						if (importData.CancelToken.IsCancellationRequested)
						{
							initRes = InstanceInit.IsCancelled;
							newClient.State = StateType.DownloadInCancellation;
							DeleteFromLibrary(newClient);
							return;
						}

						try
						{
							Directory.CreateDirectory(dirPath.Replace(from, to));
						}
						catch (Exception ex)
						{
							Runtime.DebugWrite("Exception " + ex);
						}
					}

					foreach (string sourcePath in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
					{
						if (importData.CancelToken.IsCancellationRequested)
						{
							initRes = InstanceInit.IsCancelled;
							newClient.State = StateType.DownloadInCancellation;
							DeleteFromLibrary(newClient);
							return;
						}

						string _sourcePath = sourcePath.Replace("\\", "/");
						foreach (string dir in directoriesBlackList)
						{
							if (_sourcePath.StartsWith(dir)) goto ToNextIteration;
						}

						string fileName = Path.GetFileName(sourcePath);
						if (fileName == DataFilesManager.MANIFEST_FILE || fileName == DataFilesManager.MANIFEST_FILE_OLD
						|| fileName == DataFilesManager.INSTANCE_PLATFORM_DATA_FILE || fileName == DataFilesManager.INSTANCE_PLATFORM_DATA_FILE_OLD)
							continue;

						try
						{
							File.Copy(sourcePath, sourcePath.Replace(from, to), true);
						}
						catch (Exception ex)
						{
							Runtime.DebugWrite("Exception " + ex);
						}

					ToNextIteration:;
					}

					var addonsManager = AddonsManager.GetManager(baseData, _services);

					var addonsToDownload = new List<InstanceAddon>();
					addonsToDownload.AddRange(addonsManager.GetInstalledMods(newClient.GetBaseData));
					addonsToDownload.AddRange(addonsManager.GetInstalledResourcepacks(newClient.GetBaseData));
					addonsToDownload.AddRange(addonsManager.GetInstalledShaders(newClient.GetBaseData));

					int totalAddonsCount = addonsToDownload.Count;

					newClient.DownloadStateHandler(StateType.DownloadClient, new ProgressHandlerArguments()
					{
						Stage = 1,
						StagesCount = 1,
						TotalFilesCount = totalAddonsCount
					});

					var errors = new List<InstanceAddon>();

					int filesCount = 0;
					foreach (var addon in addonsToDownload)
					{
						if (importData.CancelToken.IsCancellationRequested)
						{
							initRes = InstanceInit.IsCancelled;
							newClient.State = StateType.DownloadInCancellation;
							DeleteFromLibrary(newClient);
							return;
						}

						DownloadAddonRes result = addon.Update();
						if (result != DownloadAddonRes.Successful) errors.Add(addon);

						filesCount++;

						newClient.DownloadStateHandler(StateType.DownloadClient, new ProgressHandlerArguments()
						{
							Stage = 1,
							StagesCount = 1,
							TotalFilesCount = totalAddonsCount,
							FilesCount = filesCount,
							Procents = (int)(((double)filesCount / totalAddonsCount) * 100)
						});
					}

					if (errors.Count > 0) getUncopiedAddons(errors);

				}
				catch (Exception ex)
				{
					Runtime.DebugWrite("Exception " + ex);
				}
				finally
				{
					newClient.State = StateType.Default;
					importData.ResultHandler(new ClientInitResult(initRes));
				}

			}).Start();

			return newClient;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool ForbiddenIsCharsExists(string str)
		{
			str = str.Replace("_", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Replace(" ", "").Replace(".", "").Replace("(", "").Replace(")", "");
			return Regex.IsMatch(str, @"[^a-zA-Z0-9]");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IInstanceSource CreateSourceFactory(InstanceSource type)
		{
			switch (type)
			{
				case InstanceSource.Local:
					return new LocalSource(_services);
				case InstanceSource.Nightworld:
					return new NightWorldSource(_services);
				case InstanceSource.Curseforge:
					return new CurseforgeSource(_services);
				case InstanceSource.Modrinth:
					return new ModrinthSource(_services);
				case InstanceSource.FreeSource:
					return new FreeSource();
				default:
					{
						Runtime.DebugWrite("CreateSourceFactory error. type: " + type);
						return new LocalSource(_services);
					}
			}
		}

		private bool DirectoryIsExists(string path)
		{
			try
			{
				return Directory.Exists(path);
			}
			catch
			{
				return false;
			}
		}

	}
}
