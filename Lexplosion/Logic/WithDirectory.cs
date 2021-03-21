using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.IO.Compression;
using Newtonsoft.Json;
using Lexplosion.Objects;
using System.Threading;
using Lexplosion.Gui.Windows;
using System.Runtime.CompilerServices;

namespace Lexplosion.Logic
{
    static class WithDirectory
    {
        public static string directory;
        public static int countFiles;

        private static class Updates //класс, хранящий всё, что нужно обновить. Метод Check в него кладет, а метод Update - достает 
        {
            static public Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
            static public List<string> natives = new List<string>();
            static public List<string> libraries = new List<string>();
            static public bool minecraftJar = false;
            static public bool assetsObjects = false;
            static public bool assetsIndexes = false;
            static public bool assetsVirtual = false;
            static public List<string> oldFiles = new List<string>();
        }

        private class LauncherAssets //этот класс нужен для декодирования json
        {
            public int version = 0;
            public Dictionary<string, InstanceAssets> data;

        }

        public static void Create(string path)
        {
            try
            {
                directory = path.Replace(@"\", "/");

                if (!Directory.Exists(directory))
                {
                    string[] folders = directory.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string ff = folders[0];
                    for (int i = 1; i < folders.Length - 1; i++)
                    {
                        if (!Directory.Exists(ff))
                            Directory.CreateDirectory(ff);

                        ff += "/" + folders[i];
                    }

                    Directory.CreateDirectory(directory + "/temp");

                } else if (!Directory.Exists(directory + "/temp")) { 
                    Directory.CreateDirectory(directory + "/temp");
                }

            } catch {}

        }

        public static bool Check(InstanceFiles filesInfo, string instanceId)
        {
            Dictionary<string, int> updates = new Dictionary<string, int>();

            if (!File.Exists(directory + "/instances/" + instanceId + "/" + "lastUpdates.json"))
            {
                if (!Directory.Exists(directory + "/instances/" + instanceId))
                    Directory.CreateDirectory(directory + "/instances/" + instanceId); //создаем папку с модпаком, если её нет

                File.Create(directory + "/instances/" + instanceId + "/" + "lastUpdates.json").Close(); // создание файла со списком последних обновлений

            } else {

                try
                {
                    using (FileStream fstream = File.OpenRead(directory + "/instances/" + instanceId + "/" + "lastUpdates.json")) //открываем файл с последними обновлениями
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        try
                        {
                            if (JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes)) != null)
                                updates = JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes));

                        } catch {
                            File.Delete(directory + "/instances/" + instanceId + "/" + "lastUpdates.json");
                        }

                    }

                } catch { }
            }

            //Проходимся по списку папок(data) из класса instanceFiles
            foreach (string dir in filesInfo.data.Keys)
            {
                string folder = directory + "/instances/" + instanceId + "/" + dir;

                try
                {
                    if (!updates.ContainsKey(dir) || updates[dir] < filesInfo.data[dir].folderVersion) //проверяем версию папки. если она старая - очищаем
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(folder);
                        foreach (var file in dirInfo.GetFiles())
                            file.Delete();

                        updates[dir] = filesInfo.data[dir].folderVersion;
                    }

                    using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/" + "lastUpdates.json", FileMode.OpenOrCreate))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(updates));
                        fstream.Write(bytes, 0, bytes.Length);
                        fstream.Close();
                    }

                } catch { }

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
                                using (FileStream fstream = File.OpenRead(file)) //открываем файл на чтение
                                {
                                    byte[] bytes = new byte[fstream.Length];
                                    fstream.Read(bytes, 0, bytes.Length);
                                    fstream.Close();

                                    if (filesInfo.data[dir].objects.ContainsKey(fileName)) // проверяем есть ли этот файл в списке
                                    {
                                        SHA1 sha = new SHA1Managed();
                                        if (Convert.ToBase64String(sha.ComputeHash(bytes)) != filesInfo.data[dir].objects[fileName].sha1 || bytes.Length != filesInfo.data[dir].objects[fileName].size)
                                        {
                                            File.Delete(file); //удаляем файл, если не сходится хэш или размер

                                            if (!Updates.data.ContainsKey(dir)) //если директория отсутствует в Updates.data, то добавляем её 
                                                Updates.data.Add(dir, new List<string>());

                                            Updates.data[dir].Add(fileName); //добавляем файл в класс, который содержит обновления
                                            countFiles++;

                                        }
                                    } else {
                                        File.Delete(file);
                                    }
                                }

                            } catch {
                                //чтение одного из файлов не удалось, стопаем весь процесс
                                return false;
                            }
                        }

                        //сверяем версию файла с его версией в списке, если версия старая, то отправляем файл на обновление
                        if (filesInfo.data[dir].objects.ContainsKey(fileName))
                        {

                            if (!updates.ContainsKey(dir + "/" + fileName) || updates[dir + "/" + fileName] != filesInfo.data[dir].objects[fileName].lastUpdate)
                            {
                                if (!Updates.data.ContainsKey(dir)) //если директория отсутствует в Updates.data, то добавляем её 
                                    Updates.data.Add(dir, new List<string>());

                                if (!Updates.data[dir].Contains(fileName))
                                {
                                    Updates.data[dir].Add(fileName);
                                    countFiles++;
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

                        if (!Updates.data.ContainsKey(dir))
                            Updates.data.Add(dir, new List<string>());

                        if (!Updates.data[dir].Contains(file))
                        {
                            Updates.data[dir].Add(file);
                            countFiles++;
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
                        if (File.Exists(directory + "/instances/" + instanceId + "/" + folder + "/" + file))
                        {
                            Updates.oldFiles.Add(folder + "/" + file);
                            countFiles++;
                        }

                    } catch { }
                }
            }

            //проверяем файл версии
            if (!Directory.Exists(directory + "/instances/" + instanceId + "/version"))
            {
                Directory.CreateDirectory(directory + "/instances/" + instanceId + "/version"); //создаем папку versions если её нет
                Updates.minecraftJar = true; //сразу же добавляем minecraftJar в обновления
                countFiles++;

            } else {

                string minecraftJarFile = directory + "/instances/" + instanceId + "/version/" + filesInfo.version.minecraftJar.name;
                if (updates.ContainsKey("version") && File.Exists(minecraftJarFile) && filesInfo.version.minecraftJar.lastUpdate == updates["version"]) //проверяем его наличие и версию
                {
                    if (filesInfo.version.security) //если включена защита файла версии, то проверяем его 
                    {
                        try
                        {
                            using (FileStream fstream = File.OpenRead(minecraftJarFile))
                            {
                                byte[] bytes = new byte[fstream.Length];
                                fstream.Read(bytes, 0, bytes.Length);
                                fstream.Close();

                                SHA1 sha = new SHA1Managed();
                                if (Convert.ToBase64String(sha.ComputeHash(bytes)) != filesInfo.version.minecraftJar.sha1 || bytes.Length != filesInfo.version.minecraftJar.size)
                                {
                                    File.Delete(minecraftJarFile); //удаляем файл, если не сходится хэш или размер
                                    Updates.minecraftJar = true;
                                    countFiles++;

                                }

                            }

                        } catch {
                            return false; //чтение файла не удалось, стопаем весь процесс
                        }
                    }

                } else {
                    Updates.minecraftJar = true;
                    countFiles++;
                }
            }

            //проверяем natives
            if (!Directory.Exists(directory + "/instances/" + instanceId + "/version/natives/"))
            {
                foreach (string key in filesInfo.natives.Keys) //добавляем natives в обновления
                {
                    Updates.natives.Add(key);
                    countFiles++;
                }

            } else {

                if (!updates.ContainsKey("natives") || filesInfo.version.nativesLastUpdate != updates["natives"]) //если версия natives старая, то отправляем на обновления
                {

                    foreach (string key in filesInfo.natives.Keys)
                    {
                        if (filesInfo.natives[key] == "windows" || filesInfo.natives[key] == "all")
                        {
                            Updates.natives.Add(key);
                            countFiles++;
                        }

                    }

                } else {

                    foreach (string n in filesInfo.natives.Keys) //ищем недостающие файлы
                    {
                        if (filesInfo.natives[n] == "windows" || filesInfo.natives[n] == "all")
                        {
                            if (!File.Exists(directory + "/instances/" + instanceId + "/version/natives/" + n))
                            {
                                Updates.natives.Add(n);
                                countFiles++;
                            }

                        }
                    }
                }
            }

            //проверяем папку libraries
            if (!Directory.Exists(directory + "/libraries"))
            {
                foreach (string key in filesInfo.libraries.Keys)
                {
                    Updates.libraries.Add(key);
                    countFiles++;
                }

            } else {

                if (!updates.ContainsKey("libraries") || filesInfo.version.librariesLastUpdate != updates["libraries"]) //если версия libraries старая, тот отправляем на обновления
                {

                    foreach (string key in filesInfo.libraries.Keys)
                    {
                        if (filesInfo.libraries[key] == "windows" || filesInfo.libraries[key] == "all")
                        {
                            Updates.libraries.Add(key);
                            countFiles++;
                        }

                    }

                } else {

                    foreach (string lib in filesInfo.libraries.Keys) //ищем недостающие файлы
                    {
                        if (filesInfo.libraries[lib] == "windows" || filesInfo.libraries[lib] == "all")
                        {
                            if (!File.Exists(directory + "/libraries/" + lib))
                            {
                                Updates.libraries.Add(lib);
                                countFiles++;
                            }

                        }
                    }
                }
            }

            //проверяем assets
            if (!Directory.Exists(directory + "/assets/virtual/" + filesInfo.version.assetsVersion))
            {
                Updates.assetsVirtual = true;
                countFiles++;
            }

            if (!File.Exists(directory + "/assets/indexes/" + filesInfo.version.assetsVersion + ".json"))
            {
                Updates.assetsIndexes = true;
                countFiles++;
            }

            if (!Directory.Exists(directory + "/assets/objects"))
            {
                Updates.assetsObjects = true;
                countFiles++;
            }

            return true;
        }

        public static List<string> Update(InstanceFiles filesList, string instanceId, MainWindow window)
        {
            WebClient wc = new WebClient();
            Dictionary<string, int> updates = new Dictionary<string, int>();

            string[] folders;
            string addr;
            int completedDownloads = 0;
            List<string> errors = new List<string>();

            //функция для удаления файла при его существовании 
            void DelFile(string file)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }

            //функция обновления процента в прогресс баре
            void UpdateProgressBar()
            {
                completedDownloads++;
                double count = (double) completedDownloads / countFiles;
                count *= 100;
                window.Dispatcher.Invoke(delegate
                {
                    //window.ProgressBar.Value = (int) count;
                    //window.ProgressCount.Text = ((int)count).ToString() + "%";
                });
            }

            //функция для скачивания файлов(кроме libraries и natives)
            bool DownloadFile(string url, string file, string to, string sha1, int size)
            {
                
                string temp = directory + "/temp/";
                string zipFile = file + ".zip";

                //создаем папки в соответсвии с путем к файлу из списка
                string[] foldersPath = to.Replace(directory, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                string path = directory;
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

                    using (FileStream fstream = new FileStream(temp + file, FileMode.OpenOrCreate))
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        SHA1 sha = new SHA1Managed();
                        if (Convert.ToBase64String(sha.ComputeHash(fileBytes)) == sha1 && fileBytes.Length == size)
                        {
                            DelFile(to);
                            File.Move(temp + file, to);

                            return true;

                        } else {
                            File.Delete(temp + file);
                            return false;

                        }
                    }

                } catch {
                    DelFile(temp + file);
                    DelFile(temp + zipFile);

                    return false;
                }
            }

            //функция для скачивания libraries и natives
            bool DownloadApplicationFiles(string url, string to, string file)
            {
                //создаем папки в соответсвии с путем к файлу из списка
                string[] foldersPath = (to + file).Replace(directory, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                string path = directory;
                for (int i = 0; i < foldersPath.Length - 1; i++)
                {
                    path += "/" + foldersPath[i];
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                string temp = directory + "/temp/";
                string zipFile = file + ".zip";

                try
                {
                    wc.DownloadFile(url + ".zip", temp + zipFile);

                    DelFile(to + zipFile);
                    ZipFile.ExtractToDirectory(temp + zipFile, temp);
                    File.Delete(temp + zipFile);

                    DelFile(to + file);
                    File.Move(temp + file, to + file);

                    return true;

                } catch {
                    DelFile(temp + file);
                    DelFile(temp + zipFile);

                    return false;
                }

            }

            //пытаемся открыть файл с последними обновлениями
            try
            {
                using (FileStream fstream = File.OpenRead(directory + "/instances/" + instanceId + "/" + "lastUpdates.json"))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    //updates - словарь, в котором будут содержаться версии всех файлов

                    if (JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes)) != null)
                    {
                        updates = JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes));
                    }

                }

            } catch { }

            //скачивание файлов из списка data
            foreach (string dir in Updates.data.Keys)
            {
                foreach (string file in Updates.data[dir])
                {
                    folders = file.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    if (filesList.data[dir].objects[file].url == null)
                        addr = LaunсherSettings.serverUrl + "upload/modpacks/" + instanceId + "/" + dir + "/" + file;

                    else
                        addr = filesList.data[dir].objects[file].url;


                    if (!DownloadFile(addr, folders[folders.Length - 1], directory + "/instances/" + instanceId + "/" + dir + "/" + file, filesList.data[dir].objects[file].sha1, filesList.data[dir].objects[file].size))
                    {
                        errors.Add(dir + "/" + file);

                    } else {
                        updates[dir + "/" + file] = filesList.data[dir].objects[file].lastUpdate; //добавляем файл в список последних обновлений
                        UpdateProgressBar();
                    }
                }
            }

            //удаляем старые файлы
            foreach (string file in Updates.oldFiles)
            {
                if (File.Exists(directory + "/instances/" + instanceId + "/" + file))
                {
                    File.Delete(directory + "/instances/" + instanceId + "/" + file);
                    if (updates.ContainsKey(file))
                        updates.Remove(file);
                    UpdateProgressBar();
                }
            }

            //скачивание файла версии
            if (Updates.minecraftJar)
            {
                if (filesList.version.minecraftJar.url == null)
                    addr = LaunсherSettings.serverUrl + "upload/versions/" + filesList.version.minecraftJar.name;
                else
                    addr = filesList.version.minecraftJar.url;


                if (!DownloadFile(addr, filesList.version.minecraftJar.name, directory + "/instances/" + instanceId + "/version/" + filesList.version.minecraftJar.name, filesList.version.minecraftJar.sha1, filesList.version.minecraftJar.size))
                {
                    errors.Add("version/" + filesList.version.minecraftJar.name);

                } else {
                    updates["version"] = filesList.version.minecraftJar.lastUpdate;
                    UpdateProgressBar();
                }
            }


            //скачиваем natives
            if (filesList.version.nativesUrl == null)
                addr = LaunсherSettings.serverUrl + "upload/natives/" + filesList.version.gameVersion + "/";
            else
                addr = filesList.version.nativesUrl;

            foreach (string native in Updates.natives)
            {

                if (!DownloadApplicationFiles(addr + native, directory + "/instances/" + instanceId + "/version/natives/", native))
                {
                    //скачивание не удалось
                    errors.Add("natives/" + native);
                    DelFile(directory + "/instances/" + instanceId + "/version/natives/" + native);

                } else {
                    UpdateProgressBar();
                }
            }

            updates["natives"] = filesList.version.nativesLastUpdate;


            //скачиваем libraries
            if (filesList.version.librariesUrl == null)
                addr = LaunсherSettings.serverUrl + "upload/libraries/";
            else
                addr = filesList.version.librariesUrl;

            folders = null;
            string ff;
            foreach (string lib in Updates.libraries)
            {
                folders = lib.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                ff = lib.Replace(folders[folders.Length-1], "");

                if (!DownloadApplicationFiles(addr + lib, directory + "/libraries/" + ff, folders[folders.Length - 1]))
                {
                    errors.Add("libraries/" + lib);
                    DelFile(directory + "/libraries/" + lib);

                } else {
                    UpdateProgressBar();
                }

            }

            updates["libraries"] = filesList.version.librariesLastUpdate;

            //скачиваем assets
            if (!Directory.Exists(directory + "/assets"))
                Directory.CreateDirectory(directory + "/assets");

            if (Updates.assetsObjects)
            {
                if (!Directory.Exists(directory + "/assets/objects"))
                    Directory.CreateDirectory(directory + "/assets/objects");

                try
                {
                    wc.DownloadFile(LaunсherSettings.serverUrl + "upload/assets/" + filesList.version.assetsVersion + "/objects.zip", directory + "/temp/objects.zip");

                    ZipFile.ExtractToDirectory(directory + "/temp/objects.zip", directory + "/assets/objects");
                    File.Delete(directory + "/temp/objects.zip");

                    UpdateProgressBar();
                } catch {
                    errors.Add("asstes/objects");
                }
            }

            if (Updates.assetsIndexes)
            {
                if (!Directory.Exists(directory + "/assets/indexes"))
                    Directory.CreateDirectory(directory + "/assets/indexes");

                wc.DownloadFile(filesList.version.assetsIndexes, directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json");

                try
                {
                    wc.DownloadFile(filesList.version.assetsIndexes, directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json");
                    UpdateProgressBar();
                } catch {
                    errors.Add("asstes/indexes");
                }   
            }

            if (Updates.assetsVirtual)
            {
                if (!Directory.Exists(directory + "/assets/virtual"))
                    Directory.CreateDirectory(directory + "/assets/virtual");

                if (!Directory.Exists(directory + "/assets/virtual/" + filesList.version.assetsVersion))
                    Directory.CreateDirectory(directory + "/assets/virtual/" + filesList.version.assetsVersion);

                try
                {
                    wc.DownloadFile(LaunсherSettings.serverUrl + "upload/assets/" + filesList.version.assetsVersion + "/" + filesList.version.assetsVersion + ".zip", directory + "/temp/" + filesList.version.assetsVersion + ".zip");

                    ZipFile.ExtractToDirectory(directory + "/temp/" + filesList.version.assetsVersion + ".zip", directory + "/assets/virtual/" + filesList.version.assetsVersion);
                    File.Delete(directory + "/temp/" + filesList.version.assetsVersion + ".zip");

                    UpdateProgressBar();

                } catch {
                    errors.Add("asstes/virtuals");
                }
            }

            //сохраняем файл с последними обновлениями 
            try
            {
                using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/" + "lastUpdates.json", FileMode.OpenOrCreate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(updates));
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            } catch { }

            Updates.data = new Dictionary<string, List<string>>();
            Updates.natives = new List<string>();
            Updates.libraries = new List<string>();
            Updates.minecraftJar = false;
            Updates.assetsObjects = false;
            Updates.assetsIndexes = false;
            Updates.assetsVirtual = false;
            Updates.oldFiles = new List<string>();

            return errors;

        }

        public static void SaveSettings(Dictionary<string, string> data, string instanceId = "")
        {
            string file;

            if (instanceId == "")
            {
                string path = Environment.ExpandEnvironmentVariables("%appdata%") + "/night-world";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/settings.json";
            }
            else
            {
                string path = directory + "/instances/" + instanceId;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                file = path + "/instanceSettings.json";
            }

            try
            {
                Dictionary<string, string> settings = GetSettings(instanceId);
                if (settings != null)
                {
                    foreach (string key in data.Keys)
                        settings[key] = data[key];


                } else {
                    settings = data;
                }

                if (settings.ContainsKey("password"))
                    settings["password"] = Convert.ToBase64String(AesСryp.Encode(settings["password"], Encoding.Default.GetBytes(LaunсherSettings.passwordKey), Encoding.Default.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16))));

                if (!File.Exists(file))
                    File.Create(file).Close();

                using (FileStream fstream = new FileStream(file, FileMode.Truncate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            } catch {}
        }

        public static Dictionary<string, string> GetSettings(string instanceId = "")
        {
            string file;
            if (instanceId == "")
            {
                file = Environment.ExpandEnvironmentVariables("%appdata%") + "/night-world/settings.json";
            }
            else
            {
                file = directory + "/instances/" + instanceId + "/instanceSettings.json";

            }

            try
            {
                using (FileStream fstream = File.OpenRead(file))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(fileBytes));

                    if (settings.ContainsKey("password"))
                        settings["password"] = AesСryp.Decode(Convert.FromBase64String(settings["password"]), Encoding.Default.GetBytes(LaunсherSettings.passwordKey), Encoding.Default.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16)));

                    return settings;
                }

            } catch {
                return new Dictionary<string, string>();
            }

        }       

        static private void SaveFile(string name, string content)
        {
            try
            {
                if (!File.Exists(name))
                    File.Create(name).Close();

                using (FileStream fstream = new FileStream(name, FileMode.Truncate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(content);
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            } catch { }

        }

        static private T GetFile<T>(string file)
        {
            try
            {
                using (FileStream fstream = File.OpenRead(file))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(fileBytes));

                }

            } catch {
                return default(T);
            }

        }

        public static void SaveFilesList(string instanceId, InstanceFiles files)
        {
            SaveFile(directory + "/instances/" + instanceId + "/" + "filesList.json", JsonConvert.SerializeObject(files));
        }

        public static InstanceFiles GetFilesList(string instanceId)
        {
            return GetFile<InstanceFiles>(directory + "/instances/" + instanceId + "/" + "filesList.json");
        }

        public static Dictionary<string, string> GetModpaksList()
        {
            Dictionary<string, string> baseList = GetFile<Dictionary<string, string>>(directory + "/instanesList.json");
            Dictionary<string, string> list = new Dictionary<string, string>();

            if (baseList != null)
            {
                foreach (string key in baseList.Keys)
                {
                    if (Directory.Exists(directory + "/instances/" + key))
                    {
                        list[key] = baseList[key];
                    }
                }
            }

            return list;

        }

        public static void SaveModpaksList(Dictionary<string, string> content)
        {
            SaveFile(directory + "/instanesList.json", JsonConvert.SerializeObject(content));
        }

        public static bool DownloadUpgradeTool()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(LaunсherSettings.serverUrl, directory + "/UpgradeTool.exe");
                return true;

            } catch { return false; }

        }

        public static int GetUpgradeToolVersion()
        {

            if (!File.Exists(directory + "/up-version.txt"))
                return -1;

            try
            {
                using (FileStream fstream = File.OpenRead(directory + "/up-version.txt"))
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    return Int32.Parse(Encoding.UTF8.GetString(fileBytes));

                }

            } catch { return -1; }

        }

        public static void SetUpgradeToolVersion(int version)
        {
            try
            {
                if (!File.Exists(directory + "/up-version.txt"))
                    File.Create(directory + "/up-version.txt").Close();

                using (FileStream fstream = new FileStream(directory + "/up-version.txt", FileMode.Truncate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(version.ToString());
                    fstream.Write(bytes, 0, bytes.Length);
                    fstream.Close();
                }

            } catch { }

        }

        public static bool DeleteLastUpdates(string instanceId) //Эта функция удаляет файл lastUpdates.json
        {
            try
            {
                if(File.Exists(directory + "/instances/" + instanceId + "/lastUpdates.json"))
                {
                    File.Delete(directory + "/instances/" + instanceId + "/lastUpdates.json");
                }

                return true;

            } catch { return false; }
        }

        public static void RemoveInstanceDirecory(string instanceId)
        {
            try
            {
                if (Directory.Exists(directory + "/instances/" + instanceId))
                {
                    Directory.Delete(directory + "/instances/" + instanceId, true);
                }

                Thread.Sleep(1000);

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

        public static void CheckLauncherAssets()
        {
            bool update = false;
            bool updateJsonFile = false;

            try
            {
                string answer = ToServer.HttpPost("filesList/launcherAssets.json");
                LauncherAssets data = JsonConvert.DeserializeObject<LauncherAssets>(answer);

                if (!File.Exists(directory + "/launcherAssets.json")) //если нет файла launcherAssets.json, то скачиваем его
                {
                    SaveFile(directory + "/launcherAssets.json", JsonConvert.SerializeObject(data.data));
                    updateJsonFile = true;
                }


                if (!UserData.settings.ContainsKey("launcherAssetsV") || Int32.Parse(UserData.settings["launcherAssetsV"]) < data.version)
                {
                    updateJsonFile = true;
                    update = true;
                }
                else
                {
                    if (!Directory.Exists(directory + "/launcherAssets"))
                    {
                        updateJsonFile = true;
                        update = true;
                    }
                    else
                    {
                        foreach (InstanceAssets instance in data.data.Values)
                        {
                            foreach (string file in instance.images)
                            {
                                if (!File.Exists(directory + "/launcherAssets/" + file))
                                {
                                    update = true;
                                    goto DownloadAssets;
                                }

                            }
                        }

                    }

                }

                DownloadAssets: //скачивание асетсов
                if (update)
                {
                    if (!Directory.Exists(directory + "/launcherAssets"))
                    {
                        Directory.CreateDirectory(directory + "/launcherAssets");
                    }
                    else
                    {
                        var dirInfo = new DirectoryInfo(directory + "/launcherAssets");
                        foreach (var file in dirInfo.GetFiles())
                            file.Delete();
                    }

                    WebClient wc = new WebClient();
                    wc.DownloadFile(LaunсherSettings.serverUrl + "/upload/images.zip", directory + "/temp/launcherAssets-Images.zip");

                    ZipFile.ExtractToDirectory(directory + "/temp/launcherAssets-Images.zip", directory + "/launcherAssets");
                    File.Delete(directory + "/temp/launcherAssets-Images.zip");

                    wc.DownloadFile(LaunсherSettings.serverUrl + "/upload/images.zip", directory + "/temp/launcherAssets-Images.zip");

                    UserData.settings["launcherAssetsV"] = data.version.ToString();

                    if (!updateJsonFile)
                        SaveSettings(UserData.settings);
                }

                if (updateJsonFile)
                {
                    SaveFile(directory + "/launcherAssets.json", JsonConvert.SerializeObject(data.data));

                    UserData.settings["launcherAssetsV"] = data.version.ToString();
                    SaveSettings(UserData.settings);
                }

            }  catch { }

        }

        public static Dictionary<string, InstanceAssets> GetInstanceAssets()
        {
            try
            {
                var data = GetFile<Dictionary<string, InstanceAssets>>(directory + "/launcherAssets.json");

                return data;
                
            } catch { return new Dictionary<string, InstanceAssets>(); }
        }

        public static bool InstanceIsInstalled(string instanceId)
        {
            try
            {
                return Directory.Exists(directory + "/instances/" + instanceId);
            }
            catch
            {
                return false;
            }
        }

        public static bool ExportProfile(string instanceId, List<string> folders, string path)
        {
            return true;
        }
    }
}
