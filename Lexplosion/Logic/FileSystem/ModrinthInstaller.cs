using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Lexplosion.Tools;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Logic.Network.Web;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    class ModrinthInstaller : InstanceInstaller
    {
        public ModrinthInstaller(string instanceId) : base(instanceId) { }

        public delegate void Procent(int procent);

        public event Procent MainFileDownloadEvent;
        public event ProcentUpdate AddonsDownloadEvent;

        public InstanceContent GetInstanceContent()
        {
            var content = DataFilesManager.GetFile<InstanceContentFile>(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/instanceContent.json");
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                if (content != null)
                {
                    var data = new InstanceContent
                    {
                        Files = content.Files,
                        FullClient = content.FullClient,
                        InstalledAddons = null
                    };

                    if (content.InstalledAddons != null)
                    {
                        data.InstalledAddons = new InstalledAddonsFormat();

                        foreach (string addonId in content.InstalledAddons)
                        {
                            if (installedAddons.ContainsKey(addonId))
                            {
                                data.InstalledAddons[addonId] = installedAddons[addonId];
                            }
                        }
                    }

                    return data;
                }
                else
                {
                    return new InstanceContent();
                }
            }
        }

        public void SaveInstanceContent(InstanceContent content)
        {
            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + instanceId + "/instanceContent.json",
                JsonConvert.SerializeObject(new InstanceContentFile
                {
                    FullClient = content.FullClient,
                    Files = content.Files,
                    InstalledAddons = new List<string>(content.InstalledAddons.Keys.ToArray())
                }));

            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId))
            {
                foreach (var key in content.InstalledAddons.Keys)
                {
                    var elem = content.InstalledAddons[key];
                    if (elem != null)
                    {
                        installedAddons[key] = elem;
                    }
                }

                installedAddons.Save();
            }
        }

        /// <summary>
        /// Проверяет все ли файлы клиента присутсвуют
        /// </summary>
        public bool InvalidStruct(InstanceContent localFiles)
        {
            if (localFiles.Files == null || localFiles.InstalledAddons == null || !localFiles.FullClient)
            {
                return true;
            }

            foreach (InstalledAddonInfo addon in localFiles.InstalledAddons.Values)
            {
                if (addon == null)
                {
                    return true;
                }

                string instancePath = DirectoryPath + "/instances/" + instanceId + "/";

                if (!addon.IsExists(instancePath))
                {
                    return true;
                }
            }

            foreach (string file in localFiles.Files)
            {
                if (!File.Exists(DirectoryPath + "/instances/" + instanceId + file))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Скачивает архив с модпаком.
        /// </summary>
        /// <returns>
        /// Возвращает манифест, полученный из архива.
        /// </returns>
        public InstanceManifest DownloadInstance(string downloadUrl, string fileName, ref InstanceContent localFiles, CancellationToken cancelToken)
        {
            try
            {
                //удаляем старые файлы
                if (localFiles.Files != null)
                {
                    foreach (string file in localFiles.Files)
                    {
                        DelFile(DirectoryPath + "/instances/" + instanceId + file);
                    }
                }

                List<string> files = new List<string>();

                string tempDir = CreateTempDir();

                MainFileDownloadEvent?.Invoke(0);

                var taskArgs = new TaskArgs
                {
                    PercentHandler = delegate (int percent)
                    {
                        _fileDownloadHandler?.Invoke(fileName, percent, DownloadFileProgress.PercentagesChanged);
                        MainFileDownloadEvent?.Invoke(percent);
                    },
                    CancelToken = cancelToken
                };

                // скачивание архива
                bool res = DownloadFile(downloadUrl, fileName, tempDir, taskArgs);

                if (!res)
                {
                    _fileDownloadHandler?.Invoke(fileName, 100, DownloadFileProgress.Error);
                    return null;
                }
                _fileDownloadHandler?.Invoke(fileName, 100, DownloadFileProgress.Successful);

                if (Directory.Exists(tempDir + "dataDownload"))
                {
                    Directory.Delete(tempDir + "dataDownload", true);
                }

                // Извлекаем содержимое этого архима
                Directory.CreateDirectory(tempDir + "dataDownload");
                ZipFile.ExtractToDirectory(tempDir + fileName, tempDir + "dataDownload");
                DelFile(tempDir + fileName);

                var data = GetFile<InstanceManifest>(tempDir + "dataDownload/modrinth.index.json");

                // тут переосим нужные файлы из этого архива

                string SourcePath = tempDir + "dataDownload/overrides/";
                string DestinationPath = DirectoryPath + "/instances/" + instanceId + "/";

                foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                {
                    string dir = dirPath.Replace(SourcePath, DestinationPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                    files.Add(newPath.Replace(SourcePath, "/").Replace("\\", "/"));
                }

                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch { }

                localFiles.Files = files;

                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Скачивает все аддоны модпака из спика
        /// </summary>
        /// <returns>
        /// Возвращает список ошибок.
        /// </returns>
        public List<string> InstallInstance(InstanceManifest data, InstanceContent localFiles, CancellationToken cancelToken)
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
                var filesHashes = new Dictionary<int, string>(); // Ключ - номер файла в спике - значение хэш
                int i = 0;
                foreach (InstanceManifest.FileData file in data.files)
                {
                    if (file.hashes != null && file.hashes.ContainsKey("sha512"))
                    {
                        filesHashes[i] = file.hashes["sha512"];
                    }

                    i++;
                }

                Dictionary<string, ModrinthProjectFile> projectFiles = ModrinthApi.GetFilesFromHashes(filesHashes.Values.ToList());

                var downloadList = new List<InstanceManifest.FileData>();

                if (installedAddons != null)
                {
                    var tempList = new List<string>(); // этот список содержит айдишники аддонов, что есть в списке уже установленных и в списке с курсфорджа
                    int j = 0;
                    foreach (InstanceManifest.FileData file in data.files) // проходимся по списку адднов, полученному с курсфорджа
                    {
                        if (filesHashes.ContainsKey(j) && projectFiles.ContainsKey(filesHashes[i]))
                        {
                            string projectId = projectFiles[filesHashes[i]].ProjectId;
                            if (!installedAddons.ContainsKey(projectId)) // если этого аддона нету в списке уже установленных, то тогда кидаем на обновление
                            {
                                downloadList.Add(file);
                            }
                            else
                            {
                                tempList.Add(projectId); // Аддон есть в списке установленых. Добавляем его айдишник в список

                                InstalledAddonInfo addonInfo = installedAddons[projectId];
                                bool isValidProject = (file.hashes?.ContainsKey("sha512") ?? false) && projectFiles.ContainsKey(file.hashes["sha512"]);
                                if (isValidProject && addonInfo.FileID != projectFiles[file.hashes["sha512"]].FileId || !addonInfo.IsExists(DirectoryPath + "/instances/" + instanceId + "/"))
                                {
                                    downloadList.Add(file);
                                }
                            }
                        }

                        j++;
                    }

                    foreach (string addonId in installedAddons.Keys) // проходимя по списку установленных аддонов
                    {
                        if (!tempList.Contains(addonId)) // если аддона нету в этом списке, значит его нету в списке, полученном с курсфорджа. Поэтому удаляем
                        {
                            if (installedAddons[addonId].ActualPath != null)
                            {
                                DelFile(DirectoryPath + "/instances/" + instanceId + installedAddons[addonId].ActualPath);
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
                    downloadList = data.files;
                }

                int filesCount = downloadList.Count;
                AddonsDownloadEvent?.Invoke(filesCount, 0);

                if (filesCount != 0)
                {
                    SaveInstanceContent(compliteDownload);

                    object fileBlock = new object(); // этот объект блокировщик нужен что бы синхронизировать работу с json файлами

                    TasksPerfomer perfomer = null;
                    if (filesCount > 0)
                        perfomer = new TasksPerfomer(10, filesCount);

                    var noDownloaded = new ConcurrentBag<InstanceManifest.FileData>();
                    int downloadedCount = 0;

                    Runtime.DebugWrite("СКАЧАТЬ БЛЯТЬ НАДО " + downloadList.Count + " ЗЛОЕБУЧИХ МОДОВ");
                    foreach (InstanceManifest.FileData file in downloadList)
                    {
                        perfomer.ExecuteTask(delegate ()
                        {
                            Runtime.DebugWrite("ADD MOD TO PERFOMER");

                            if (file.path == null || file.downloads == null || file.downloads.Count() == 0)
                            {
                                //ошибку возвращать
                            }

                            var taskArgs = new TaskArgs
                            {
                                PercentHandler = delegate (int percent)
                                {
                                    _fileDownloadHandler?.Invoke(file.path, percent, DownloadFileProgress.PercentagesChanged);
                                },
                                CancelToken = cancelToken
                            };

                            if (file.hashes != null && file.hashes.ContainsKey("sha512") && projectFiles.ContainsKey(file.hashes["sha512"]))
                            {
                                ModrinthProjectFile projectFile = projectFiles[file.hashes["sha512"]];
                                ModrinthProjectType addontype;
                                if (file.path.StartsWith("mods/"))
                                {
                                    addontype = ModrinthProjectType.Mod;
                                }
                                else if (file.path.StartsWith("resourcepacks/"))
                                {
                                    addontype = ModrinthProjectType.Resourcepack;
                                }
                                else if (file.path.StartsWith("shaders/"))
                                {
                                    addontype = ModrinthProjectType.Shader;
                                }
                                else
                                {
                                    addontype = ModrinthProjectType.Unknown;
                                }

                                var result = ModrinthApi.DownloadAddon(projectFiles[file.hashes["sha512"]], addontype, "/instances/" + instanceId + "/", taskArgs);

                                _fileDownloadHandler?.Invoke(file.path, 100, DownloadFileProgress.Successful);

                                if (result.Value2 == DownloadAddonRes.Successful)
                                {
                                    downloadedCount++;
                                    AddonsDownloadEvent?.Invoke(filesCount, downloadedCount);
                                }
                                else //скачивание мода не удалось.
                                {
                                    Runtime.DebugWrite("ERROR " + result.Value2 + " " + result.Value1);
                                    noDownloaded.Add(file);
                                }

                                lock (fileBlock)
                                {
                                    compliteDownload.InstalledAddons[projectFile.ProjectId] = result.Value1;
                                    Runtime.DebugWrite("GGHT " + compliteDownload.InstalledAddons.Count);
                                    SaveInstanceContent(compliteDownload);
                                }

                                Runtime.DebugWrite("EXIT PERFOMER");
                            }
                            else
                            {
                                // ошибку возвращать
                            }              
                        });

                        if (cancelToken.IsCancellationRequested) break;
                    }

                    if (!cancelToken.IsCancellationRequested)
                    {
                        perfomer?.WaitEnd();

                        Runtime.DebugWrite("ДОКАЧИВАЕМ " + noDownloaded.Count);
                        foreach (InstanceManifest.FileData file in noDownloaded)
                        {
                            if (cancelToken.IsCancellationRequested) break;

                            var taskArgs = new TaskArgs
                            {
                                PercentHandler = delegate (int percent)
                                {
                                    _fileDownloadHandler?.Invoke(file.path, percent, DownloadFileProgress.PercentagesChanged);
                                },
                                CancelToken = cancelToken
                            };

                            ModrinthProjectType addontype;
                            if (file.path.StartsWith("mods/"))
                            {
                                addontype = ModrinthProjectType.Mod;
                            }
                            else if (file.path.StartsWith("resourcepacks/"))
                            {
                                addontype = ModrinthProjectType.Resourcepack;
                            }
                            else if (file.path.StartsWith("shaders/"))
                            {
                                addontype = ModrinthProjectType.Shader;
                            }
                            else
                            {
                                addontype = ModrinthProjectType.Unknown;
                            }

                            int count = 0;
                            ValuePair<InstalledAddonInfo, DownloadAddonRes> result = ModrinthApi.DownloadAddon(projectFiles[file.hashes["sha512"]], addontype, "/instances/" + instanceId + "/", taskArgs);

                            while (count < 4 && result.Value2 != DownloadAddonRes.Successful && !cancelToken.IsCancellationRequested)
                            {
                                Thread.Sleep(1000);
                                Runtime.DebugWrite("REPEAT DOWNLOAD " + file.path);
                                result = ModrinthApi.DownloadAddon(projectFiles[file.hashes["sha512"]], addontype, "/instances/" + instanceId + "/", taskArgs);

                                count++;
                            }

                            if (result.Value2 != DownloadAddonRes.Successful)
                            {
                                Runtime.DebugWrite("ХУЙНЯ, НЕ СКАЧАЛОСЬ " + file.path + " " + result.Value2);
                                errors.Add("File: " + file.path);

                                _fileDownloadHandler?.Invoke(file.path, 100, DownloadFileProgress.Error);
                            }
                            else
                            {
                                _fileDownloadHandler?.Invoke(file.path, 100, DownloadFileProgress.Successful);
                            }

                            downloadedCount++;
                            AddonsDownloadEvent?.Invoke(filesCount, downloadedCount);
                        }
                    }
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
                errors.Add("uncnownError");
                return null;
            }
        }
    }
}
