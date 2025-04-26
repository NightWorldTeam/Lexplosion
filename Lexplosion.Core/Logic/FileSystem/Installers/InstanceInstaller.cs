using Lexplosion.Global;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Lexplosion.Logic.FileSystem.Installers
{
	class InstanceInstaller
	{
		private static KeySemaphore<string> _librariesBlock = new KeySemaphore<string>();
		private static KeySemaphore<string> _assetsBlock = new KeySemaphore<string>();

		protected readonly WithDirectory withDirectory;
		protected readonly DataFilesManager dataFilesManager;
		protected readonly ToServer webService;

		protected string instanceId;

		public InstanceInstaller(string instanceID, IFileServicesContainer services)
		{
			instanceId = instanceID;

			withDirectory = services.DirectoryService;
			dataFilesManager = services.DataFilesService;
			webService = services.WebService;
		}

		public struct Assets
		{
			public struct AssetFile
			{
				public string hash;
			}

			public Dictionary<string, AssetFile> objects;
		}

		/// <summary>
		/// Эвент скачивнаия файла. string - имя файла, int - проценты. DownloadFileProgress - стадия
		/// </summary>
		public event Action<string, int, DownloadFileProgress> FileDownloadEvent
		{
			add
			{
				_fileDownloadHandler += value;
			}

			remove
			{
				_fileDownloadHandler -= value;
			}
		}

		protected Action<string, int, DownloadFileProgress> _fileDownloadHandler;

		public event ProcentUpdate BaseDownloadEvent;

		private ConcurrentDictionary<string, LibInfo> _libraries;
		private bool _minecraftJar;
		private bool _assetsIndexes;
		private Assets _assets;

		private int _updatesCount = 0;

		private void ResetUpdatesList()
		{
			_libraries = new ConcurrentDictionary<string, LibInfo>();
			_minecraftJar = false;
			_assetsIndexes = false;
			_assets.objects = null;
			_updatesCount = 0;
		}

		/// <summary>
		/// Получем версию либрариеса
		/// </summary>
		/// <param name="libName">Имя для файла. Должно быть получено через GetLibName</param>
		/// <param name="folderName">Имя папки. Либо additionalLibraries, либо libraries</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private long GetLver(string libName, string folderName)
		{
			if (File.Exists(withDirectory.DirectoryPath + "/versions/" + folderName + "/lastUpdates/" + libName + ".lver"))
			{
				try
				{
					string fileContent = dataFilesManager.GetFile(withDirectory.DirectoryPath + "/versions/" + folderName + "/lastUpdates/" + libName + ".lver"); //открываем файл с версией libraries
					long ver = 0;
					Int64.TryParse(fileContent, out ver);
					return ver;
				}
				catch
				{
					return 0;
				}
			}
			else
			{
				dataFilesManager.SaveFile(withDirectory.DirectoryPath + "/versions/" + folderName + "/lastUpdates/" + libName + ".lver", "0");
				return 0;
			}
		}

		/// <summary>
		/// Проверяет основные файла клиента, недостающие файлы помещает во внуренний список на скачивание
		/// </summary>
		/// <returns>
		/// Возвращает количество файлов, которые нужно обновить. -1 в случае неудачи (возможно только если включена защита целосности клиента). 
		/// </returns>
		public int CheckBaseFiles(in VersionManifest manifest, ref LastUpdates updates) // функция проверяет основные файлы клиента (файл версии, либрариесы и тп)
		{
			try
			{
				ResetUpdatesList();

				string gameVersionName = manifest.version.CustomVersionName ?? manifest.version.GameVersion;

				//проверяем файл версии
				if (!Directory.Exists(withDirectory.InstancesPath + instanceId + "/version"))
				{
					Directory.CreateDirectory(withDirectory.GetInstancePath(instanceId) + "version"); //создаем папку versions если её нет
					_minecraftJar = true; //сразу же добавляем minecraftJar в обновления
					_updatesCount++;
				}
				else
				{
					string minecraftJarFile = withDirectory.GetInstancePath(instanceId) + "version/" + manifest.version.MinecraftJar.name;
					if (updates.ContainsKey("version") && File.Exists(minecraftJarFile) && manifest.version.MinecraftJar.lastUpdate == updates["version"]) //проверяем его наличие и версию
					{
						if (manifest.version.Security) //если включена защита файла версии, то проверяем его 
						{
							try
							{
								using (FileStream fstream = new FileStream(minecraftJarFile, FileMode.Open, FileAccess.Read))
								{
									byte[] bytes = new byte[fstream.Length];
									fstream.Read(bytes, 0, bytes.Length);
									fstream.Close();

									using (SHA1 sha = new SHA1Managed())
									{
										if (Convert.ToBase64String(sha.ComputeHash(bytes)) != manifest.version.MinecraftJar.sha1 || bytes.Length != manifest.version.MinecraftJar.size)
										{
											File.Delete(minecraftJarFile); //удаляем файл, если не сходится хэш или размер
											_minecraftJar = true;
											_updatesCount++;
										}
									}
								}
							}
							catch
							{
								return -1; //чтение файла не удалось, стопаем весь процесс
							}
						}
					}
					else
					{
						_minecraftJar = true;
						_updatesCount++;
					}
				}

				//получаем версию libraries
				string libName = manifest.version.GetLibName;
				updates["libraries"] = GetLver(libName, "libraries");

				//получаем версию дополнительных либраиесов
				if (manifest.version.AdditionalInstaller != null)
				{
					updates["additionalLibraries"] = GetLver(manifest.version.AdditionalInstaller.GetLibName, "additionalLibraries");
				}

				//проверяем папку libraries
				string librariesDir = withDirectory.DirectoryPath + "/libraries/";
				if (!Directory.Exists(librariesDir))
				{
					foreach (string lib in manifest.libraries.Keys)
					{
						_libraries[lib] = manifest.libraries[lib];
						_updatesCount++;
					}
				}
				else
				{
					if (manifest.version.LibrariesLastUpdate != updates["libraries"]) //если версия libraries старая, то отправляем на обновления
					{
						foreach (string lib in manifest.libraries.Keys)
						{
							_libraries[lib] = manifest.libraries[lib];
							_updatesCount++;
						}
					}
					else
					{
						if (manifest.version.AdditionalInstaller != null && manifest.version.AdditionalInstaller.librariesLastUpdate != updates["additionalLibraries"]) //если версия дополнительных libraries старая, то отправляем на обновления
						{
							foreach (string lib in manifest.libraries.Keys)
							{
								if (manifest.libraries[lib].additionalInstallerType != null)
								{
									_libraries[lib] = manifest.libraries[lib];
									_updatesCount++;
								}
							}
						}

						// получем файл, в ктором хранятси список либрариесов, которые удачно скачались в прошлый раз
						List<string> downloadedFiles = new List<string>();
						string downloadedInfoAddr = withDirectory.DirectoryPath + "/versions/libraries/" + libName + "-downloaded.json";
						bool fileExided = false;
						if (File.Exists(downloadedInfoAddr))
						{
							downloadedFiles = dataFilesManager.GetFile<List<string>>(downloadedInfoAddr);
							fileExided = true;
						}

						//ищем недостающие файлы
						foreach (string lib in manifest.libraries.Keys)
						{
							if ((downloadedFiles == null && fileExided) || !File.Exists(withDirectory.DirectoryPath + "/libraries/" + lib) || (fileExided && downloadedFiles != null && !downloadedFiles.Contains(lib)))
							{
								_libraries[lib] = manifest.libraries[lib];
								_updatesCount++;
							}
						}
					}
				}

				if (!Directory.Exists(withDirectory.DirectoryPath + "/natives/" + gameVersionName))
				{
					foreach (string lib in manifest.libraries.Keys)
					{
						if (manifest.libraries[lib].isNative)
						{
							_libraries[lib] = manifest.libraries[lib];
							_updatesCount++;
						}
					}
				}

				// Проверяем assets

				// Пытаемся получить список всех асетсов из json файла
				Assets asstes = dataFilesManager.GetFile<Assets>(withDirectory.DirectoryPath + "/assets/indexes/" + manifest.version.AssetsVersion + ".json");

				// Файла нет, или он битый. Получаем асетсы с сервера
				if (asstes.objects == null)
				{
					_assetsIndexes = true; //устанавливаем флаг что нужно скачать json файл
					_updatesCount++;

					if (!File.Exists(withDirectory.DirectoryPath + "/assets/indexes/" + manifest.version.AssetsVersion + ".json"))
					{
						try
						{
							// Получем асетсы с сервера
							asstes = JsonConvert.DeserializeObject<Assets>(webService.HttpGet(manifest.version.AssetsIndexes));
						}
						catch { }
					}
				}

				if (asstes.objects != null) // проверяем не возникла ли ошибка
				{
					_assets.objects = new Dictionary<string, Assets.AssetFile>();

					foreach (string asset in asstes.objects.Keys)
					{
						string assetHash = asstes.objects[asset].hash;
						if (assetHash != null)
						{
							// проверяем существует ли файл. Если нет - отправляем на обновление
							string assetPath = "/" + assetHash.Substring(0, 2);
							if (!File.Exists(withDirectory.DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
							{
								_assets.objects[asset] = asstes.objects[asset];
								_updatesCount++;
							}
						}
						else
						{
							// С этим файлом возникла ошибка. Добавляем его в список на обновление. Метод обновления законет его в список ошибок
							_assets.objects[asset] = asstes.objects[asset];
							_updatesCount++;
						}
					}
				}
				else
				{
					_assets.objects = null;
				}
			}
			catch
			{
				return 0;
			}

			return _updatesCount;
		}

		protected bool TryDownloadFile(string url, string file, string temp, TaskArgs taskArgs)
		{
			int i = 0;
			while (!withDirectory.DownloadFile(url, file, temp, taskArgs) && i < 3 && !taskArgs.CancelToken.IsCancellationRequested)
			{
				Thread.Sleep(1500);
				i++;
			}

			//успех если попыток было меньше трёх и скачивнаие не было отменено
			return i <= 2 && !taskArgs.CancelToken.IsCancellationRequested;
		}

		protected bool TryDownloadWithMirror(string url, string file, string temp, TaskArgs taskArgs)
		{
			if (!TryDownloadFile(url, file, temp, taskArgs))
			{
				if (!url.StartsWith("https://") || taskArgs.CancelToken.IsCancellationRequested) return false;

				url = url.Replace("https://", "");
				url = LaunсherSettings.URL.MirrorUrl + url;

				Runtime.DebugWrite("Try mirror, url " + url);
				return withDirectory.DownloadFile(url, file, temp, taskArgs);
			}

			return true;
		}

		/// <summary>
		/// Ффункция для скачивания файлов клиента в zip формате, без проверки хеша
		/// </summary>
		/// <param name="url">Ссылка на файл, охуеть, да? C .zip в конце.</param>
		/// <param name="file">Имя файла</param>
		/// <param name="to">Путь куда скачать (без имени файла), должен заканчиваться на слеш.</param>
		/// <param name="temp">Временная директория (без имени файла), должена заканчиваться на слеш.</param>
		/// <param name="taskArgs">Аргументы задачи</param> 
		/// <returns></returns>
		protected bool UnsafeDownloadZip(string url, string to, string file, string temp, TaskArgs taskArgs)
		{
			string zipFile = file + ".zip";

			try
			{
				if (!Directory.Exists(to))
				{
					Directory.CreateDirectory(to);
				}
				withDirectory.DelFile(temp + zipFile);

				//пробуем скачать 4 раза
				int i = 0;
				while (!withDirectory.DownloadFile(url, zipFile, temp, taskArgs) && i < 4 && !taskArgs.CancelToken.IsCancellationRequested)
				{
					Thread.Sleep(1500);
					i++;
				}

				// все попытки неувенчались успехом
				if (i > 3 || taskArgs.CancelToken.IsCancellationRequested)
				{
					return false;
				}

				ZipFile.ExtractToDirectory(temp + zipFile, temp);
				File.Delete(temp + zipFile);

				withDirectory.DelFile(to + file);
				File.Move(temp + file, to + file);

				return true;
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				withDirectory.DelFile(temp + file);
				withDirectory.DelFile(temp + zipFile);

				return false;
			}

		}

		/// <summary>
		/// Функция для скачивания файлов клиента в zip формате, со сравнением хеша
		/// </summary>
		/// <param name="url">Ссыллка на файл, охуеть, да? Без .zip в конце.</param>
		/// <param name="file">Имя файла</param>
		/// <param name="to">Путь куда скачать (без имени файла), должен заканчиваться на слеш.</param>
		/// <param name="temp">Временная директория (без имени файла), должна заканчиваться на слеш.</param>
		/// <param name="sha1">Хэш</param>
		/// <param name="size">Размер</param>
		/// <param name="taskArgs">аргументы задачи</param>
		/// <returns></returns>
		protected bool SaveDownloadZip(string url, string file, string to, string temp, string sha1, long size, TaskArgs taskArgs)
		{
			string zipFile = file + ".zip";

			try
			{
				if (!Directory.Exists(to))
				{
					Directory.CreateDirectory(to);
				}

				//пробуем скачать 4 раза
				int i = 0;
				while (!withDirectory.DownloadFile(url + ".zip", zipFile, temp, taskArgs) && i < 4 && !taskArgs.CancelToken.IsCancellationRequested)
				{
					Thread.Sleep(1500);
					i++;
				}

				// все попытки неувенчались успехом
				if (i > 3 || taskArgs.CancelToken.IsCancellationRequested)
				{
					return false;
				}

				withDirectory.DelFile(to + file);

				ZipFile.ExtractToDirectory(temp + zipFile, temp);
				File.Delete(temp + zipFile);

				using (FileStream fstream = new FileStream(temp + file, FileMode.Open, FileAccess.Read))
				{
					byte[] fileBytes = new byte[fstream.Length];
					fstream.Read(fileBytes, 0, fileBytes.Length);
					fstream.Close();

					using (SHA1 sha = new SHA1Managed())
					{
						if (Convert.ToBase64String(sha.ComputeHash(fileBytes)) == sha1 && fileBytes.Length == size)
						{
							withDirectory.DelFile(to + file);
							File.Move(temp + file, to + file);

							return true;
						}
						else
						{
							File.Delete(temp + file);
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				withDirectory.DelFile(temp + file);
				withDirectory.DelFile(temp + zipFile);

				return false;
			}
		}


		/// <summary>
		/// Aункция для скачивания файлов в jar формате, без сравнения хэша
		/// </summary>
		/// <param name="url">Url файла</param>
		/// <param name="to">Директория куда скачать (без имени файла). Должно заканчиваться слешем.</param>
		/// <param name="file">Имя файла</param>
		/// <param name="temp">Временная директория (тоже без имени файла). Должно заканчиваться слешем.</param>
		/// <param name="taskArgs">аргументы задачи</param>
		/// <returns>Охуенно или пиздец</returns>
		protected bool UnsafeDownloadJar(string url, string to, string file, string temp, TaskArgs taskArgs, bool mirrorIfError = false)
		{
			try
			{
				if (!Directory.Exists(to))
				{
					Directory.CreateDirectory(to);
				}

				if (mirrorIfError && !TryDownloadWithMirror(url, file, temp, taskArgs))
				{
					return false;
				}
				else if (!mirrorIfError && !TryDownloadFile(url, file, temp, taskArgs))
				{
					return false;
				}


				withDirectory.DelFile(to + file);
				File.Move(temp + file, to + file);

				return true;
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				withDirectory.DelFile(temp + file);
				return false;
			}
		}

		/// <summary>
		/// Aункция для скачивания файлов в jar формате, со сравнением хэша
		/// </summary>
		/// <param name="url">URL файла</param>
		/// <param name="file">Имя файла</param>
		/// <param name="to">Директория куда скачать (без имени файла). Должно заканчиваться слешем</param>
		/// <param name="temp">Временная директория (без имени файла). Должно заканчиваться слешем</param>
		/// <param name="sha1">Хэш файла</param>
		/// <param name="size">Размер файла</param>
		/// <param name="taskArgs">аргументы задачи</param>
		/// <returns></returns>
		protected bool SaveDownloadJar(string url, string file, string to, string temp, string sha1, long size, TaskArgs taskArgs, bool mirrorIfError = false)
		{
			try
			{
				if (!Directory.Exists(to))
				{
					Directory.CreateDirectory(to);
				}

				if (mirrorIfError && !TryDownloadWithMirror(url, file, temp, taskArgs))
				{
					return false;
				}
				else if (!mirrorIfError && !TryDownloadFile(url, file, temp, taskArgs))
				{
					return false;
				}

				withDirectory.DelFile(to + file);

				using (FileStream fstream = new FileStream(temp + file, FileMode.Open, FileAccess.Read))
				{
					using (SHA1CryptoServiceProvider mySha = new SHA1CryptoServiceProvider())
					{
						StringBuilder Sb = new StringBuilder();
						byte[] result = mySha.ComputeHash(fstream);

						foreach (byte b in result)
							Sb.Append(b.ToString("x2"));

						bool hashIsValid = Sb.ToString() == sha1 || Convert.ToBase64String(result) == sha1;
						if (hashIsValid && fstream.Length == size)
						{
							fstream.Close();
							withDirectory.DelFile(to + file);
							File.Move(temp + file, to + file);

							return true;
						}
						else
						{
							fstream.Close();
							File.Delete(temp + file);
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				withDirectory.DelFile(temp + file);
				return false;
			}
		}

		/// <summary>
		/// Обновляет файлы, которые метод CheckBaseFiles добавил в список
		/// </summary>
		/// <returns>
		/// Возвращает список файлов, скачивание которых закончилось ошибкой
		/// </returns>
		public List<string> UpdateBaseFiles(in VersionManifest manifest, ref LastUpdates updates, string javaPath, CancellationToken cancelToken)
		{
			string gameVersionName = manifest.version.CustomVersionName ?? manifest.version.GameVersion;

			string addr;
			int updated = 0;
			var errors = new List<string>();
			string temp = withDirectory.CreateTempDir();

			BaseDownloadEvent?.Invoke(_updatesCount, 0);

			//скачивание файла версии
			if (_minecraftJar)
			{
				Objects.CommonClientData.FileInfo minecraftJar = manifest.version.MinecraftJar;
				if (minecraftJar.url == null)
				{
					addr = LaunсherSettings.URL.Upload + "versions/" + minecraftJar.name;
				}
				else
				{
					addr = minecraftJar.url;
				}

				var taskArgs = new TaskArgs
				{
					PercentHandler = delegate (int a)
					{
						_fileDownloadHandler?.Invoke(minecraftJar.name, a, DownloadFileProgress.PercentagesChanged);
					},
					CancelToken = cancelToken
				};

				bool isDownload;
				string to = withDirectory.GetInstancePath(instanceId) + "version/";
				if (minecraftJar.notArchived)
				{
					isDownload = SaveDownloadJar(addr, minecraftJar.name, to, temp, minecraftJar.sha1, minecraftJar.size, taskArgs, true);
				}
				else
				{
					isDownload = SaveDownloadZip(addr, minecraftJar.name, to, temp, minecraftJar.sha1, minecraftJar.size, taskArgs);
				}

				DownloadFileProgress downloadResult;
				if (isDownload)
				{
					updates["version"] = minecraftJar.lastUpdate;
					downloadResult = DownloadFileProgress.Successful;
				}
				else
				{
					errors.Add("version/" + minecraftJar.name);
					downloadResult = DownloadFileProgress.Error;
				}

				if (cancelToken.IsCancellationRequested)
				{
					dataFilesManager.SaveLastUpdates(instanceId, updates);
					return errors;
				}

				updated++;
				_fileDownloadHandler?.Invoke(minecraftJar.name, 100, downloadResult);
				BaseDownloadEvent?.Invoke(_updatesCount, updated);

				if (downloadResult == DownloadFileProgress.Error)
				{
					dataFilesManager.SaveLastUpdates(instanceId, updates);
					return errors;
				}
			}

			//скачиваем libraries
			_librariesBlock.WaitOne(gameVersionName);
			try
			{
				string libName = manifest.version.GetLibName;

				var executedMethods = new List<string>();
				string downloadedLibsAddr = withDirectory.DirectoryPath + "/versions/libraries/" + libName + "-downloaded.json"; // адрес файла в котором убдет храниться список downloadedLibs
																												   // TODO: список downloadedLibs мы получаем в методе проверки. брать от туда, а не подгружать опять
				var downloadedLibs = dataFilesManager.GetFile<List<string>>(downloadedLibsAddr); // сюда мы пихаем файлы, которые удачно скачались. При каждом удачном скачивании сохраняем список в файл. Если все файлы скачались удачно - удаляем этот список
				if (downloadedLibs == null) downloadedLibs = new List<string>();
				int startDownloadedLibsCount = downloadedLibs.Count;
				var downloadedLibsLocker = new object();

				if (_libraries.Count > 0) //сохраняем версию либририесов если в списке на обновление(updateList.Libraries) есть хотя бы один либрариес
				{
					dataFilesManager.SaveFile(withDirectory.DirectoryPath + "/versions/libraries/lastUpdates/" + libName + ".lver", manifest.version.LibrariesLastUpdate.ToString());
					if (manifest.version.AdditionalInstaller != null)
					{
						string lName = manifest.version.AdditionalInstaller.GetLibName;
						string lastUpdate = manifest.version.AdditionalInstaller.librariesLastUpdate.ToString();
						dataFilesManager.SaveFile(withDirectory.DirectoryPath + "/versions/additionalLibraries/lastUpdates/" + lName + ".lver", lastUpdate);
					}
				}

				var libsToDownload = new List<string>();
				var libsToObtaining = new List<string>();

				foreach (string lib in _libraries.Keys)
				{
					if (_libraries[lib].obtainingMethod == null) libsToDownload.Add(lib);
					else libsToObtaining.Add(lib);
				}

				if (cancelToken.IsCancellationRequested) return errors;

				TasksPerfomer tasksPerfomer = null;
				if (libsToDownload.Count > 0)
					tasksPerfomer = new TasksPerfomer(7, libsToDownload.Count);

				long librariesVersion = updates["libraries"];
				foreach (string lib in libsToDownload)
				{
					if (cancelToken.IsCancellationRequested) return errors;

					tasksPerfomer.ExecuteTask(() =>
					{
						LibHandle(lib, librariesVersion, cancelToken, gameVersionName, downloadedLibsLocker, downloadedLibs, downloadedLibsAddr, errors, ref updated);
					});
				}

				tasksPerfomer?.WaitEnd();

				if (cancelToken.IsCancellationRequested) return errors;

				foreach (string lib in libsToObtaining)
				{
					if (cancelToken.IsCancellationRequested) return errors;

					List<List<string>> obtainingMethod = _libraries[lib].obtainingMethod; // получаем метод
					ObtainingMethodExecute(in manifest, obtainingMethod, executedMethods, errors, downloadedLibs, lib, javaPath, downloadedLibsAddr, cancelToken);

					updated++;
					BaseDownloadEvent?.Invoke(_updatesCount, updated);
				}

				try
				{
					Directory.Delete(temp, true);
				}
				catch { }

				if (downloadedLibs.Count - startDownloadedLibsCount == _libraries.Count)
				{
					//все либрариесы скачались удачно. Удаляем файл
					withDirectory.DelFile(downloadedLibsAddr);
				}
			}
			finally
			{
				_librariesBlock.Release(gameVersionName);
			}

			//скачиваем assets
			_assetsBlock.WaitOne(gameVersionName);

			// скачиваем файлы objects
			if (_assets.objects != null)
			{
				int assetsCount = _assets.objects.Count;

				TasksPerfomer perfomer = null;
				if (assetsCount > 0)
					perfomer = new TasksPerfomer(7, assetsCount);

				var taskArgs = new TaskArgs
				{
					PercentHandler = delegate (int pr) { },
					CancelToken = cancelToken
				};

				int i = 0;
				foreach (string asset in _assets.objects.Keys)
				{
					perfomer.ExecuteTask(delegate ()
					{
						string assetHash = _assets.objects[asset].hash;
						if (assetHash != null)
						{
							string assetPath = "/" + assetHash.Substring(0, 2);
							if (!File.Exists(withDirectory.DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
							{
								bool flag = false;
								for (int i = 0; i < 3; i++) // 3 попытки делаем
								{
									if (withDirectory.InstallFile("https://resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath, taskArgs))
									{
										flag = true;
										break;
									}
								}

								if (cancelToken.IsCancellationRequested) return;

								// если не скачалось, то пробуем сделать еще одну попытку через прокси
								if (!flag && withDirectory.InstallFile(LaunсherSettings.URL.MirrorUrl + "resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath, taskArgs))
								{
									flag = true;
								}

								if (cancelToken.IsCancellationRequested) return;

								if (!flag)
								{
									_fileDownloadHandler?.Invoke("asstes: " + asset, 100, DownloadFileProgress.Error);
									errors.Add("asstes: " + asset);
									Runtime.DebugWrite("Downloading error " + asset);
								}

								updated++;
								BaseDownloadEvent?.Invoke(_updatesCount, updated);
							}
						}
						else
						{
							_fileDownloadHandler?.Invoke("asstes: " + asset, 100, DownloadFileProgress.Error);
							errors.Add("asstes: " + asset);
						}

						i++;
						_fileDownloadHandler?.Invoke("assets files", (int)(((decimal)i / (decimal)assetsCount) * 100), DownloadFileProgress.PercentagesChanged);
					});

					if (cancelToken.IsCancellationRequested) return errors;
				}

				perfomer?.WaitEnd();
				_fileDownloadHandler?.Invoke("assets files", 100, DownloadFileProgress.Successful);
			}
			else
			{
				errors.Add("asstes/objects");
			}

			//скачиваем json файл
			if (_assetsIndexes)
			{
				if (!File.Exists(withDirectory.DirectoryPath + "/assets/indexes/" + manifest.version.AssetsVersion + ".json"))
				{
					if (!Directory.Exists(withDirectory.DirectoryPath + "/assets/indexes"))
						Directory.CreateDirectory(withDirectory.DirectoryPath + "/assets/indexes");

					string filename = manifest.version.AssetsVersion + ".json";

					var taskArgs = new TaskArgs
					{
						PercentHandler = delegate (int pr)
						{
							_fileDownloadHandler?.Invoke(filename, pr, DownloadFileProgress.PercentagesChanged);
						},
						CancelToken = cancelToken
					};

					if (!withDirectory.InstallFile(manifest.version.AssetsIndexes, filename, "/assets/indexes/", taskArgs))
					{
						string url = manifest.version.AssetsIndexes.Replace("https://", "");
						url = LaunсherSettings.URL.MirrorUrl + url;

						withDirectory.InstallFile(url, filename, "/assets/indexes/", taskArgs);
					}

					_fileDownloadHandler?.Invoke(filename, 100, DownloadFileProgress.Successful);
					// TODO: я на ошибки тут не проверяю то ли потмоу что мне лень было, толи просто не хотел делать так, чтобы от одного этогоф айла клиент не запускался
				}
			}

			_assetsBlock.Release(gameVersionName);

			//сохраняем lastUpdates
			dataFilesManager.SaveLastUpdates(instanceId, updates);

			return errors;
		}

		private void LibHandle(string lib, long librariesVersion, CancellationToken cancelToken, string gameVersionName, object downloadedLibsLocker, List<string> downloadedLibs, string downloadedLibsAddr, List<string> errors, ref int updated)
		{
			bool isNwDownload = false;
			string addr;
			LibInfo libInfo = _libraries[lib];

			if (libInfo.url == null)
			{
				addr = LaunсherSettings.URL.Upload + "libraries/";
				isNwDownload = true;
			}
			else
			{
				addr = libInfo.url;
			}

			string[] folders = lib.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			string libFolder = lib.Replace(folders[folders.Length - 1], "");

			if (addr.Length > 5 && addr.Substring(addr.Length - 4) != ".jar" && addr.Substring(addr.Length - 4) != ".zip")
			{
				addr = addr + lib;
			}

			bool isDownload;
			string name = folders[folders.Length - 1];
			string fileDir = withDirectory.DirectoryPath + "/libraries/" + libFolder;

			var taskArgs = new TaskArgs
			{
				PercentHandler = delegate (int pr)
				{
					_fileDownloadHandler?.Invoke(name, pr, DownloadFileProgress.PercentagesChanged);
				},
				CancelToken = cancelToken
			};

			string tempDir = withDirectory.CreateTempDir();

			if (libInfo.notArchived)
			{
				if (isNwDownload) addr += "?" + librariesVersion;

				isDownload = UnsafeDownloadJar(addr, fileDir, name, tempDir, taskArgs, true);
			}
			else
			{
				if (isNwDownload) addr += ".zip?" + librariesVersion;

				isDownload = UnsafeDownloadZip(addr, fileDir, name, tempDir, taskArgs);
			}

			try
			{
				Directory.Delete(tempDir, true);
			}
			catch { }

			if (cancelToken.IsCancellationRequested) return;

			_fileDownloadHandler?.Invoke(name, 100, isDownload ? DownloadFileProgress.Successful : DownloadFileProgress.Error);

			if (libInfo.isNative && isDownload)
			{
				try
				{
					string tempFolder = withDirectory.CreateTempDir();
					// извлекаем во временную папку
					ZipFile.ExtractToDirectory(fileDir + "/" + name, tempFolder);

					if (!Directory.Exists(withDirectory.DirectoryPath + "/natives/" + gameVersionName + "/"))
					{
						Directory.CreateDirectory(withDirectory.DirectoryPath + "/natives/" + gameVersionName + "/");
					}

					//Скопировать все файлы. И перезаписать (если такие существуют)
					foreach (string newPath in Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories))
					{
						string oldPath = newPath.Replace(tempFolder, withDirectory.DirectoryPath + "/natives/" + gameVersionName + "/");
						string oldPathDir = Path.GetDirectoryName(oldPath);

						if (!Directory.Exists(oldPathDir))
						{
							Directory.CreateDirectory(oldPathDir);
						}

						File.Copy(newPath, oldPath, true);
					}

					Directory.Delete(tempFolder, true);
				}
				catch
				{
					isDownload = false;
				}
			}

			lock (downloadedLibsLocker)
			{
				if (isDownload)
				{
					downloadedLibs.Add(lib);
					dataFilesManager.SaveFile(downloadedLibsAddr, JsonConvert.SerializeObject(downloadedLibs));
				}
				else
				{
					errors.Add("libraries/" + lib);
					withDirectory.DelFile(withDirectory.DirectoryPath + "/libraries/" + lib);
				}
			}

			updated++;
			BaseDownloadEvent?.Invoke(_updatesCount, updated);
		}


		private void ObtainingMethodExecute(in VersionManifest manifest, List<List<string>> obtainingMethod, List<string> executedMethods, List<string> errors, List<string> downloadedLibs, string lib, string javaPath, string downloadedLibsAddr, CancellationToken cancelToken)
		{
			try
			{
				var vars = new Dictionary<string, string>(); //здесь хранятся переменные этого метода

				if (!executedMethods.Contains(obtainingMethod[0][0])) //проверяем был ли этот метод уже выполнен
				{
					string tempDir = withDirectory.CreateTempDir();

					int i = 1; //начинаем цикл с первого элемента, т.к нулевой - название метода
					while (i < obtainingMethod.Count)
					{
						// получаем команду и выполняем её
						switch (obtainingMethod[i][0])
						{
							case "downloadFile":
								{
									string fileName = obtainingMethod[i][2];
									string downloadUrl = obtainingMethod[i][1];

									foreach (string key in vars.Keys)
									{
										fileName = fileName.Replace(key, vars[key]);
										downloadUrl = downloadUrl.Replace(key, vars[key]);
									}

									var taskArgs = new TaskArgs
									{
										PercentHandler = delegate (int pr)
										{
											_fileDownloadHandler?.Invoke(fileName, pr, DownloadFileProgress.PercentagesChanged);
										},
										CancelToken = cancelToken
									};

									if (!withDirectory.DownloadFile(downloadUrl, fileName, tempDir, taskArgs))
									{
										_fileDownloadHandler?.Invoke(fileName, 100, DownloadFileProgress.Error);
										goto EndWhile; //возникла ошибка
									}

									_fileDownloadHandler?.Invoke(fileName, 100, DownloadFileProgress.Successful);
								}
								break;
							case "unzipFile":
								ZipFile.ExtractToDirectory(tempDir + obtainingMethod[i][1], tempDir + obtainingMethod[i][2]);
								break;
							case "startProcess":
								{
									Utils.ProcessExecutor executord;
									string processExecutord = obtainingMethod[i][1];

									if (processExecutord == "java")
									{
										executord = Utils.ProcessExecutor.Java;
									}
									else if (processExecutord == "cmd")
									{
										executord = Utils.ProcessExecutor.Cmd;
									}
									else
									{
										goto EndWhile; //возникла ошибка
									}

									string command = obtainingMethod[i][2];

									Runtime.DebugWrite();
									Runtime.DebugWrite("Command pattern: " + command);
									Runtime.DebugWrite("{DIR}: " + withDirectory.DirectoryPath);
									Runtime.DebugWrite("{TEMP_DIR}: " + tempDir);
									Runtime.DebugWrite("{MINECRAFT_JAR}: " + withDirectory.GetInstancePath(instanceId) + "version/" + manifest.version.MinecraftJar.name);

									command = command.Replace("{DIR}", withDirectory.DirectoryPath);
									command = command.Replace("{TEMP_DIR}", tempDir);
									string minecraftJar = "\"" + withDirectory.GetInstancePath(instanceId) + "version/" + manifest.version.MinecraftJar.name + "\"";
									command = command.Replace("\"{MINECRAFT_JAR}\"", minecraftJar);
									command = command.Replace("{MINECRAFT_JAR}", minecraftJar);

									Runtime.DebugWrite(executord + " " + command);

									if (!Utils.StartProcess(command, executord, javaPath))
									{
										errors.Add("libraries/" + lib);
										goto EndWhile; //возникла ошибка
									}
								}
								break;

							case "moveFile":
								{
									string from = obtainingMethod[i][1].Replace("{DIR}", withDirectory.DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
									string to = obtainingMethod[i][2].Replace("{DIR}", withDirectory.DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
									if (File.Exists(to))
									{
										File.Delete(to);
									}
									if (!Directory.Exists(to.Replace(Path.GetFileName(to), "")))
									{
										Directory.CreateDirectory(to.Replace(Path.GetFileName(to), ""));
									}

									File.Move(from, to);

								}
								break;
							case "copyFile":
								{
									string from = obtainingMethod[i][1].Replace("{MINECRAFT_JAR}", withDirectory.GetInstancePath(instanceId) + "version/" + manifest.version.MinecraftJar.name).Replace("//", "/");
									string to = obtainingMethod[i][2].Replace("{DIR}", withDirectory.DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
									if (File.Exists(to))
									{
										File.Delete(to);
									}

									string d = to.Replace(Path.GetFileName(to), "");
									if (!Directory.Exists(d))
									{
										Directory.CreateDirectory(d);
									}

									File.Copy(from, to);
								}
								break;
							case "findOnPage":
								{
									string input = webService.HttpGet(obtainingMethod[i][1]); // получем содержимое страницы по url

									//по регулярке из этого метода ищем нужную строку
									Regex regex = new Regex(obtainingMethod[i][2]);
									var result = regex.Match(input);
									//закидывеам полученное значение в список переменных
									vars["{@" + obtainingMethod[i][3] + "}"] = result.Groups[1].ToString();
								}
								break;
						}
						i++;
					}

					try
					{
						Directory.Delete(tempDir, true);
					}
					catch { }
				}

			//теперь добавляем этот метод в уже выполненные и если не существует файла, который мы должны получить - значит произошла ошибка
			EndWhile: executedMethods.Add(obtainingMethod[0][0]);

				if (!File.Exists(withDirectory.DirectoryPath + "/libraries/" + lib))
				{
					errors.Add("libraries/" + lib);
				}
				else
				{
					downloadedLibs.Add(lib);
					dataFilesManager.SaveFile(downloadedLibsAddr, JsonConvert.SerializeObject(downloadedLibs));
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Obtainig method exception " + ex);
				errors.Add("libraries/" + lib);
			}
		}

		/// <summary>
		/// Изменяет id сборки и создает соотвествующий каталог. Если каталог уже есть, то переименовывает его.
		/// </summary>
		public void ChangeInstanceId(string newId)
		{
			try
			{
				if (instanceId != null)
				{
					string oldPath = withDirectory.GetInstancePath(instanceId);
					if (!Directory.Exists(oldPath))
					{
						string newPaath = withDirectory.GetInstancePath(newId);
						if (!Directory.Exists(newPaath)) Directory.CreateDirectory(newPaath);
						instanceId = newId;
						return;
					}

					Directory.Move(withDirectory.GetInstancePath(instanceId), withDirectory.GetInstancePath(newId));
				}

				instanceId = newId;
				Directory.CreateDirectory(withDirectory.GetInstancePath(instanceId));
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite(ex);
				instanceId = newId;
			}
		}
	}
}
