using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Linq;
using System;
using Newtonsoft.Json;
using Lexplosion.Tools;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using static Lexplosion.Logic.FileSystem.WithDirectory;

namespace Lexplosion.Logic.FileSystem
{
    abstract class StandartInstanceInstaller<TManifest> : InstanceInstaller, IArchivedInstanceInstaller<TManifest>
    {
        public StandartInstanceInstaller(string instanceId) : base(instanceId) { }

        public event Action<int> MainFileDownload;
        public event ProcentUpdate AddonsDownload;

        /// <summary>
        /// Вызывает когда нужно обработать разорхивированный архив со сборкой. 
        /// </summary>
        /// <param name="unzupArchivePath">Путь до папки, содержащей разорхивированный архив.</param>
        /// <param name="files">Список файлов клиента.</param>
        /// <returns>Манифест</returns>
        protected abstract TManifest ArchiveHadnle(string unzupArchivePath, out List<string> files);

        public abstract List<string> Install(TManifest data, InstanceContent localFiles, CancellationToken cancelToken);

        protected void AddonsDownloadEventInvoke(int totalDataCount, int nowDataCount)
        {
            AddonsDownload?.Invoke(totalDataCount, nowDataCount);
        }

        public InstanceContent GetInstanceContent()
        {
            var content = DataFilesManager.GetFile<InstanceContentFile>(WithDirectory.InstancesPath + instanceId + "/instanceContent.json");
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
            DataFilesManager.SaveFile(WithDirectory.InstancesPath + instanceId + "/instanceContent.json",
                JsonConvert.SerializeObject(new InstanceContentFile
                {
                    FullClient = content.FullClient,
                    Files = content.Files,
                    InstalledAddons = new List<string>(content.InstalledAddons.Keys.ToArray())
                })
            );

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

        public bool InvalidStruct(InstanceContent localFiles)
        {
            if (localFiles == null || localFiles.Files == null || localFiles.InstalledAddons == null || !localFiles.FullClient)
            {
                return true;
            }

            foreach (InstalledAddonInfo addon in localFiles.InstalledAddons.Values)
            {
                if (addon == null)
                {
                    return true;
                }

                string instancePath = InstancesPath + instanceId + "/";

                if (!addon.IsExists(instancePath))
                {
                    return true;
                }
            }

            foreach (string file in localFiles.Files)
            {
                if (!File.Exists(InstancesPath + instanceId + file))
                {
                    return true;
                }
            }

            return false;
        }


        public TManifest Extraction(InstanceFileGetter instanceFileGetter, ref InstanceContent localFiles, CancellationToken cancelToken)
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
                string tempDir = CreateTempDir();

                MainFileDownload?.Invoke(0);

                (bool, string, string) res = instanceFileGetter(tempDir, BuildTaskArgs);

                if (!res.Item1)
                {
                    _fileDownloadHandler?.Invoke(res.Item3, 100, DownloadFileProgress.Error);
                    return default;
                }
                _fileDownloadHandler?.Invoke(res.Item3, 100, DownloadFileProgress.Successful);

                //удаляем старые файлы
                if (localFiles.Files != null)
                {
                    foreach (string file in localFiles.Files)
                    {
                        DelFile(InstancesPath + instanceId + file);
                    }
                }

                if (Directory.Exists(tempDir + "dataDownload"))
                {
                    Directory.Delete(tempDir + "dataDownload", true);
                }

                // Извлекаем содержимое этого архима
                Directory.CreateDirectory(tempDir + "dataDownload");
                ZipFile.ExtractToDirectory(res.Item2, tempDir + "dataDownload");

                var manifest = ArchiveHadnle(tempDir + "dataDownload/", out List<string> files);
                localFiles.Files = files;

                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch { }

                return manifest;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return default;
            }
        }

        public void SetInstanceId(string id)
        {
            ChangeInstanceId(id);
            instanceId = id;
        }
    }
}
