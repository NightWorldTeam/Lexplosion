using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.IO.Compression;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using System.Threading;
using Lexplosion.Gui.Windows;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using System.Text.RegularExpressions;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    static class WithDirectory
    {
        public static string directory;
        public static int countFiles;

        private struct Updates //структура, хранящая всё, что нужно обновить. Метод Check в неё кладет, а метод Update - достает 
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

        private struct LauncherAssets //этот класс нужен для декодирования json
        {
            public int version;
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

                } 
                else if (!Directory.Exists(directory + "/temp")) 
                { 
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
                    using (FileStream fstream = File.OpenRead(directory + "/instances/" + instanceId + "/lastUpdates.json")) //открываем файл с последними обновлениями
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        try
                        {
                            if (JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes)) != null)
                                updates = JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(fileBytes));

                        } catch {
                            File.Delete(directory + "/instances/" + instanceId + "/lastUpdates.json");
                        }

                    }

                } 
                catch { }

            }

            if (File.Exists(directory + "/versions/libraries/lastUpdates/" + filesInfo.version.gameVersion + ".lver"))
            {
                using (FileStream fstream = File.OpenRead(directory + "/versions/libraries/lastUpdates/" + filesInfo.version.gameVersion + ".lver")) //открываем файл с версией libraries
                {
                    byte[] fileBytes = new byte[fstream.Length];
                    fstream.Read(fileBytes, 0, fileBytes.Length);
                    fstream.Close();

                    int ver = 0;
                    Int32.TryParse(Encoding.UTF8.GetString(fileBytes), out ver);
                    updates["libraries"] = ver;

                }

            }
            else
            {
                SaveFile(directory + "/versions/libraries/lastUpdates/" + filesInfo.version.gameVersion + ".lver", "0");
            }

            //Проходимся по списку папок(data) из класса instanceFiles
            foreach (string dir in filesInfo.data.Keys)
            {
                string folder = directory + "/instances/" + instanceId + "/" + dir;

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
                    }

                    using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Truncate))
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
                                {
                                    Updates.data.Add(dir, new List<string>());
                                } 
                                   

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
                foreach (string lib in filesInfo.libraries)
                {
                    Updates.libraries.Add(lib);
                    countFiles++;
                }

            } else {

                if (!updates.ContainsKey("libraries") || filesInfo.version.librariesLastUpdate != updates["libraries"]) //если версия libraries старая, тот отправляем на обновления
                {
                    foreach (string lib in filesInfo.libraries)
                    {
                        Updates.libraries.Add(lib);
                        countFiles++;
                    }

                } else {

                    foreach (string lib in filesInfo.libraries) //ищем недостающие файлы
                    {
                        if (!File.Exists(directory + "/libraries/" + lib))
                        {
                            Updates.libraries.Add(lib);
                            countFiles++;
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

            wc.Dispose();

            //сохраняем файл с последними обновлениями 
            try
            {
                SaveFile(directory + "/versions/libraries/lastUpdates/" + filesList.version.gameVersion + ".lver", filesList.version.librariesLastUpdate.ToString());

                using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Truncate))
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

        public static bool DownloadUpgradeTool()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(LaunсherSettings.serverUrl, directory + "/UpgradeTool.exe");
                    return true;

                }            

            } 
            catch { return false; }

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

                            if (!File.Exists(directory + "/launcherAssets/" + instance.mainImage))
                            {
                                update = true;
                                goto DownloadAssets;
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

                    using(WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(LaunсherSettings.serverUrl + "/upload/images.zip", directory + "/temp/launcherAssets-Images.zip");

                        ZipFile.ExtractToDirectory(directory + "/temp/launcherAssets-Images.zip", directory + "/launcherAssets");
                        File.Delete(directory + "/temp/launcherAssets-Images.zip");

                        wc.DownloadFile(LaunсherSettings.serverUrl + "/upload/images.zip", directory + "/temp/launcherAssets-Images.zip");
                    }

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

        public static bool ExportInstance(string instanceId, List<string> directoryList, string exportPath, string description)
        {

            string targetDir = directory + "/temp/" + instanceId + "-export";
            string srcDir = directory + "/instances/" + instanceId;

            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }

            foreach (string dirUnit_ in directoryList)
            {
                string dirUnit = dirUnit_.Replace(@"\", "/");
                string target = dirUnit.Replace(srcDir, targetDir + "/files");
                string finalPath = target.Substring(0, target.LastIndexOf("/"));

                if (!Directory.Exists(finalPath))
                {
                    try
                    {
                        Directory.CreateDirectory(finalPath);
                    }
                    catch
                    {
                       return false;
                    }
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
                        return false;
                    }

                }
                else
                {
                    return false;
                }

            }

            InstanceFiles instanceFile = GetFilesList(instanceId);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["gameVersion"] = instanceFile.version.gameVersion,
                ["description"] = description,
                ["name"] = UserData.InstancesList[instanceId]
            };


            string jsonData = JsonConvert.SerializeObject(data);
            if(!SaveFile(targetDir + "/instanceInfo.json", jsonData))
            {
                try
                {
                    Directory.Delete(targetDir, true);
                } 
                catch { }

                return false;
            }


            try
            {
                ZipFile.CreateFromDirectory(targetDir, exportPath + "/" + instanceId + ".zip");
                Directory.Delete(targetDir, true);

                return true;
            }
            catch
            {
                Directory.Delete(targetDir, true);

                return false;

            }

        }

        public static byte ImportInstance(string zipFile, out List<string> errors)
        {
            string dir = directory + "/temp/import/";

            errors = new List<string>();

            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch
                {
                    return 255;
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
                return 1;
            }

            Dictionary<string, string> instanceInfo = GetFile<Dictionary<string, string>>(dir + "instanceInfo.json");

            if (instanceInfo == null)
            {
                Directory.Delete(dir, true);
                return 1;
            }

            if (!instanceInfo.ContainsKey("gameVersion") || !instanceInfo.ContainsKey("name"))
            {
                Directory.Delete(dir, true);
                return 1;
            }

            if (string.IsNullOrEmpty(instanceInfo["gameVersion"]) || string.IsNullOrEmpty(instanceInfo["name"]))
            {
                Directory.Delete(dir, true);
                return 1;
            }

            SHA1 sha = new SHA1Managed();
            Random rnd = new Random();

            //генерация id модпака
            string instanceId = instanceInfo["name"];
            instanceId = instanceId.Replace(" ", "_");

            if (Regex.IsMatch(instanceId.Replace("_", ""), @"[^a-zA-Z0-9]"))
            {
                do
                {
                    instanceId = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(instanceInfo["name"] + ":" + rnd.Next(0, 9999))));
                    instanceId = instanceId.Replace("+", "").Replace("/", "").Replace("=", "");
                    instanceId = instanceId.ToLower();
                }
                while (UserData.InstancesList.ContainsKey(instanceId));

            }

            string addr = dir + "files/";
            string targetDir = directory + "/instances/" + instanceId + "/";

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
                return 2;
            }

            if (UserData.offline)
            {
                Directory.Delete(dir, true);
                return 3;
            }

            InstanceFiles files = ToServer.GetFilesList(instanceInfo["gameVersion"], true);
            if(files == null)
            {
                return 4;
            }

            Check(files, instanceId);
            
            if(countFiles > 0)
            {
                errors = Update(files, instanceId, MainWindow.Obj);
                countFiles = 0;
            }

            SaveFilesList(instanceId, files);

            UserData.InstancesList[instanceId] = instanceInfo["name"];
            SaveModpaksList(UserData.InstancesList);

            Directory.Delete(dir, true);

            if(Gui.Pages.Right.Menu.ModpacksContainerPage.obj != null)
            {
                Uri logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
                Gui.Pages.Right.Menu.ModpacksContainerPage.obj.BuildInstanceForm(instanceId, UserData.InstancesList.Count - 1, logoPath, UserData.InstancesList[instanceId], "NightWorld", "test", new List<string>());
            }

            return 0;
        }

    }

}
