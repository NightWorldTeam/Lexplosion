using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Lexplosion.Logic.FileSystem.Installers
{
    abstract class StandartInstanceInstaller<TManifest> : InstanceInstaller, IArchivedInstanceInstaller<TManifest>
    {
        public StandartInstanceInstaller(string instanceId, IFileServicesContainer servicesContainer) : base(instanceId, servicesContainer) { }

        public event Action<int> MainFileDownload;
        public event ProcentUpdate AddonsDownload;

        /// <summary>
        /// Вызывается, когда необходимо загрузить манифест из разорхивированного файла со сборкой. 
        /// </summary>
        /// <param name="unzupArchivePath"></param>
        /// <returns>Манифест</returns>
        protected abstract TManifest LoadManifest(string unzupArchivePath);

        /// <summary>
        /// Вызывается когда нужно обработать разорхивированный архив со сборкой. 
        /// </summary>
        /// <param name="unzupArchivePath">Путь до папки, содержащей разорхивированный архив.</param>
        /// <param name="files">Список файлов клиента.</param>
        /// <returns>Манифест</returns>
        protected abstract void ArchiveHadnle(string unzupArchivePath, out List<string> files);

        public abstract List<string> Install(TManifest data, InstanceContent localFiles, CancellationToken cancelToken);

        protected void AddonsDownloadEventInvoke(int totalDataCount, int nowDataCount)
        {
            AddonsDownload?.Invoke(totalDataCount, nowDataCount);
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

                string instancePath = withDirectory.GetInstancePath(instanceId);

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

        private string _extractedFilesDir;

        public TManifest Extraction(InstanceFileGetter instanceFileGetter, CancellationToken cancelToken)
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

                if (Directory.Exists(_extractedFilesDir + "dataDownload"))
                {
                    Directory.Delete(_extractedFilesDir + "dataDownload", true);
                }

                // Извлекаем содержимое этого архима
                Directory.CreateDirectory(_extractedFilesDir + "dataDownload");
                ZipFile.ExtractToDirectory(res.Item2, _extractedFilesDir + "dataDownload");

                return LoadManifest(_extractedFilesDir + "dataDownload/");
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return default;
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
                    }
                }

                ArchiveHadnle(_extractedFilesDir + "dataDownload/", out List<string> files);
                localFiles.Files = files;

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

        public void SetInstanceId(string id)
        {
            ChangeInstanceId(id);
            instanceId = id;
        }
    }
}
