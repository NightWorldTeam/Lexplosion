using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Network;
using System.IO.Compression;
using System.Net;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    class InstanceInstaller
    {
        protected string instanceId;

        public InstanceInstaller(string instanceID)
        {
            instanceId = instanceID;
        }

        public struct Assets
        {
            public struct AssetFile
            {
                public string hash;
            }

            public Dictionary<string, AssetFile> objects;
        }

        public delegate void ProcentUpdate(int totalDataCount, int nowDataCount);
        public event ProcentUpdate ProcentUpdateEvent;

        private Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
        private bool minecraftJar = false;
        private bool assetsIndexes = false;
        private Assets assets;

        int updatesCount = 0;

        /// <summary>
        /// Проверяет основные файла клиента, недостающие файлы помещает во внуренний список на скачивание
        /// </summary>
        /// <returns>
        /// Возвращает количество файлов, которые нужно обновить. -1 в случае неудачи (возможно только если включена защита целосности клиента). 
        /// </returns>
        // TODO: его вызов обернуть в try
        public int CheckBaseFiles(VersionManifest filesInfo, ref LastUpdates updates) // функция проверяет основные файлы клиента (файл версии, либрариесы и тп)
        {
            //проверяем файл версии
            Console.WriteLine(DirectoryPath + "/instances/" + instanceId + "/version");
            if (!Directory.Exists(DirectoryPath + "/instances/" + instanceId + "/version"))
            {
                Directory.CreateDirectory(DirectoryPath + "/instances/" + instanceId + "/version"); //создаем папку versions если её нет
                minecraftJar = true; //сразу же добавляем minecraftJar в обновления
                updatesCount++;
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
                                        minecraftJar = true;
                                        updatesCount++;
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
                    minecraftJar = true;
                    updatesCount++;
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

                        long ver = 0;
                        Int64.TryParse(Encoding.UTF8.GetString(fileBytes), out ver);
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
                    libraries[lib] = filesInfo.libraries[lib];
                    updatesCount++;
                }
            }
            else
            {
                if (filesInfo.version.librariesLastUpdate != updates["libraries"]) //если версия libraries старая, то отправляем на обновления
                {
                    foreach (string lib in filesInfo.libraries.Keys)
                    {
                        libraries[lib] = filesInfo.libraries[lib];
                        updatesCount++;
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
                            libraries[lib] = filesInfo.libraries[lib];
                            updatesCount++;
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
                        libraries[lib] = filesInfo.libraries[lib];
                        updatesCount++;
                    }
                }
            }

            // Проверяем assets

            // Пытаемся получить список всех асетсов из json файла
            Assets asstes = GetFile<Assets>(DirectoryPath + "/assets/indexes/" + filesInfo.version.assetsVersion + ".json");

            // Файла нет, или он битый. Получаем асетсы с сервера
            if (asstes.objects == null)
            {
                assetsIndexes = true; //устанавливаем флаг что нужно скачать json файл
                updatesCount++;
                Console.WriteLine("assetsIndexes ");

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
                assets.objects = new Dictionary<string, Assets.AssetFile>();

                foreach (string asset in asstes.objects.Keys)
                {
                    string assetHash = asstes.objects[asset].hash;
                    if (assetHash != null)
                    {
                        // проверяем существует ли файл. Если нет - отправляем на обновление
                        string assetPath = "/" + assetHash.Substring(0, 2);
                        if (!File.Exists(DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
                        {
                            assets.objects[asset] = asstes.objects[asset];
                            updatesCount++;
                        }
                    }
                    else
                    {
                        // С этим файлом возникла ошибка. Добавляем его в список на обновление. Метод обновления законет его в список ошибок
                        assets.objects[asset] = asstes.objects[asset];
                        updatesCount++;
                    }
                }
            }
            else
            {
                assets.objects = null;
            }

            return updatesCount;
        }

        //функция для скачивания файлов клиента в zip формате, без проверки хеша
        protected bool UnsafeDownloadZip(string url, string to, string file, string temp, WebClient wc)
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
        protected bool SaveDownloadZip(string url, string file, string to, string temp, string sha1, long size, WebClient wc)
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
        protected bool UnsafeDownloadJar(string url, string to, string file, WebClient wc, string temp)
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
        protected bool SaveDownloadJar(string url, string file, string to, string temp, string sha1, long size, WebClient wc)
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

        /// <summary>
        /// Обновляет файлы, которые метод CheckBaseFiles добавил в список
        /// </summary>
        /// <returns>
        /// Возвращает список файлов, скачивание которых закончилось ошибкой
        /// </returns>
        public List<string> UpdateBaseFiles(VersionManifest filesList, ref LastUpdates updates)
        {
            string addr;
            string[] folders;
            int updated = 0;

            List<string> errors = new List<string>();
            WebClient wc = new WebClient();

            string temp = CreateTempDir();

            //скачивание файла версии
            if (minecraftJar)
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

                updated++;
                ProcentUpdateEvent?.Invoke(updatesCount, updated);

            }

            //скачиваем libraries
            folders = null;
            List<string> executedMethods = new List<string>();
            string downloadedLibsAddr = DirectoryPath + "/versions/libraries/" + GetLibName(instanceId, filesList.version) + "-downloaded.json"; // адрес файла в котором убдет храниться список downloadedLibs
            // TODO: список downloadedLibs мы получаем в методе проверки. брать от туда, а не подгружать опять
            List<string> downloadedLibs = GetFile<List<string>>(downloadedLibsAddr); // сюда мы пихаем файлы, которые удачно скачались. При каждом удачном скачивании сохраняем список в файл. Если все файлы скачались удачно - удаляем этот список
            if (downloadedLibs == null) downloadedLibs = new List<string>();
            int startDownloadedLibsCount = downloadedLibs.Count;

            if (libraries.Count > 0) //сохраняем версию либририесов если в списке на обновление(updateList.Libraries) есть хотя бы один либрариес
            {
                SaveFile(DirectoryPath + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesList.version) + ".lver", filesList.version.librariesLastUpdate.ToString());
            }

            string tempDir = CreateTempDir();
            foreach (string lib in libraries.Keys)
            {
                if (libraries[lib].obtainingMethod == null)
                {
                    if (libraries[lib].url == null)
                    {
                        addr = LaunсherSettings.URL.Upload + "libraries/";
                    }
                    else
                    {
                        addr = libraries[lib].url;
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
                    if (libraries[lib].notArchived)
                    {
                        isDownload = UnsafeDownloadJar(addr, fileDir, name, wc, tempDir);
                    }
                    else
                    {
                        isDownload = UnsafeDownloadZip(addr, fileDir, name, tempDir, wc);
                    }

                    if (libraries[lib].isNative)
                    {
                        //try
                        {
                            string tempFolder = CreateTempDir();
                            // извлекаем во временную папку
                            ZipFile.ExtractToDirectory(fileDir + "/" + name, tempFolder);

                            if (!Directory.Exists(DirectoryPath + "/natives/" + filesList.version.gameVersion + "/"))
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

                    updated++;
                    ProcentUpdateEvent?.Invoke(updatesCount, updated);
                }
                else
                {
                    try
                    {
                        List<List<string>> obtainingMethod = libraries[lib].obtainingMethod; // получаем метод

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

                    updated++;
                    ProcentUpdateEvent?.Invoke(updatesCount, updated);
                }
            }

            Directory.Delete(tempDir, true);
            Directory.Delete(temp, true);

            if (downloadedLibs.Count - startDownloadedLibsCount == libraries.Count)
            {
                //все либрариесы скачались удачно. Удаляем файл
                DelFile(downloadedLibsAddr);
            }

            //скачиваем assets

            // скачиваем файлы objects
            if (assets.objects != null)
            {
                foreach (string asset in assets.objects.Keys)
                {
                    string assetHash = assets.objects[asset].hash;
                    if (assetHash != null)
                    {
                        string assetPath = "/" + assetHash.Substring(0, 2);
                        if (!File.Exists(DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
                        {
                            if (!InstallFile("http://resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath))
                            {
                                errors.Add("asstes: " + asset);
                            }

                            updated++;
                            ProcentUpdateEvent?.Invoke(updatesCount, updated);
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
            if (assetsIndexes)
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
    }
}
