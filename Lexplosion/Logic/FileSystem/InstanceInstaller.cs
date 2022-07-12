using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.IO.Compression;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Network;
using static Lexplosion.Logic.FileSystem.WithDirectory;
using static Lexplosion.Logic.FileSystem.DataFilesManager;
using Lexplosion.Tools;

namespace Lexplosion.Logic.FileSystem
{
    class InstanceInstaller
    {
        private static KeySemaphore<string> _librariesBlock = new KeySemaphore<string>();
        private static KeySemaphore<string> _assetsBlock = new KeySemaphore<string>();

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
        public event ProcentUpdate BaseDownloadEvent;

        private Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
        private bool minecraftJar = false;
        private bool assetsIndexes = false;
        private Assets assets;

        private int updatesCount = 0;

        /// <summary>
        /// Проверяет основные файла клиента, недостающие файлы помещает во внуренний список на скачивание
        /// </summary>
        /// <returns>
        /// Возвращает количество файлов, которые нужно обновить. -1 в случае неудачи (возможно только если включена защита целосности клиента). 
        /// </returns>
        // TODO: его вызов обернуть в try
        public int CheckBaseFiles(in VersionManifest manifest, ref LastUpdates updates) // функция проверяет основные файлы клиента (файл версии, либрариесы и тп)
        {
            string gameVersionName = manifest.version.CustomVersionName ?? manifest.version.gameVersion;

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
                string minecraftJarFile = DirectoryPath + "/instances/" + instanceId + "/version/" + manifest.version.minecraftJar.name;
                if (updates.ContainsKey("version") && File.Exists(minecraftJarFile) && manifest.version.minecraftJar.lastUpdate == updates["version"]) //проверяем его наличие и версию
                {
                    if (manifest.version.security) //если включена защита файла версии, то проверяем его 
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
                                    if (Convert.ToBase64String(sha.ComputeHash(bytes)) != manifest.version.minecraftJar.sha1 || bytes.Length != manifest.version.minecraftJar.size)
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
            string libName = manifest.version.GetLibName;
            if (File.Exists(DirectoryPath + "/versions/libraries/lastUpdates/" + libName + ".lver"))
            {
                try
                {
                    using (FileStream fstream = new FileStream(DirectoryPath + "/versions/libraries/lastUpdates/" + libName + ".lver", FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл с версией libraries
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
                SaveFile(DirectoryPath + "/versions/libraries/lastUpdates/" + libName + ".lver", "0");
                updates["libraries"] = 0;
            }

            //проверяем папку libraries
            if (!Directory.Exists(DirectoryPath + "/libraries"))
            {
                foreach (string lib in manifest.libraries.Keys)
                {
                    libraries[lib] = manifest.libraries[lib];
                    updatesCount++;
                }
            }
            else
            {
                if (manifest.version.librariesLastUpdate != updates["libraries"]) //если версия libraries старая, то отправляем на обновления
                {
                    foreach (string lib in manifest.libraries.Keys)
                    {
                        libraries[lib] = manifest.libraries[lib];
                        updatesCount++;
                    }
                }
                else
                {
                    // получем файл, в ктором хранятси список либрариесов, которые удачно скачались в прошлый раз
                    List<string> downloadedFiles = new List<string>();
                    string downloadedInfoAddr = DirectoryPath + "/versions/libraries/" + libName + "-downloaded.json";
                    bool fileExided = false;
                    if (File.Exists(downloadedInfoAddr))
                    {
                        downloadedFiles = GetFile<List<string>>(downloadedInfoAddr);
                        fileExided = true;
                    }

                    //ищем недостающие файлы
                    foreach (string lib in manifest.libraries.Keys)
                    {
                        if ((downloadedFiles == null && fileExided) || !File.Exists(DirectoryPath + "/libraries/" + lib) || (fileExided && downloadedFiles != null && !downloadedFiles.Contains(lib)))
                        {
                            libraries[lib] = manifest.libraries[lib];
                            updatesCount++;
                        }
                    }
                }
            }

            if (!Directory.Exists(DirectoryPath + "/natives/" + gameVersionName))
            {
                foreach (string lib in manifest.libraries.Keys)
                {
                    if (manifest.libraries[lib].isNative)
                    {
                        libraries[lib] = manifest.libraries[lib];
                        updatesCount++;
                    }
                }
            }

            // Проверяем assets

            // Пытаемся получить список всех асетсов из json файла
            Assets asstes = GetFile<Assets>(DirectoryPath + "/assets/indexes/" + manifest.version.assetsVersion + ".json");

            // Файла нет, или он битый. Получаем асетсы с сервера
            if (asstes.objects == null)
            {
                assetsIndexes = true; //устанавливаем флаг что нужно скачать json файл
                updatesCount++;
                Console.WriteLine("assetsIndexes ");

                if (!File.Exists(DirectoryPath + "/assets/indexes/" + manifest.version.assetsVersion + ".json"))
                {
                    try
                    {
                        // Получем асетсы с сервера
                        asstes = JsonConvert.DeserializeObject<Assets>(ToServer.HttpGet(manifest.version.assetsIndexes));
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

            //try
            {
                DelFile(temp + zipFile);
                wc.DownloadFile(url + ".zip", temp + zipFile);

                ZipFile.ExtractToDirectory(temp + zipFile, temp);
                File.Delete(temp + zipFile);

                DelFile(to + file);
                File.Move(temp + file, to + file);

                return true;
            }
            //catch
            //{
            //    DelFile(temp + file);
            //    DelFile(temp + zipFile);

            //    return false;
            //}

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

            //try
            {
                // TODO: возможно не удалять старый файл, а скачивать только в случае, если старый файл отличается
                wc.DownloadFile(url, temp + file);
                DelFile(to + file);
                File.Move(temp + file, to + file);

                return true;
            }
            /*catch
            {
                DelFile(temp + file);
                return false;
            }*/
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
        public List<string> UpdateBaseFiles(in VersionManifest manifest, ref LastUpdates updates, string javaPath)
        {
            string gameVersionName = manifest.version.CustomVersionName ?? manifest.version.gameVersion;

            string addr;
            string[] folders;
            int updated = 0;

            List<string> errors = new List<string>();
            WebClient wc = new WebClient();

            string temp = CreateTempDir();

            //скачивание файла версии
            if (minecraftJar)
            {
                Objects.CommonClientData.FileInfo minecraftJar = manifest.version.minecraftJar;
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
                BaseDownloadEvent?.Invoke(updatesCount, updated);

            }

            //скачиваем libraries
            _librariesBlock.WaitOne(gameVersionName);
            string libName = manifest.version.GetLibName;

            folders = null;
            List<string> executedMethods = new List<string>();
            string downloadedLibsAddr = DirectoryPath + "/versions/libraries/" + libName + "-downloaded.json"; // адрес файла в котором убдет храниться список downloadedLibs
            // TODO: список downloadedLibs мы получаем в методе проверки. брать от туда, а не подгружать опять
            List<string> downloadedLibs = GetFile<List<string>>(downloadedLibsAddr); // сюда мы пихаем файлы, которые удачно скачались. При каждом удачном скачивании сохраняем список в файл. Если все файлы скачались удачно - удаляем этот список
            if (downloadedLibs == null) downloadedLibs = new List<string>();
            int startDownloadedLibsCount = downloadedLibs.Count;

            if (libraries.Count > 0) //сохраняем версию либририесов если в списке на обновление(updateList.Libraries) есть хотя бы один либрариес
            {
                SaveFile(DirectoryPath + "/versions/libraries/lastUpdates/" + libName + ".lver", manifest.version.librariesLastUpdate.ToString());
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

                    if (libraries[lib].isNative && isDownload)
                    {
                        //try
                        {
                            string tempFolder = CreateTempDir();
                            // извлекаем во временную папку
                            ZipFile.ExtractToDirectory(fileDir + "/" + name, tempFolder);

                            if (!Directory.Exists(DirectoryPath + "/natives/" + gameVersionName + "/"))
                            {
                                Directory.CreateDirectory(DirectoryPath + "/natives/" + gameVersionName + "/");
                            }

                            //Скопировать все файлы. И перезаписать(если такие существуют)
                            foreach (string newPath in Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories))
                            {
                                if (!newPath.Contains("META-INF"))
                                {
                                    File.Copy(newPath, newPath.Replace(tempFolder, DirectoryPath + "/natives/" + gameVersionName + "/"), true);
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
                    }
                    else
                    {
                        errors.Add("libraries/" + lib);
                        DelFile(DirectoryPath + "/libraries/" + lib);
                    }

                    updated++;
                    BaseDownloadEvent?.Invoke(updatesCount, updated);
                }
                else
                {
                    //try
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
                                        command = command.Replace("{MINECRAFT_JAR}", DirectoryPath + "/instances/" + instanceId + "/version/" + manifest.version.minecraftJar.name);
                                        Console.WriteLine();
                                        Console.WriteLine(command);

                                        if (!Utils.StartProcess(command, executord, javaPath))
                                        {
                                            errors.Add("libraries/" + lib);
                                            goto EndWhile; //возникла ошибка
                                        }

                                        break;

                                    case "moveFile":
                                        {
                                            string from = obtainingMethod[i][1].Replace("{DIR}", DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
                                            string to = obtainingMethod[i][2].Replace("{DIR}", DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
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
                                            string from = obtainingMethod[i][1].Replace("{MINECRAFT_JAR}", DirectoryPath + "/instances/" + instanceId + "/version/" + manifest.version.minecraftJar.name).Replace("//", "/");
                                            string to = obtainingMethod[i][2].Replace("{DIR}", DirectoryPath).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
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
                    /*catch
                    {
                        errors.Add("libraries/" + lib);
                    }*/

                    updated++;
                    BaseDownloadEvent?.Invoke(updatesCount, updated);
                }
            }

            try
            {
                Directory.Delete(tempDir, true);
            }
            catch { }
            try
            {
                Directory.Delete(temp, true);
            }
            catch { }

            if (downloadedLibs.Count - startDownloadedLibsCount == libraries.Count)
            {
                //все либрариесы скачались удачно. Удаляем файл
                DelFile(downloadedLibsAddr);
            }

            _librariesBlock.Release(gameVersionName);

            //скачиваем assets
            _assetsBlock.WaitOne(gameVersionName);

            // скачиваем файлы objects
            if (assets.objects != null)
            {
                TasksPerfomer perfomer = null;
                if (assets.objects.Count > 0)
                    perfomer = new TasksPerfomer(15, assets.objects.Count);

                foreach (string asset in assets.objects.Keys)
                {
                    perfomer.ExecuteTask(delegate ()
                    {
                        string assetHash = assets.objects[asset].hash;
                        if (assetHash != null)
                        {
                            string assetPath = "/" + assetHash.Substring(0, 2);
                            if (!File.Exists(DirectoryPath + "/assets/objects/" + assetPath + "/" + assetHash))
                            {
                                bool flag = false;
                                for (int i = 0; i < 3; i++) // 3 попытки делаем
                                {
                                    if (InstallFile("http://resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }

                                if (!flag)
                                {
                                    errors.Add("asstes: " + asset);
                                }


                                updated++;
                                BaseDownloadEvent?.Invoke(updatesCount, updated);
                            }
                        }
                        else
                        {
                            errors.Add("asstes: " + asset);
                        }
                    });
                }

                perfomer?.WaitEnd();
            }
            else
            {
                errors.Add("asstes/objects");
            }

            //скачиваем json файл
            if (assetsIndexes)
            {
                if (!File.Exists(DirectoryPath + "/assets/indexes/" + manifest.version.assetsVersion + ".json"))
                {
                    if (!Directory.Exists(DirectoryPath + "/assets/indexes"))
                        Directory.CreateDirectory(DirectoryPath + "/assets/indexes");

                    try
                    {
                        wc.DownloadFile(manifest.version.assetsIndexes, DirectoryPath + "/assets/indexes/" + manifest.version.assetsVersion + ".json"); // TODO: заюзать мою функцию для скачивания
                    }
                    catch { }
                }
            }

            _assetsBlock.Release(gameVersionName);

            wc.Dispose();

            //сохраняем lastUpdates
            SaveFile(DirectoryPath + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

            return errors;
        }
    }
}
