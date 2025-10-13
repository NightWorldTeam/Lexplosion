using Lexplosion.Global;
using Lexplosion.Logic.FileSystem.Extensions;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.FreeSource;
using Lexplosion.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Modrinth = Lexplosion.Logic.Objects.Modrinth;

namespace Lexplosion.Logic.FileSystem.Installers
{
    class FreeSourceInstanceInstaller : InstanceInstaller, IArchivedInstanceInstaller<InstanceManifest>
    {
        private ModrinthApi _modrinthApi;
        private CurseforgeApi _curseforgeApi;

        private string _extractedFilesDir;

        public FreeSourceInstanceInstaller(string instanceID, IPlatfromServiceContainer servicesContainer) : base(instanceID, servicesContainer)
        {
            _modrinthApi = servicesContainer.MdApi;
            _curseforgeApi = servicesContainer.CfApi;
        }

        public event Action<int> MainFileDownload;
        public event ProcentUpdate AddonsDownload;

        private class WhiteListManifest
        {
            [JsonProperty("whiteListExists")]
            public bool WhiteListExists = false;
            [JsonProperty("whiteList")]
            public HashSet<FileDesc> WhiteList;
        }

        public InstanceManifest Extraction(InstanceFileGetter instanceFileGetter, CancellationToken cancelToken)
        {
            TaskArgs BuildTaskArgs(string fileName)
            {
                return new TaskArgs
                {
                    PercentHandler = delegate (int percent)
                    {
                        _fileDownloadHandler?.Invoke(fileName, percent, DownloadFileProgress.PercentagesChanged);
                        MainFileDownload?.Invoke(percent);
                    },
                    CancelToken = cancelToken
                };
            }

            try
            {
                _extractedFilesDir = withDirectory.CreateTempDir();

                MainFileDownload?.Invoke(0);

                (bool, string, string) res = instanceFileGetter(_extractedFilesDir, BuildTaskArgs);

                if (!res.Item1)
                {
                    _fileDownloadHandler?.Invoke(res.Item3, 100, DownloadFileProgress.Error);
                    return default;
                }
                _fileDownloadHandler?.Invoke(res.Item3, 100, DownloadFileProgress.Successful);


                string unzipFolder = _extractedFilesDir + "dataDownload/";

                if (Directory.Exists(unzipFolder))
                {
                    Directory.Delete(unzipFolder, true);
                }

                Directory.CreateDirectory(unzipFolder);

                //определяем белый список файлов
                HashSet<FileDesc> files = null;
                var content = dataFilesManager.GetExtendedPlatfromData<FreeSourcePlatformData>(instanceId);
                if (content != null && content.IsValid())
                {
                    string result = webService.HttpGet(LaunсherSettings.URL.Base + "api/freeSources/" + content.sourceId + "/modpacks/" + content.id + "/exutableFilesWhiteList");
                    if (result != null)
                    {
                        try
                        {
                            var data = JsonConvert.DeserializeObject<WhiteListManifest>(result);
                            if (data != null && data.WhiteListExists)
                            {
                                files = data.WhiteList;
                            }
                        }
                        catch { }
                    }
                }

                if (files != null)
                {
                    // у нас есть список разрешенных файлов. Проходимся по всему архиву и берем только нужные файлы
                    using (ZipArchive zip = ZipFile.Open(res.Item2, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            if (cancelToken.IsCancellationRequested) return null;

                            string entryPath = entry.FullName.Replace("\\", "/");
                            bool isModsFolder = entryPath.StartsWith("/files/mods/") || entryPath.StartsWith("files/mods/");

                            // TODO: тут пихнуть больше расширений и потом при портировании на другеи os вписать и их файлы
                            if (isModsFolder || entryPath.EndsWith(".jar") || entryPath.EndsWith(".zip") || entryPath.EndsWith(".dll") || entryPath.EndsWith(".exe"))
                            {
                                Stream entryContent = entry.Open();
                                var fileDesc = new FileDesc(Cryptography.Sha512(entryContent), entry.Length);
                                if (!files.Contains(fileDesc))
                                {
                                    Runtime.DebugWrite("File not allowed. EntryPath: " + entryPath + ", fileDesc: " + fileDesc);
                                    continue;
                                }
                            }

                            string pathToExtract = unzipFolder + entryPath;
                            string folderToExtract = Path.GetDirectoryName(pathToExtract);
                            if (!Directory.Exists(folderToExtract))
                            {
                                Directory.CreateDirectory(folderToExtract);
                            }

                            entry.ExtractToFile(pathToExtract);
                        }
                    }
                }
                else
                {
                    //белого спсика нет. Тупо потрошим архив
                    ZipFile.ExtractToDirectory(res.Item2, unzipFolder);
                }

                var manifest = dataFilesManager.GetFile<InstanceManifest>(unzipFolder + "instanceInfo.json");
                if (manifest == null || (string.IsNullOrWhiteSpace(manifest.GameVersion) && manifest.GameVersionInfo?.IsNan != false))
                {
                    Runtime.DebugWrite("Manifest error. (manifest is null: " + (manifest == null) + "), (GameVersionInfo is null " + (manifest?.GameVersionInfo == null) + ")");
                    return null;
                }

                if (manifest.Addons == null || manifest.Addons.Count < 1)
                {
                    var installedAddons = dataFilesManager.GetFile<InstalledAddonsFormat>(unzipFolder + "files/" + DataFilesManager.INSTALLED_ADDONS_FILE);
                    manifest.Addons = installedAddons ?? new InstalledAddonsFormat();
                }

                return manifest;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return null;
            }
        }

