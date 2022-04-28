using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    class CurseforgeInstaller : InstanceInstaller
    {
        public CurseforgeInstaller(string instanceId): base(instanceId) { }

        public class InstanceManifest
        {
            public class McVersionInfo
            {
                public string version;
                public List<ModLoaders> modLoaders;
            }

            public class ModLoaders
            {
                public string id;
                public bool primary;
            }

            public class FileData
            {
                public int projectID;
                public int fileID;
            }

            public McVersionInfo minecraft;
            public string name;
            public string version;
            public string author;
            public List<FileData> files;
        }

        public class LocalFiles
        {
            public InstalledAddons InstalledAddons;
            public List<string> Files;
            public bool FullClient = false;
        }

        public event ProcentUpdate MainFileDownloadEvent;
        public event ProcentUpdate AddonsDownloadEvent;

        /// <summary>
        /// Проверяет все ли файлы клиента присутсвуют
        /// </summary>
        public bool InvalidStruct(LocalFiles localFiles)
        {
            if (localFiles.Files == null || localFiles.InstalledAddons == null || !localFiles.FullClient)
            {
                return true;
            }

            foreach (InstalledAddonInfo addon in localFiles.InstalledAddons.Values)
            {
                string filePath = DirectoryPath + "/instances/" + instanceId + "/" + addon.ActualPath;

                if (!File.Exists(filePath))
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
        public InstanceManifest DownloadInstance(string downloadUrl, string fileName, ref LocalFiles localFiles)
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
                DelFile(tempDir + fileName);

                // скачивание архива
                using (WebClient wc = new WebClient())
                {
                    MainFileDownloadEvent?.Invoke(1, 0);
                    DelFile(tempDir + fileName);
                    wc.DownloadFile(downloadUrl, tempDir + fileName);
                    MainFileDownloadEvent?.Invoke(1, 1);
                }

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
        public List<string> InstallInstance(InstanceManifest data, LocalFiles localFiles)
        {
            InstalledAddons installedAddons = null;
            installedAddons = localFiles.InstalledAddons;

            var errors = new List<string>();

            //try
            {
                LocalFiles compliteDownload = new LocalFiles
                {
                    InstalledAddons = new InstalledAddons(),
                    Files = localFiles.Files
                };

                List<string> delList = new List<string>();
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

                            if (installedAddons[file.projectID].FileID < file.fileID || !File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + installedAddons[file.projectID].ActualPath))
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

                int i = 0;
                int addonsCount = downloadList.Count;
                AddonsDownloadEvent?.Invoke(addonsCount, i);

                int filesCount = downloadList.Count;

                if (filesCount != 0)
                {
                    Semaphore sem = new Semaphore(15, 15); // этот семафор нужен чтобы за раз не запустилось более 15 потоков
                    ManualResetEvent endEvent = new ManualResetEvent(false); // эта хуйня сработает когда все потоки завершат работу и все аддоны будут скачаны
                    Semaphore fileBlock = new Semaphore(1, 1); // этот семофор нужен что бы синхронизировать работу с фалом localFiles.json

                    Console.WriteLine("СКАЧАТЬ БЛЯТЬ НАДО " + downloadList.Count + " ЗЛОЕБУЧИХ МОДОВ");
                    foreach (InstanceManifest.FileData file in downloadList)
                    {
                        sem.WaitOne();
                        new Thread(delegate ()
                        {
                            sem.WaitOne();

                            var result = CurseforgeApi.DownloadAddon(file.projectID, file.fileID, "/instances/" + instanceId + "/");

                            if (result[result.First().Key].Item2 != CurseforgeApi.DownloadAddonRes.Successful) //скачивание мода не удалось.
                            {
                                Console.WriteLine("ERROR " + result[result.First().Key].Item2 + " " + result.First().Key);

                                // если вылезли эти ошибки, то возможно это временная ошибка курсфорджа. Пробуем еще 4 раза
                                if (result[result.First().Key].Item2 == CurseforgeApi.DownloadAddonRes.ProjectIdError || result[result.First().Key].Item2 == CurseforgeApi.DownloadAddonRes.DownloadError)
                                {
                                    int j = 0;
                                    while (j < 4 && result[result.First().Key].Item2 != CurseforgeApi.DownloadAddonRes.Successful)
                                    {
                                        Console.WriteLine("REPEAT DOWNLOAD");
                                        result = CurseforgeApi.DownloadAddon(file.projectID, file.fileID, "/instances/" + instanceId + "/");
                                        j++;
                                    }

                                    // все попытки были неудачными. возвращаем ошибку
                                    if (result[result.First().Key].Item2 != CurseforgeApi.DownloadAddonRes.Successful)
                                    {
                                        sem.Release();
                                        Console.WriteLine("GFDGS пизда");
                                        //errors.Add(file.projectID + " " + file.fileID);
                                        return;
                                    }
                                }
                                else
                                {
                                    sem.Release();
                                    //errors.Add(file.projectID + " " + file.fileID);
                                    return;
                                }
                            }

                            fileBlock.WaitOne();
                            compliteDownload.InstalledAddons[file.projectID] = result[result.First().Key].Item1;
                            Console.WriteLine("GGHT " + compliteDownload.InstalledAddons.Count);
                            DataFilesManager.SaveFile(DirectoryPath + "/instances/" + instanceId + "/localFiles.json", JsonConvert.SerializeObject(compliteDownload));
                            fileBlock.Release();

                            i++;
                            AddonsDownloadEvent?.Invoke(addonsCount, i);

                            filesCount--;
                            if (filesCount == 0)
                            {
                                endEvent.Set();
                            }

                            sem.Release();
                        }).Start();

                        sem.Release();
                    }

                    Console.WriteLine("ЖДЁМ КОНЦА ");
                    endEvent.WaitOne();
                    Console.WriteLine("КОНЕЦ ");
                }

                if (errors.Count == 0)
                {
                    compliteDownload.FullClient = true; // TODO: учитывать ошибки
                }

                DataFilesManager.SaveFile(DirectoryPath + "/instances/" + instanceId + "/localFiles.json", JsonConvert.SerializeObject(compliteDownload));

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
