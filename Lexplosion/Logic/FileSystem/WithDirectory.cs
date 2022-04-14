using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System.Windows;
using System.Linq;
using Lexplosion.Logic.Management;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    static class WithDirectory
    {
        // TODO: во всём WithDirectory я заменяю элементы адресов директорий через replace. Не знаю как на винде, но на линуксе могут появиться проблемы, ведь replace заменяет подстроки в строке, а не только конечную подстроку

        // Делегат для обновления процентов загрузки
        public delegate void ProcentUpdate(int totalDataCount, int nowDataCount);

        public static string DirectoryPath;

        public struct Assets
        {
            public struct AssetFile
            {
                public string hash;
            }

            public Dictionary<string, AssetFile> objects;
        }

        // этот класс возвращает метод CheckBaseFiles
        public class BaseFilesUpdates
        {
            public Dictionary<string, LibInfo> Libraries = new Dictionary<string, LibInfo>();
            public bool MinecraftJar = false;
            public bool AssetsIndexes = false;
            public Assets Assets;

            public int UpdatesCount = 0;
            public ProcentUpdate ProcentUpdateFunc;
        }

        // TODO: тут ссылок 0
        private class LauncherAssets //этот класс нужен для декодирования json
        {
            public int version;
            public Dictionary<string, InstanceAssets> data;
        }

        public static class NightWorld
        {
            public class ModpackFilesUpdates
            {
                public Dictionary<string, List<string>> Data = new Dictionary<string, List<string>>(); //сюда записываем файлы, которые нужно обновить
                public List<string> OldFiles = new List<string>(); // список старых файлов, которые нуждаются в обновлении
                public bool Successful = true; // удачна или неудачна ли проверка

                public int UpdatesCount = 0;
                public delegate void ProcentUpdate(int totalDataCount, int nowDataCount);
                public ProcentUpdate ProcentUpdateFunc;
            }
            public static List<string> UpdateInstance(ModpackFilesUpdates updatesList, NInstanceManifest filesList, string instanceId, string externalId, ref Dictionary<string, int> updates)
            {
                int updatesCount = 0;
                WebClient wc = new WebClient();
                string tempDir = CreateTempDir();

                string[] folders;
                string addr;
                List<string> errors = new List<string>();

                //скачивание файлов из списка data
                foreach (string dir in updatesList.Data.Keys)
                {
                    foreach (string file in updatesList.Data[dir])
                    {
                        folders = file.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                        if (filesList.data[dir].objects[file].url == null)
                        {
                            addr = LaunсherSettings.URL.Upload + "modpacks/" + externalId + "/" + dir + "/" + file;
                        }
                        else
                        {
                            addr = filesList.data[dir].objects[file].url;
                        }

                        if (!SaveDownloadZip(addr, folders[folders.Length - 1], DirectoryPath + "/instances/" + instanceId + "/" + dir + "/" + file, tempDir, filesList.data[dir].objects[file].sha1, filesList.data[dir].objects[file].size, wc))
                        {
                            errors.Add(dir + "/" + file);
                        }
                        else
                        {
                            updates[dir + "/" + file] = filesList.data[dir].objects[file].lastUpdate; //добавляем файл в список последних обновлений
                        }

                        updatesCount++;
                        updatesList.ProcentUpdateFunc(updatesList.UpdatesCount, updatesCount);

                        // TODO: где-то тут записывать что файл был обновлен, чтобы если загрузка была первана она началась с того же места
                    }
                }

                wc.Dispose();

                //удаляем старые файлы
                foreach (string file in updatesList.OldFiles)
                {
                    if (File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + file))
                    {
                        File.Delete(DirectoryPath + "/instances/" + instanceId + "/" + file);
                        if (updates.ContainsKey(file))
                        {
                            updates.Remove(file);

                            updatesCount++;
                            updatesList.ProcentUpdateFunc(updatesList.UpdatesCount, updatesCount);
                        }
                    }
                }

                //сохарняем updates
                SaveFile(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

                Directory.Delete(tempDir, true);

                return errors;
            }

            public static ModpackFilesUpdates CheckInstance(NInstanceManifest filesInfo, string instanceId, ref Dictionary<string, int> updates)
            {
                var filesUpdates = new ModpackFilesUpdates();

                //Проходимся по списку папок(data) из класса instanceFiles
                foreach (string dir in filesInfo.data.Keys)
                {
                    string folder = DirectoryPath + "/instances/" + instanceId + "/" + dir;

                    try
                    {
                        if (!updates.ContainsKey(dir) || updates[dir] < filesInfo.data[dir].folderVersion) //проверяем версию папки. если она старая - очищаем
                        {
                            if (Directory.Exists(folder))
                            {
                                Directory.Delete(folder, true);
                                Directory.CreateDirectory(folder);
                            }

                            updates[dir] = filesInfo.data[dir].folderVersion;
                            filesUpdates.UpdatesCount += filesInfo.data[dir].objects.Count;
                        }

                        // TODO: тут из lastUpdates удалить все файлы из этой папки

                        //отрываем файл с последними обновлениями и записываем туда updates, который уже содержит последнюю версию папки. Папка сейчас будет пустой, поэтому метод Update в любом случае скачает нужные файлы
                        using (FileStream fstream = new FileStream(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Create, FileAccess.Write))
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(updates));
                            fstream.Write(bytes, 0, bytes.Length);
                            fstream.Close();
                        }
                    }
                    catch { }

                    if (Directory.Exists(folder))
                    {
                        //Получаем список всех файлов в папке
                        string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                        foreach (string file in files) //проходимся по папке
                        {
                            string fileName = file.Replace(folder, "").Remove(0, 1).Replace(@"\", "/");

                            if (filesInfo.data[dir].security) //при включенной защите данной папки удалем левые файлы
                            {
                                try
                                {
                                    using (FileStream fstream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл на чтение
                                    {
                                        byte[] bytes = new byte[fstream.Length];
                                        fstream.Read(bytes, 0, bytes.Length);
                                        fstream.Close();

                                        if (filesInfo.data[dir].objects.ContainsKey(fileName)) // проверяем есть ли этот файл в списке
                                        {
                                            using (SHA1 sha = new SHA1Managed())
                                            {
                                                if (Convert.ToBase64String(sha.ComputeHash(bytes)) != filesInfo.data[dir].objects[fileName].sha1 || bytes.Length != filesInfo.data[dir].objects[fileName].size)
                                                {
                                                    File.Delete(file); //удаляем файл, если не сходится хэш или размер

                                                    if (!filesUpdates.Data.ContainsKey(dir)) //если директория отсутствует в data, то добавляем её 
                                                    {
                                                        filesUpdates.Data.Add(dir, new List<string>());
                                                    }

                                                    filesUpdates.Data[dir].Add(fileName); //добавляем файл в список на обновление
                                                    filesUpdates.UpdatesCount++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            File.Delete(file);
                                        }
                                    }
                                }
                                catch
                                {
                                    //чтение одного из файлов не удалось, стопаем весь процесс
                                    filesUpdates.Successful = false; // проверка неудачна
                                    return filesUpdates;
                                }
                            }

                            //сверяем версию файла с его версией в списке, если версия старая, то отправляем файл на обновление
                            if (filesInfo.data[dir].objects.ContainsKey(fileName))
                            {
                                if (!updates.ContainsKey(dir + "/" + fileName) || updates[dir + "/" + fileName] != filesInfo.data[dir].objects[fileName].lastUpdate)
                                {
                                    if (!filesUpdates.Data.ContainsKey(dir)) //если директория отсутствует в data, то добавляем её 
                                    {
                                        filesUpdates.Data.Add(dir, new List<string>());
                                        filesUpdates.UpdatesCount++;
                                    }

                                    if (!filesUpdates.Data[dir].Contains(fileName))
                                    {
                                        filesUpdates.Data[dir].Add(fileName);
                                        filesUpdates.UpdatesCount++;
                                    }
                                }
                            }
                        }
                    }

                    //ищем отсутвующие файлы
                    foreach (string file in filesInfo.data[dir].objects.Keys)
                    {

                        if (!File.Exists(folder + "/" + file))
                        {
                            if (!filesUpdates.Data.ContainsKey(dir))
                            {
                                filesUpdates.Data.Add(dir, new List<string>());
                                filesUpdates.UpdatesCount++;
                            }

                            if (!filesUpdates.Data[dir].Contains(file))
                            {
                                filesUpdates.Data[dir].Add(file);
                                filesUpdates.UpdatesCount++;
                            }
                        }
                    }
                }

                //ищем старые файлы
                foreach (string folder in filesInfo.data.Keys)
                {
                    foreach (string file in filesInfo.data[folder].oldFiles)
                    {
                        try
                        {
                            if (File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + folder + "/" + file))
                            {
                                filesUpdates.OldFiles.Add(folder + "/" + file);
                                filesUpdates.UpdatesCount++;
                            }

                        }
                        catch { }
                    }
                }

                return filesUpdates;
            }
        }

        public static class CurseForge
        {
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
                public Dictionary<int, CurseforgeApi.InstalledAddonInfo> InstalledAddons;
                public List<string> Files;
                public bool FullClient = false;
            }

            public static bool InvalidStruct(string instanceId, LocalFiles localFiles)
            {
                if (localFiles.Files == null || localFiles.InstalledAddons == null || !localFiles.FullClient)
                {
                    return true;
                }

                foreach (CurseforgeApi.InstalledAddonInfo addon in localFiles.InstalledAddons.Values)
                {
                    if (!File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + addon.Path))
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

            public static InstanceManifest DownloadInstance(string downloadUrl, string instanceId, string fileName, ProcentUpdate progressFunction, ref LocalFiles localFiles)
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
                    if (File.Exists(tempDir + fileName))
                    {
                        File.Delete(tempDir + fileName);
                    }

                    // скачивание архива
                    using (WebClient wc = new WebClient())
                    {
                        progressFunction(1, 0);
                        DelFile(tempDir + fileName);
                        wc.DownloadFile(downloadUrl, tempDir + fileName);
                        progressFunction(1, 1);
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

            public static List<string> InstallInstance(string instanceId, InstanceManifest data, LocalFiles localFiles, ProcentUpdate progressFunction)
            {
                Dictionary<int, CurseforgeApi.InstalledAddonInfo> installedAddons = null;
                installedAddons = localFiles.InstalledAddons;

                var errors = new List<string>();

                //try
                {
                    LocalFiles compliteDownload = new LocalFiles
                    {
                        InstalledAddons = new Dictionary<int, CurseforgeApi.InstalledAddonInfo>(),
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

                                if (installedAddons[file.projectID].FileID < file.fileID || !File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + installedAddons[file.projectID].Path))
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
                                if (installedAddons[addonId].Path != null)
                                {
                                    DelFile(DirectoryPath + "/instances/" + instanceId + installedAddons[addonId].Path);
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
                    progressFunction(addonsCount, i);

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

                                Dictionary<string, (CurseforgeApi.InstalledAddonInfo, CurseforgeApi.DownloadAddonRes)> result =
                                CurseforgeApi.DownloadAddon(file.projectID, file.fileID, "/instances/" + instanceId + "/");

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
                                progressFunction(addonsCount, i);

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

        public static void Create(string path)
        {
            try
            {
                DirectoryPath = path.Replace(@"\", "/");

                if (Directory.Exists(DirectoryPath + "/temp"))
                {
                    Directory.Delete(DirectoryPath + "/temp", true);
                }

                Directory.CreateDirectory(DirectoryPath + "/temp");
            }
            catch { }
        }

        private static Random random = new Random();

        private static string CreateTempDir() // TODO: пр использовании этого метода разными потоками может создаться одна папка на два вызова
        {
            string dirName = DirectoryPath + "/temp";
            string dirName_ = dirName;
            
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            do
            {
                dirName_ = dirName + "/" + new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
            } while (Directory.Exists(dirName_));

            Directory.CreateDirectory(dirName_);

            return dirName_ + "/";
        }

        public static bool InstallFile(string url, string fileName, string path)
        {
            string tempDir = null;

            try
            {
                tempDir = CreateTempDir();

                if (!Directory.Exists(DirectoryPath + path))
                {
                    Directory.CreateDirectory(DirectoryPath + path);
                }

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(url, tempDir + fileName);
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                    File.Move(tempDir + fileName, DirectoryPath + "/" + path + "/" + fileName);
                    Directory.Delete(tempDir, true);
                }

                return true;
            }
            catch
            {
                if (tempDir != null)
                {
                    DelFile(tempDir + fileName);
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                }

                return false;
            }
        }

        public static bool DownloadFile(string url, string fileName, string tempDir)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    DelFile(tempDir + fileName);
                    wc.DownloadFile(url, tempDir + fileName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DropLastUpdates(string instanceId)
        {
            try
            {

                using (FileStream fstream = new FileStream(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Create, FileAccess.Write))
                {
                    fstream.Write(new byte[0], 0, 0);
                    fstream.Close();
                }
            }
            catch { }
        }

        public static Dictionary<string, int> GetLastUpdates(string instanceId)
        {
            Dictionary<string, int> updates = new Dictionary<string, int>();

            try
            {
                if (!File.Exists(DirectoryPath + "/instances/" + instanceId + "/" + "lastUpdates.json"))
                {
                    if (!Directory.Exists(DirectoryPath + "/instances/" + instanceId))
                    {
                        Directory.CreateDirectory(DirectoryPath + "/instances/" + instanceId); //создаем папку с модпаком, если её нет
                    }
                }

                using (FileStream fstream = new FileStream(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл с последними обновлениями
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    try
                    {
                        if (JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes)) != null)
                        {
                            updates = JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes));
                        }
                    }
                    catch
                    {
                        File.Delete(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json");
                    }
                }

            }
            catch { }

            return updates;
        }

        // TODO: его вызов обернуть в try
        public static BaseFilesUpdates CheckBaseFiles(VersionManifest filesInfo, string instanceId, ref Dictionary<string, int> updates) // функция проверяет основные файлы клиента (файл версии, либрариесы и тп)
        {
            BaseFilesUpdates updatesList = new BaseFilesUpdates(); //возвращаемый список обновлений

            //проверяем файл версии
            if (!Directory.Exists(DirectoryPath + "/instances/" + instanceId + "/version"))
            {
                Directory.CreateDirectory(DirectoryPath + "/instances/" + instanceId + "/version"); //создаем папку versions если её нет
                updatesList.MinecraftJar = true; //сразу же добавляем minecraftJar в обновления
                updatesList.UpdatesCount++;
            }
            else
            {
                string minecraftJarFile = DirectoryPath + "/instances/" + instanceId + "/version/" + filesInfo.version.minecraftJar.name;
                if (updates.ContainsKey("version") && File.Exists(minecraftJarFile) && filesInfo.version.minecraftJar.lastUpdate == updates["version"]) //проверяем его наличие и версию
                {
                    if (filesInfo.version.security) //если включена защита файла версии, то проверяем его 
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
                                    if (Convert.ToBase64String(sha.ComputeHash(bytes)) != filesInfo.version.minecraftJar.sha1 || bytes.Length != filesInfo.version.minecraftJar.size)
                                    {
                                        File.Delete(minecraftJarFile); //удаляем файл, если не сходится хэш или размер
                                        updatesList.MinecraftJar = true;
                                        updatesList.UpdatesCount++;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            return null; //чтение файла не удалось, стопаем весь процесс
                        }
                    }
                }
                else
                {
                    updatesList.MinecraftJar = true;
                    updatesList.UpdatesCount++;
                }
            }

            //получаем версию libraries
            if (File.Exists(DirectoryPath + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesInfo.version) + ".lver"))
            {
                try
                {
                    using (FileStream fstream = new FileStream(DirectoryPath + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesInfo.version) + ".lver", FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл с версией libraries
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        int ver = 0;
                        Int32.TryParse(Encoding.UTF8.GetString(fileBytes), out ver);
                        updates["libraries"] = ver;
                    }
                }
                catch
                {
                    updates["libraries"] = 0;
                }
            }
            else
            {
                SaveFile(DirectoryPath + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesInfo.version) + ".lver", "0");
                updates["libraries"] = 0;
            }

            //проверяем папку libraries
            if (!Directory.Exists(DirectoryPath + "/libraries"))
            {
                foreach (string lib in filesInfo.libraries.Keys)
                {
                    updatesList.Libraries[lib] = filesInfo.libraries[lib];
                    updatesList.UpdatesCount++;
                }
            }
            else
            {
                if (filesInfo.version.librariesLastUpdate != updates["libraries"]) //если версия libraries старая, то отправляем на обновления
                {
                    foreach (string lib in filesInfo.libraries.Keys)
                    {
                        updatesList.Libraries[lib] = filesInfo.libraries[lib];
                        updatesList.UpdatesCount++;
                    }
                }
                else
                {
                    // получем файл, в ктором хранятси список либрариесов, которые удачно скачались в прошлый раз
                    List<string> downloadedFiles = new List<string>();
                    string downloadedInfoAddr = DirectoryPath + "/versions/libraries/" + GetLibName(instanceId, filesInfo.version) + "-downloaded.json";
                    bool fileExided = false;
                    if (File.Exists(downloadedInfoAddr))
                    {
                        downloadedFiles = GetFile<List<string>>(downloadedInfoAddr);
                        fileExided = true;
                    }

                    //ищем недостающие файлы
                    foreach (string lib in filesInfo.libraries.Keys)
                    {
                        if ((downloadedFiles == null && fileExided) || !File.Exists(DirectoryPath + "/libraries/" + lib) || (fileExided && downloadedFiles != null && !downloadedFiles.Contains(lib)))
                        {
                            updatesList.Libraries[lib] = filesInfo.libraries[lib];
                            updatesList.UpdatesCount++;
                        }
                    }
                }
            }

            if (!Directory.Exists(DirectoryPath + "/natives/" + filesInfo.version.gameVersion))
            {
                foreach (string lib in filesInfo.libraries.Keys)
                {
                    if (filesInfo.libraries[lib].isNative)
                    {
                        updatesList.Libraries[lib] = filesInfo.libraries[lib];
                        updatesList.UpdatesCount++;
                    }
                }
            }

            // Проверяем assets

            // Пытаемся получить список всех асетсов из json файла
            Assets asstes = GetFile<Assets>(DirectoryPath + "/assets/indexes/" + filesInfo.version.assetsVersion + ".json");

            // Файла нет, или он битый. Получаем асетсы с сервера
            if (asstes.objects == null)
            {
                updatesList.AssetsIndexes = true; //устанавливаем флаг что нужно скачать json файл
                updatesList.UpdatesCount++;

                if (!File.Exists(DirectoryPath + "/assets/indexes/" + filesInfo.version.assetsVersion + ".json"))
                {
                    try
                    {
                        // Получем асетсы с сервера
                        asstes = JsonConvert.DeserializeObject<Assets>(ToServer.HttpGet(filesInfo.version.assetsIndexes));
                    }
                    catch { }
                }
            }

            if (asstes.objects != null) // проверяем не возникла ли ошибка
            {
                updatesList.Assets.objects = new Dictionary<string, Assets.AssetFile>();

                foreach (string asset in asstes.objects.Keys)
                {
                    string assetHash = asstes.objects[asset].hash;
                    if (assetHash != null)
                    {
                        // проверяем существует ли файл. Если нет - отправляем на обновление
                        string assetPath = "/" + assetHash.Substring(0, 2);
                        if (!File.Exists(DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
                        {
                            updatesList.Assets.objects[asset] = asstes.objects[asset];
                            updatesList.UpdatesCount++;
                        }
                    }
                    else
                    {
                        // С этим файлом возникла ошибка. Добавляем его в список на обновление. Метод обновления законет его в список ошибок
                        updatesList.Assets.objects[asset] = asstes.objects[asset];
                        updatesList.UpdatesCount++;
                    }
                }
            }
            else
            {
                updatesList.Assets.objects = null;
            }

            return updatesList;
        }

        //функция для удаления файла при его существовании 
        private static void DelFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch { }
        }

        //функция для скачивания файлов клиента в zip формате, без проверки хеша
        private static bool UnsafeDownloadZip(string url, string to, string file, string temp, WebClient wc)
        {
            //создаем папки в соответсвии с путем к файлу из списка
            string[] foldersPath = (to + file).Replace(DirectoryPath, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string path = DirectoryPath;
            // TODO: какой-то ебанутый метод создания директорий
            for (int i = 0; i < foldersPath.Length - 1; i++)
            {
                path += "/" + foldersPath[i];
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            string zipFile = file + ".zip";

            try
            {
                wc.DownloadFile(url + ".zip", temp + zipFile);

                DelFile(to + zipFile); // TODO: не ебу че это. Возможно убрать
                ZipFile.ExtractToDirectory(temp + zipFile, temp);
                File.Delete(temp + zipFile);

                DelFile(to + file);
                File.Move(temp + file, to + file);

                return true;
            }
            catch
            {
                DelFile(temp + file);
                DelFile(temp + zipFile);

                return false;
            }

        }

        //функция для скачивания файлов клиента в zip формате, со сравнением хеша
        private static bool SaveDownloadZip(string url, string file, string to, string temp, string sha1, int size, WebClient wc)
        {
            string zipFile = file + ".zip";

            //создаем папки в соответсвии с путем к файлу из списка
            string[] foldersPath = to.Replace(DirectoryPath, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // TODO: это всё можно одной функцией заменить

            string path = DirectoryPath;
            for (int i = 0; i < foldersPath.Length - 1; i++)
            {
                path += "/" + foldersPath[i];
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            try
            {
                wc.DownloadFile(url + ".zip", temp + file + ".zip");

                DelFile(to + ".zip");
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
                            DelFile(to);
                            File.Move(temp + file, to);

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
            catch
            {
                DelFile(temp + file);
                DelFile(temp + zipFile);

                return false;
            }
        }

        //функция для скачивания файлов в jar формате, без сравнения хэша
        private static bool UnsafeDownloadJar(string url, string to, string file, WebClient wc, string temp)
        {
            // TODO: сделать нормальное создание входящих директорий
            //создаем папки в соответсвии с путем к файлу из списка
            string[] foldersPath = (to + file).Replace(DirectoryPath, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string path = DirectoryPath;
            for (int i = 0; i < foldersPath.Length - 1; i++)
            {
                path += "/" + foldersPath[i];
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            try
            {
                // TODO: возможно не удалять старый файл, а скачивать только в случае, если старый файл отличается
                wc.DownloadFile(url, temp + file);
                DelFile(to + file);
                File.Move(temp + file, to + file);

                return true;
            }
            catch
            {
                DelFile(temp + file);
                return false;
            }

        }

        //функция для скачивания файлов в jar формате, со сравнением хэша
        private static bool SaveDownloadJar(string url, string file, string to, string temp, string sha1, int size, WebClient wc)
        {
            //создаем папки в соответсвии с путем к файлу из списка
            string[] foldersPath = to.Replace(DirectoryPath, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // TODO: это всё можно одной функцией заменить

            string path = DirectoryPath;
            for (int i = 0; i < foldersPath.Length - 1; i++)
            {
                path += "/" + foldersPath[i];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);

                }
            }

            try
            {
                wc.DownloadFile(url, temp + file);
                DelFile(to);

                using (FileStream fstream = new FileStream(temp + file, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    using (SHA1 sha = new SHA1Managed())
                    {
                        if (fileBytes.Length == size) //Convert.ToBase64String(sha.ComputeHash(fileBytes)) == sha1 &&
                        {
                            DelFile(to);
                            File.Move(temp + file, to);

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
            catch
            {
                DelFile(temp + file);
                return false;
            }
        }

        public static List<string> UpdateBaseFiles(BaseFilesUpdates updateList, VersionManifest filesList, string instanceId, ref Dictionary<string, int> updates)
        {
            string addr;
            string[] folders;
            int updatesCount = 0;

            List<string> errors = new List<string>();
            WebClient wc = new WebClient();

            string temp = CreateTempDir();

            //скачивание файла версии
            if (updateList.MinecraftJar)
            {
                Objects.FileInfo minecraftJar = filesList.version.minecraftJar;
                if (minecraftJar.url == null)
                {
                    addr = LaunсherSettings.URL.Upload + "versions/" + minecraftJar.name;
                }
                else
                {
                    addr = minecraftJar.url;
                }

                bool isDownload;
                if (minecraftJar.notArchived)
                {
                    isDownload = SaveDownloadJar(addr, minecraftJar.name, DirectoryPath + "/instances/" + instanceId + "/version/" + minecraftJar.name, temp, minecraftJar.sha1, minecraftJar.size, wc);
                }
                else
                {
                    isDownload = SaveDownloadZip(addr, minecraftJar.name, DirectoryPath + "/instances/" + instanceId + "/version/" + minecraftJar.name, temp, minecraftJar.sha1, minecraftJar.size, wc);
                }

                if (isDownload)
                {
                    updates["version"] = minecraftJar.lastUpdate;
                }
                else
                {
                    errors.Add("version/" + minecraftJar.name);
                }

                updatesCount++;
                updateList.ProcentUpdateFunc(updateList.UpdatesCount, updatesCount);

            }

            //скачиваем libraries
            folders = null;
            List<string> executedMethods = new List<string>();
            string downloadedLibsAddr = DirectoryPath + "/versions/libraries/" + GetLibName(instanceId, filesList.version) + "-downloaded.json"; // адрес файла в котором убдет храниться список downloadedLibs
            // TODO: список downloadedLibs мы получаем в методе проверки. брать от туда, а не подгружать опять
            List<string> downloadedLibs = GetFile<List<string>>(downloadedLibsAddr); // сюда мы пихаем файлы, которые удачно скачались. При каждом удачном скачивании сохраняем список в файл. Если все файлы скачались удачно - удаляем этот список
            if (downloadedLibs == null) downloadedLibs = new List<string>();
            int startDownloadedLibsCount = downloadedLibs.Count;

            if (updateList.Libraries.Count > 0) //сохраняем версию либририесов если в списке на обновление(updateList.Libraries) есть хотя бы один либрариес
            {
                SaveFile(DirectoryPath + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesList.version) + ".lver", filesList.version.librariesLastUpdate.ToString());
            }

            string tempDir = CreateTempDir();
            foreach (string lib in updateList.Libraries.Keys)
            {
                if (updateList.Libraries[lib].obtainingMethod == null)
                {
                    if (updateList.Libraries[lib].url == null)
                    {
                        addr = LaunсherSettings.URL.Upload + "libraries/";
                    }
                    else
                    {
                        addr = updateList.Libraries[lib].url;
                    }

                    folders = lib.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string ff = lib.Replace(folders[folders.Length - 1], "");

                    if (addr.Length > 5 && addr.Substring(addr.Length - 4) != ".jar" && addr.Substring(addr.Length - 4) != ".zip")
                    {
                        addr = addr + lib;
                    }

                    bool isDownload;
                    string name = folders[folders.Length - 1];
                    string fileDir = DirectoryPath + "/libraries/" + ff;
                    if (updateList.Libraries[lib].notArchived)
                    {
                        isDownload = UnsafeDownloadJar(addr, fileDir, name, wc, tempDir);
                    }
                    else
                    {
                        isDownload = UnsafeDownloadZip(addr, fileDir, name, tempDir, wc);
                    }

                    if (updateList.Libraries[lib].isNative)
                    {
                        //try
                        {
                            string tempFolder = CreateTempDir();
                            // извлекаем во временную папку
                            ZipFile.ExtractToDirectory(fileDir + "/" + name, tempFolder);

                            if(!Directory.Exists(DirectoryPath + "/natives/" + filesList.version.gameVersion + "/"))
                            {
                                Directory.CreateDirectory(DirectoryPath + "/natives/" + filesList.version.gameVersion + "/");
                            }

                            //Скопировать все файлы. И перезаписать(если такие существуют)
                            foreach (string newPath in Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories))
                            {
                                if (!newPath.Contains("META-INF"))
                                {
                                    File.Copy(newPath, newPath.Replace(tempFolder, DirectoryPath + "/natives/" + filesList.version.gameVersion + "/"), true);
                                }
                            }

                            Directory.Delete(tempFolder, true); // TODO: тут выползало исключение папка не пуста
                        }
                        /*
                        catch
                        {
                            isDownload = false;
                        }*/
                    }

                    if (isDownload)
                    {
                        downloadedLibs.Add(lib);
                        SaveFile(downloadedLibsAddr, JsonConvert.SerializeObject(downloadedLibs));
                        Console.WriteLine("SAVE DOWNLOADED");
                    }
                    else
                    {
                        errors.Add("libraries/" + lib);
                        DelFile(DirectoryPath + "/libraries/" + lib);
                    }


                    updatesCount++;
                    updateList.ProcentUpdateFunc(updateList.UpdatesCount, updatesCount);
                }
                else
                {
                    try
                    {
                        List<List<string>> obtainingMethod = updateList.Libraries[lib].obtainingMethod; // получаем метод

                        if (!executedMethods.Contains(obtainingMethod[0][0])) //проверяем был ли этот метод уже выполнен
                        {
                            int i = 1; //начинаем цикл с первого элемента, т.к нулевой - название метода
                            while (i < obtainingMethod.Count)
                            {
                                // получаем команду и выполняем её
                                switch (obtainingMethod[i][0])
                                {
                                    case "downloadFile":
                                        Console.WriteLine("download " + obtainingMethod[i][1]);
                                        if (!DownloadFile(obtainingMethod[i][1], obtainingMethod[i][2], tempDir))
                                        {
                                            goto EndWhile; //возникла ошибка
                                        }
                                        break;
                                    case "unzipFile":
                                        ZipFile.ExtractToDirectory(tempDir + obtainingMethod[i][1], tempDir + obtainingMethod[i][2]);
                                        break;
                                    case "startProcess":
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
                                        command = command.Replace("{DIR}", DirectoryPath);
                                        command = command.Replace("{TEMP_DIR}", tempDir);
                                        command = command.Replace("{MINECRAFT_JAR}", DirectoryPath + "/instances/" + instanceId + "/version/" + filesList.version.minecraftJar.name);
                                        Console.WriteLine();
                                        Console.WriteLine(command);

                                        if (!Utils.StartProcess(command, executord))
                                        {
                                            errors.Add("libraries/" + lib);
                                            goto EndWhile; //возникла ошибка
                                        }

                                        break;

                                    case "moveFile":
                                        string to = obtainingMethod[i][2].Replace("{DIR}", DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
                                        string from = obtainingMethod[i][1].Replace("{DIR}", DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
                                        if (File.Exists(to))
                                        {
                                            File.Delete(to);
                                        }
                                        if (!Directory.Exists(to.Replace(Path.GetFileName(to), "")))
                                        {
                                            Directory.CreateDirectory(to.Replace(Path.GetFileName(to), ""));
                                        }
                                        File.Move(from, to);
                                        break;
                                }
                                i++;
                            }
                        }

                        //теперь добавляем этот метод в уже выполненные и если не существует файла, который мы должны получить - значит произошла ошибка
                        EndWhile: executedMethods.Add(obtainingMethod[0][0]);
                        if (!File.Exists(DirectoryPath + "/libraries/" + lib))
                        {
                            Console.WriteLine(DirectoryPath + "/libraries/" + lib);
                            errors.Add("libraries/" + lib);
                        }
                        else
                        {
                            downloadedLibs.Add(lib);
                            SaveFile(downloadedLibsAddr, JsonConvert.SerializeObject(downloadedLibs));
                        }
                    }
                    catch
                    {
                        errors.Add("libraries/" + lib);
                    }

                    updatesCount++;
                    updateList.ProcentUpdateFunc(updateList.UpdatesCount, updatesCount);
                }
            }

            Directory.Delete(tempDir, true);
            Directory.Delete(temp, true);

            if (downloadedLibs.Count - startDownloadedLibsCount == updateList.Libraries.Count)
            {
                //все либрариесы скачались удачно. Удаляем файл
                DelFile(downloadedLibsAddr);
            }

            //скачиваем assets

            // скачиваем файлы objects
            if (updateList.Assets.objects != null)
            {
                foreach (string asset in updateList.Assets.objects.Keys)
                {
                    string assetHash = updateList.Assets.objects[asset].hash;
                    if (assetHash != null)
                    {
                        string assetPath = "/" + assetHash.Substring(0, 2);
                        if (!File.Exists(DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
                        {
                            if (!InstallFile("http://resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath))
                            {
                                errors.Add("asstes: " + asset);
                            }

                            updatesCount++;
                            updateList.ProcentUpdateFunc(updateList.UpdatesCount, updatesCount);
                        }
                    }
                    else
                    {
                        errors.Add("asstes: " + asset);
                    }

                }
            }
            else
            {
                errors.Add("asstes/objects");
            }

            //скачиваем json файл
            if (updateList.AssetsIndexes)
            {
                if (!File.Exists(DirectoryPath + "/assets/indexes/" + filesList.version.assetsVersion + ".json"))
                {
                    if (!Directory.Exists(DirectoryPath + "/assets/indexes"))
                        Directory.CreateDirectory(DirectoryPath + "/assets/indexes");

                    try
                    {
                        wc.DownloadFile(filesList.version.assetsIndexes, DirectoryPath + "/assets/indexes/" + filesList.version.assetsVersion + ".json"); // TODO: заюзать мою функцию для скачивания
                    }
                    catch { }
                }
            }

            wc.Dispose();

            //сохраняем lastUpdates
            SaveFile(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

            return errors;
        }

        public static ExportResult ExportInstance(string instanceId, List<string> directoryList, string exportFile, string description)
        {
            // TODO: удалять временную папку в конце
            string targetDir = CreateTempDir() + instanceId + "-export"; //временная папка, куда будем копировать все файлы
            string srcDir = DirectoryPath + "/instances/" + instanceId;

            try
            {
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }
            }
            catch
            {
                return ExportResult.TempPathError;
            }

            foreach (string dirUnit_ in directoryList)
            {
                string dirUnit = dirUnit_.Replace(@"\", "/"); //адрес исходного файла
                string target = dirUnit.Replace(srcDir, targetDir + "/files"); //адрес этого файла во временной папке
                string finalPath = target.Substring(0, target.LastIndexOf("/")); //адрес временной папки, где будет храниться этот файл

                try
                {
                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                }
                catch
                {
                    return ExportResult.TempPathError;
                }

                if (File.Exists(dirUnit))
                {
                    try
                    {
                        if (File.Exists(target))
                        {
                            File.Delete(target);
                        }

                        File.Copy(dirUnit, target);
                    }
                    catch
                    {
                        return ExportResult.FileCopyError;
                    }
                }
                else
                {
                    return ExportResult.FileCopyError;
                }
            }

            VersionManifest instanceFile = GetManifest(instanceId, false);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["gameVersion"] = instanceFile.version.gameVersion,
                ["description"] = description,
                ["name"] = UserData.Instances.Record[instanceId].Name,
                ["author"] = UserData.Login,
                ["modloaderType"] = instanceFile.version.modloaderType.ToString(),
                ["modloaderVersion"] = instanceFile.version.modloaderVersion,
            };


            string jsonData = JsonConvert.SerializeObject(data);
            if (!SaveFile(targetDir + "/instanceInfo.json", jsonData))
            {
                try
                {
                    Directory.Delete(targetDir, true);
                }
                catch { }

                return ExportResult.InfoFileError;
            }

            try
            {
                ZipFile.CreateFromDirectory(targetDir, exportFile);
                Directory.Delete(targetDir, true);

                return ExportResult.Successful;
            }
            catch
            {
                Directory.Delete(targetDir, true);

                return ExportResult.ZipFileError;
            }
        }

        public static ImportResult ImportInstance(string zipFile, out List<string> errors, out string instance_Id)
        {
            instance_Id = null;

            string dir = CreateTempDir() + "import/";
            errors = new List<string>();

            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch
                {
                    return ImportResult.DirectoryCreateError;
                }
            }
            else
            {
                Directory.Delete(dir, true);
            }

            try
            {
                ZipFile.ExtractToDirectory(zipFile, dir);
            }
            catch
            {
                Directory.Delete(dir, true);

                return ImportResult.ZipFileError;
            }

            Dictionary<string, string> instanceInfo = GetFile<Dictionary<string, string>>(dir + "instanceInfo.json");
            ModloaderType modloader = ModloaderType.None;

            if (instanceInfo == null || !instanceInfo.ContainsKey("gameVersion") || string.IsNullOrEmpty(instanceInfo["gameVersion"]))
            {
                Directory.Delete(dir, true);
                return ImportResult.GameVersionError;
            }

            if (!instanceInfo.ContainsKey("name") || string.IsNullOrEmpty(instanceInfo["name"]))
            {
                instanceInfo["name"] = "Unknown Name";
            }

            if (!instanceInfo.ContainsKey("author") || string.IsNullOrEmpty(instanceInfo["author"]))
            {
                instanceInfo["author"] = "Unknown author";
            }

            if (!instanceInfo.ContainsKey("description") || string.IsNullOrEmpty(instanceInfo["description"]))
            {
                instanceInfo["description"] = "";
            }

            if (!instanceInfo.ContainsKey("modloaderVersion") || string.IsNullOrEmpty(instanceInfo["modloaderVersion"]))
            {
                instanceInfo["modloaderVersion"] = "";
            }

            if (!instanceInfo.ContainsKey("modloaderType") || string.IsNullOrEmpty(instanceInfo["modloaderType"]))
            {
                instanceInfo["modloaderType"] = "";
            }

            Enum.TryParse(instanceInfo["modloaderType"], out modloader);


            string instanceId = ManageLogic.CreateInstance(instanceInfo["name"], InstanceSource.Local, instanceInfo["gameVersion"], modloader, instanceInfo["modloaderVersion"]);
            instance_Id = instanceId;
            MessageBox.Show(instanceId);

            string addr = dir + "files/";
            string targetDir = DirectoryPath + "/instances/" + instanceId + "/";

            try
            {
                IEnumerable<string> allFiles = Directory.EnumerateFiles(addr, "*", SearchOption.AllDirectories);
                foreach (string fileName in allFiles)
                {
                    string targetFileName = fileName.Replace(addr, targetDir);
                    string dirName = Path.GetDirectoryName(targetFileName);

                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    File.Copy(fileName, targetFileName);
                }
            }
            catch
            {
                Directory.Delete(dir, true);

                return ImportResult.MovingFilesError;
            }

            try
            {
                Directory.Delete(dir, true);
            }
            catch { }


            return ImportResult.Successful;
        }

        public static void RemoveInstanceDirecory(string instanceId)
        {
            try
            {
                if (Directory.Exists(DirectoryPath + "/instances/" + instanceId))
                {
                    Directory.Delete(DirectoryPath + "/instances/" + instanceId, true);
                }
            }
            catch
            {
                MainWindow.Obj.Dispatcher.Invoke(delegate
                {
                    MainWindow.Obj.SetMessageBox("Произошла ошибка при удалении.");
                });
            }

            MainWindow.Obj.Dispatcher.Invoke(delegate
            {
                //MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
            });
        }
    }
}
