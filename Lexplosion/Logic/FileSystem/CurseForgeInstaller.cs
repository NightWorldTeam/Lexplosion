using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Lexplosion.Tools;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    class CurseforgeInstaller : InstanceInstaller
    {
        public CurseforgeInstaller(string instanceId) : base(instanceId) { }

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

                        foreach (int addonId in content.InstalledAddons)
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
                    InstalledAddons = new List<int>(content.InstalledAddons.Keys.ToArray())
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
        public InstanceManifest DownloadInstance(string downloadUrl, string fileName, ref InstanceContent localFiles)
        {
            //try
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

                // скачивание архива
                MainFileDownloadEvent?.Invoke(0);
                bool res = DownloadFile(downloadUrl, fileName, tempDir, delegate(int percent) 
                {
                    _fileDownloadHandler?.Invoke(fileName, percent, DownloadFileProgress.PercentagesChanged);
                    MainFileDownloadEvent?.Invoke(percent);
                });

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

                var data = GetFile<InstanceManifest>(tempDir + "dataDownload/manifest.json");

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
            /*catch
            {
                return null;
            }*/
        }

        /// <summary>
        /// Скачивает все аддоны модпака из спика
        /// </summary>
        /// <returns>
        /// Возвращает список ошибок.
        /// </returns>
        public List<string> InstallInstance(InstanceManifest data, InstanceContent localFiles)
        {
            InstalledAddonsFormat installedAddons = null;
            installedAddons = localFiles.InstalledAddons;

            var errors = new List<string>();

            //try
            {
                InstanceContent compliteDownload = new InstanceContent
                {
                    InstalledAddons = new InstalledAddonsFormat(),
                    Files = localFiles.Files
                };

                List<InstanceManifest.FileData> downloadList = new List<InstanceManifest.FileData>();

                int test = 0;

                if (installedAddons != null)
                {
                    Console.WriteLine("fdsv " + installedAddons.Count);
                    List<int> tempList = new List<int>(); // этот список содержит айдишники аддонов, что есть в списке уже установленных и в списке с курсфорджа
                    foreach (InstanceManifest.FileData file in data.files) // проходимся по списку адднов, полученному с курсфорджа
                    {
                        if (!installedAddons.ContainsKey(file.projectID)) // если этого аддона нету в списке уже установленных, то тогда кидаем на обновление
                        {
                            test++;
                            downloadList.Add(file);
                        }
                        else
                        {
                            tempList.Add(file.projectID); // Аддон есть в списке установленых. Добавляем его айдишник в список

                            if (installedAddons[file.projectID].FileID < file.fileID || !installedAddons[file.projectID].IsExists(DirectoryPath + "/instances/" + instanceId + "/"))
                            {
                                test++;
                                downloadList.Add(file);
                            }
                        }
                    }

                    foreach (int addonId in installedAddons.Keys) // проходимя по списку установленных аддонов
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

                Console.WriteLine("qweqwe " + test);

                int filesCount = downloadList.Count;
                AddonsDownloadEvent?.Invoke(filesCount, 0);

                if (filesCount != 0)
                {
                    SaveInstanceContent(compliteDownload);

                    object fileBlock = new object(); // этот объект блокировщик нужен что бы синхронизировать работу с json файлами

                    // формируем список айдишников
                    int[] ids = new int[filesCount];
                    int j = 0;
                    foreach (InstanceManifest.FileData file in downloadList)
                    {
                        ids[j] = file.projectID;
                        j++;
                    }

                    // получем инфу о всех аддонах
                    List<CurseforgeAddonInfo> addnos_ = CurseforgeApi.GetAddonsInfo(ids);
                    if (addnos_ == null) // у этих долбаебов просто по  приколу может упасть соединение во время запроса. пробуем второй раз
                    {
                        Thread.Sleep(5000);
                        addnos_ = CurseforgeApi.GetAddonsInfo(ids);
                    }
                    //преобразовываем эту хуйню в нормальный спсиок
                    Dictionary<int, CurseforgeAddonInfo> addons = new Dictionary<int, CurseforgeAddonInfo>();
                    if (addnos_ != null)
                    {
                        foreach (CurseforgeAddonInfo addon in addnos_)
                        {
                            addons[addon.id] = addon;
                        }
                    }

                    TasksPerfomer perfomer = null;
                    if (filesCount > 0)
                        perfomer = new TasksPerfomer(10, filesCount);

                    var noDownloaded = new ConcurrentBag<InstanceManifest.FileData>();
                    int downloadedCount = 0;

                    Console.WriteLine("СКАЧАТЬ БЛЯТЬ НАДО " + downloadList.Count + " ЗЛОЕБУЧИХ МОДОВ");
                    foreach (InstanceManifest.FileData file in downloadList)
                    {
                        perfomer.ExecuteTask(delegate ()
                        {
                            Console.WriteLine("ADD MOD TO PERFOMER");
                            CurseforgeAddonInfo addonInfo;
                            if (addons.ContainsKey(file.projectID))
                            {
                                addonInfo = addons[file.projectID];
                            }
                            else
                            {
                                addonInfo = CurseforgeApi.GetAddonInfo(file.projectID.ToString());
                            }

                            var result = CurseforgeApi.DownloadAddon(addonInfo, file.fileID, "/instances/" + instanceId + "/", delegate (int percent)
                            {
                                _fileDownloadHandler?.Invoke(addonInfo.name, percent, DownloadFileProgress.PercentagesChanged);
                            });

                            _fileDownloadHandler?.Invoke(addonInfo.name, 100, DownloadFileProgress.Successful);

                            if (result.Value2 == DownloadAddonRes.Successful)
                            {
                                downloadedCount++;
                                AddonsDownloadEvent?.Invoke(filesCount, downloadedCount);
                            }
                            else //скачивание мода не удалось.
                            {
                                Console.WriteLine("ERROR " + result.Value2 + " " + result.Value1);
                                noDownloaded.Add(file);
                            }

                            lock (fileBlock)
                            {
                                compliteDownload.InstalledAddons[file.projectID] = result.Value1;
                                Console.WriteLine("GGHT " + compliteDownload.InstalledAddons.Count);
                                SaveInstanceContent(compliteDownload);
                            }

                            Console.WriteLine("EXIT PERFOMER");
                        });
                    }

                    Console.WriteLine("ЖДЁМ КОНЦА ");
                    perfomer?.WaitEnd();
                    Console.WriteLine("КОНЕЦ ");

                    Console.WriteLine("ДОКАЧИВАЕМ " + noDownloaded.Count);
                    foreach (InstanceManifest.FileData file in noDownloaded)
                    {
                        CurseforgeAddonInfo addonInfo = CurseforgeApi.GetAddonInfo(file.projectID.ToString());
                        if (addonInfo.latestFiles == null && addons.ContainsKey(file.projectID))
                        {
                            addonInfo = addons[file.projectID];
                        }

                        int count = 0;
                        ValuePair<InstalledAddonInfo, DownloadAddonRes> result = CurseforgeApi.DownloadAddon(addonInfo, file.fileID, "/instances/" + instanceId + "/", delegate (int percent)
                        {
                            _fileDownloadHandler?.Invoke(addonInfo.name, percent, DownloadFileProgress.PercentagesChanged);
                        });

                        while (count < 4 && result.Value2 != DownloadAddonRes.Successful)
                        {
                            Thread.Sleep(1000);
                            Console.WriteLine("REPEAT DOWNLOAD " + addonInfo.id);
                            addonInfo = CurseforgeApi.GetAddonInfo(file.projectID.ToString());

                            result = CurseforgeApi.DownloadAddon(addonInfo, file.fileID, "/instances/" + instanceId + "/", delegate (int percent)
                            {
                                _fileDownloadHandler?.Invoke(addonInfo.name, percent, DownloadFileProgress.PercentagesChanged);
                            });

                            count++;
                        }

                        if (result.Value2 != DownloadAddonRes.Successful)
                        {
                            Console.WriteLine("ХУЙНЯ, НЕ СКАЧАЛОСЬ " + file.projectID + " " + result.Value2);
                            if (addonInfo != null)
                            {
                                errors.Add(addonInfo.name + ", File id: " + file.fileID);
                            }
                            else
                            {
                                errors.Add("Project Id: " + file.projectID + ", File id: " + file.fileID);
                            }

                            _fileDownloadHandler?.Invoke(addonInfo.name, 100, DownloadFileProgress.Error);
                        }
                        else
                        {
                            _fileDownloadHandler?.Invoke(addonInfo.name, 100, DownloadFileProgress.Successful);
                        }

                        downloadedCount++;
                        AddonsDownloadEvent?.Invoke(filesCount, downloadedCount);
                    }
                }

                if (errors.Count == 0)
                {
                    compliteDownload.FullClient = true;
                }

                SaveInstanceContent(compliteDownload);
                Console.WriteLine("END INSTALL INSTANCE");

                return errors;
            }
            /*catch
            {
                errors.Add("uncnowError");
                return null;
            }*/
        }
    }
}