        public bool HandleExtractedFiles(ref InstanceContent localFiles, CancellationToken cancelToken)
        {
            try
            {
                //удаляем старые файлы
                if (localFiles.Files != null)
                {
                    foreach (string file in localFiles.Files)
                    {
                        withDirectory.DelFile(withDirectory.InstancesPath + instanceId + file);
                        if (cancelToken.IsCancellationRequested) return false;
                    }
                }

                localFiles.Files = new List<string>();

                string unzipFolder = _extractedFilesDir + "dataDownload/";
                string sourcePath = unzipFolder + "files/";
                string destinationPath = withDirectory.GetInstancePath(instanceId);

                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string dir = dirPath.Replace(sourcePath, destinationPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                foreach (string path in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(path) != "manifest.jaon")
                    {
                        File.Copy(path, path.Replace(sourcePath, destinationPath), true);
                        localFiles.Files.Add(path.Replace(sourcePath, "/").Replace("\\", "/"));
                    }
                }

                try
                {
                    if (Directory.Exists(_extractedFilesDir))
                    {
                        Directory.Delete(_extractedFilesDir, true);
                    }
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return false;
            }
        }

        public List<string> Install(InstanceManifest data, InstanceContent localFiles, CancellationToken cancelToken)
        {
            var errors = new List<string>();
            InstanceContent compliteDownload = new InstanceContent
            {
                InstalledAddons = new InstalledAddonsFormat(),
                Files = localFiles.Files
            };

            string instanceFolder = withDirectory.InstancesPath + instanceId + "/";

            InstalledAddonsFormat installedAddons = localFiles.InstalledAddons;
            data.Addons ??= new InstalledAddonsFormat();

            var cursefrogeAddons = new List<InstalledAddonInfo>();
            var modrinthAddons = new List<InstalledAddonInfo>();

            if (installedAddons != null)
            {
                foreach (InstalledAddonInfo addon in data.Addons.Values)
                {
                    if (installedAddons.ContainsKey(addon.ProjectID))
                    {
                        var localAddon = installedAddons[addon.ProjectID];
                        if (!localAddon.IsExists(instanceFolder))
                        {
                            if (addon.Source == ProjectSource.Curseforge)
                            {
                                cursefrogeAddons.Add(addon);
                            }
                            else if (addon.Source == ProjectSource.Modrinth)
                            {
                                modrinthAddons.Add(addon);
                            }
                        }
                        else if (localAddon.FileID != addon.FileID || localAddon.Path != addon.Path)
                        {
                            if (addon.Source == ProjectSource.Curseforge)
                            {
                                localAddon.RemoveFromDir(instanceFolder); // TODO: если скачивания будет уотменено, а мод уже удален, то сборка будет сломана, и такой косяк не только здесь
                                cursefrogeAddons.Add(addon);
                            }
                            else if (addon.Source == ProjectSource.Modrinth)
                            {
                                localAddon.RemoveFromDir(instanceFolder);
                                modrinthAddons.Add(addon);
                            }
                        }
                        else
                        {
                            compliteDownload.InstalledAddons[localAddon.ProjectID] = localAddon;
                        }

                    }
                    else
                    {
                        if (addon.Source == ProjectSource.Curseforge)
                        {
                            cursefrogeAddons.Add(addon);
                        }
                        else if (addon.Source == ProjectSource.Modrinth)
                        {
                            modrinthAddons.Add(addon);
                        }
                    }

                    if (cancelToken.IsCancellationRequested) goto End;
                }
            }
            else
            {
                foreach (InstalledAddonInfo addon in data.Addons.Values)
                {
                    if (addon.Source == ProjectSource.Curseforge)
                    {
                        cursefrogeAddons.Add(addon);
                    }
                    else if (addon.Source == ProjectSource.Modrinth)
                    {
                        modrinthAddons.Add(addon);
                    }
                }
            }

            int nowDataCount = 0;
            int totalDataCount = cursefrogeAddons.Count + modrinthAddons.Count;

            string folder = "/instances/" + instanceId + "/";

            foreach (InstalledAddonInfo addon in cursefrogeAddons)
            {
                var taskArgs = new TaskArgs
                {
                    PercentHandler = delegate (int percent)
                    {
                        _fileDownloadHandler?.Invoke(addon.Path, percent, DownloadFileProgress.PercentagesChanged);
                    },
                    CancelToken = cancelToken
                };

                var addonInfo = _curseforgeApi.GetProjectFile(addon.ProjectID, addon.FileID);
                var result = _curseforgeApi.DownloadAddon(addonInfo, addon.Type, folder, withDirectory, taskArgs);

                if (result.Value2 == DownloadAddonRes.Successful)
                {
                    AddonsDownload?.Invoke(totalDataCount, nowDataCount);

                    compliteDownload.InstalledAddons[addon.ProjectID] = result.Value1;
                    SaveInstanceContent(compliteDownload);
                }
                else
                {
                    errors.Add(addon.Path);
                    Runtime.DebugWrite("ERROR " + result.Value2 + " " + result.Value1);
                }

                nowDataCount++;

                if (cancelToken.IsCancellationRequested) goto End;
            }

            foreach (InstalledAddonInfo addon in modrinthAddons)
            {
                var taskArgs = new TaskArgs
                {
                    PercentHandler = delegate (int percent)
                    {
                        _fileDownloadHandler?.Invoke(addon.Path, percent, DownloadFileProgress.PercentagesChanged);
                    },
                    CancelToken = cancelToken
                };

                var addonInfo = _modrinthApi.GetProjectFile(addon.FileID);
                var result = _modrinthApi.DownloadAddon(addonInfo, ProjectTypeConvert(addon.Type), folder, withDirectory, taskArgs);

                if (result.Value2 == DownloadAddonRes.Successful)
                {
                    AddonsDownload?.Invoke(totalDataCount, nowDataCount);

                    compliteDownload.InstalledAddons[addon.ProjectID] = result.Value1;
                    SaveInstanceContent(compliteDownload);
                }
                else
                {
                    errors.Add(addon.Path);
                    Runtime.DebugWrite("ERROR " + result.Value2 + " " + result.Value1);
                }

                nowDataCount++;

                if (cancelToken.IsCancellationRequested) break;
            }

            if (errors.Count == 0 && !cancelToken.IsCancellationRequested)
            {
                compliteDownload.FullClient = true;
            }

        End:
            SaveInstanceContent(compliteDownload);
            Runtime.DebugWrite("END INSTALL INSTANCE");

            return errors;
        }

        private static Modrinth.ModrinthProjectType ProjectTypeConvert(AddonType addonType)
        {
            switch (addonType)
            {
                case AddonType.Mods: return Modrinth.ModrinthProjectType.Mod;
                case AddonType.Resourcepacks: return Modrinth.ModrinthProjectType.Resourcepack;
                case AddonType.Shaders: return Modrinth.ModrinthProjectType.Shader;
                default: return Modrinth.ModrinthProjectType.Unknown;
            }
        }

        public InstanceContent GetInstanceContent()
        {
            var content = dataFilesManager.GetInstanceContent(instanceId);
            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId, dataFilesManager))
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
            dataFilesManager.SaveInstanceContent(instanceId, new InstanceContentFile
            {
                FullClient = content.FullClient,
                Files = content.Files,
                InstalledAddons = new List<string>(content.InstalledAddons.Keys.ToArray())
            });

            using (InstalledAddons installedAddons = InstalledAddons.Get(instanceId, dataFilesManager))
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
            if (localFiles?.Files == null || localFiles.InstalledAddons == null || !localFiles.FullClient)
            {
                return true;
            }

            foreach (InstalledAddonInfo addon in localFiles.InstalledAddons.Values)
            {
                if (addon == null)
                {
                    return true;
                }

                string instancePath = withDirectory.InstancesPath + instanceId + "/";

                if (!addon.IsExists(instancePath))
                {
                    return true;
                }
            }

            foreach (string file in localFiles.Files)
            {
                if (!File.Exists(withDirectory.InstancesPath + instanceId + file))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetInstanceId(string id)
        {
            ChangeInstanceId(id);
            instanceId = id;
        }
    }
}
