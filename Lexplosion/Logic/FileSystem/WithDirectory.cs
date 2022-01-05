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
using System.Text.RegularExpressions;
using System.Windows;
using System.Linq;
using Lexplosion.Logic.Management;
using static Lexplosion.Logic.FileSystem.DataFilesManager;
using System.Diagnostics;

namespace Lexplosion.Logic.FileSystem
{
    static class WithDirectory
    {
        // TODO: во всём WithDirectory я заменяю элементы адресов директорий через replace. Не знаю как на винде, но на линуксе могут появиться проблемы, ведь replace заменяет подстроки в строке, а не только конечную подстроку
        // этот класс возвращает метод CheckBaseFiles
        public class BaseFilesUpdates
        {
            public List<string> Natives = new List<string>();
            public Dictionary<string, LibInfo> Libraries = new Dictionary<string, LibInfo>();
            public bool MinecraftJar = false;
            public bool Assets = false;
        }

        private class LauncherAssets //этот класс нужен для декодирования json
        {
            public int version;
            public Dictionary<string, InstanceAssets> data;
        }

        private struct Assets
        {
            public struct AssetFile
            {
                public string hash;
            }

            public Dictionary<string, AssetFile> objects;
        }

        public static string directory;

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

            }
            catch { }
        }

        private static string CreateTempDir()
        {
            string dirName = directory + "/temp";
            string dirName_ = dirName;

            Random random = new Random();
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
            try
            {
                string tempDir = CreateTempDir();
                if (!Directory.Exists(directory + path))
                {
                    Directory.CreateDirectory(directory + path);
                }

                using (WebClient wc = new WebClient())
                {
                    DelFile(tempDir + fileName);
                    wc.DownloadFile(url, tempDir + fileName);
                    File.Move(tempDir + fileName, directory + "/" + path + "/" + fileName);
                }

                Directory.Delete(tempDir, true);

                return true;
            }
            catch
            {
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

                using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Create, FileAccess.Write))
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
                if (!File.Exists(directory + "/instances/" + instanceId + "/" + "lastUpdates.json"))
                {
                    if (!Directory.Exists(directory + "/instances/" + instanceId))
                    {
                        Directory.CreateDirectory(directory + "/instances/" + instanceId); //создаем папку с модпаком, если её нет
                    }
                }

                using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/lastUpdates.json", FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл с последними обновлениями
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
                        File.Delete(directory + "/instances/" + instanceId + "/lastUpdates.json");
                    }
                }

            }
            catch { }

            return updates;
        }

        public static NightworldIntance.ModpackFilesUpdates CheckNigntworldInstance(NInstanceManifest filesInfo, string instanceId, ref Dictionary<string, int> updates)
        {
            var filesUpdates = new NightworldIntance.ModpackFilesUpdates();

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

                    // TODO: тут из lastUpdates удалить все файлы из этой папки

                    //отрываем файл с последними обновлениями и записываем туда updates, который уже содержит последнюю версию папки. Папка сейчас будет пустой, поэтому метод Update в любом случае скачает нужные файлы
                    using (FileStream fstream = new FileStream(directory + "/instances/" + instanceId + "/lastUpdates.json", FileMode.Create, FileAccess.Write))
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
                                        using(SHA1 sha = new SHA1Managed())
                                        {
                                            if (Convert.ToBase64String(sha.ComputeHash(bytes)) != filesInfo.data[dir].objects[fileName].sha1 || bytes.Length != filesInfo.data[dir].objects[fileName].size)
                                            {
                                                File.Delete(file); //удаляем файл, если не сходится хэш или размер

                                                if (!filesUpdates.Data.ContainsKey(dir)) //если директория отсутствует в data, то добавляем её 
                                                {
                                                    filesUpdates.Data.Add(dir, new List<string>());
                                                }

                                                filesUpdates.Data[dir].Add(fileName); //добавляем файл в список на обновление
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
                                }

                                if (!filesUpdates.Data[dir].Contains(fileName))
                                {
                                    filesUpdates.Data[dir].Add(fileName);
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
                        }    

                        if (!filesUpdates.Data[dir].Contains(file))
                        {
                            filesUpdates.Data[dir].Add(file);
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
                            filesUpdates.OldFiles.Add(folder + "/" + file);
                        }

                    }
                    catch { }
                }
            }

            return filesUpdates;
        }

        // TODO: его вызов обернуть в try
        public static BaseFilesUpdates CheckBaseFiles(VersionManifest filesInfo, string instanceId, ref Dictionary<string, int> updates) // функция проверяет основные файлы клиента (файл версии, либрариесы и тп)
        {
            BaseFilesUpdates updatesList = new BaseFilesUpdates(); //возвращаемый список обновлений

            //проверяем файл версии
            if (!Directory.Exists(directory + "/instances/" + instanceId + "/version"))
            {
                Directory.CreateDirectory(directory + "/instances/" + instanceId + "/version"); //создаем папку versions если её нет
                updatesList.MinecraftJar = true; //сразу же добавляем minecraftJar в обновления
            }
            else
            {
                string minecraftJarFile = directory + "/instances/" + instanceId + "/version/" + filesInfo.version.minecraftJar.name;
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
                }
            }

            //получаем версию libraries
            if (File.Exists(directory + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesInfo.version) + ".lver"))
            {
                try
                {
                    using (FileStream fstream = new FileStream(directory + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesInfo.version) + ".lver", FileMode.OpenOrCreate, FileAccess.Read)) //открываем файл с версией libraries
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
                SaveFile(directory + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesInfo.version) + ".lver", "0");
                updates["libraries"] = 0;
            }

            //проверяем папку libraries
            if (!Directory.Exists(directory + "/libraries"))
            {
                foreach (string lib in filesInfo.libraries.Keys)
                {
                    updatesList.Libraries[lib] = filesInfo.libraries[lib];
                }
            }
            else
            {
                if (filesInfo.version.librariesLastUpdate != updates["libraries"]) //если версия libraries старая, то отправляем на обновления
                {
                    foreach (string lib in filesInfo.libraries.Keys)
                    {
                        updatesList.Libraries[lib] = filesInfo.libraries[lib];
                    }
                }
                else
                {
                    // получем файл, в ктором хранятси список либрариесов, которые удачно скачались в прошлый раз
                    List<string> downloadedFiles = new List<string>();
                    string downloadedInfoAddr = directory + "/versions/libraries/" + GetLibName(instanceId, filesInfo.version) + "-downloaded.json";
                    bool fileExided = false;
                    if (File.Exists(downloadedInfoAddr))
                    {
                        downloadedFiles = GetFile<List<string>>(downloadedInfoAddr);
                        fileExided = true;
                    }

                    //ищем недостающие файлы
                    foreach (string lib in filesInfo.libraries.Keys) 
                    {
                        if ((downloadedFiles == null && fileExided) || !File.Exists(directory + "/libraries/" + lib) || (fileExided && downloadedFiles != null && !downloadedFiles.Contains(lib)))
                        {
                            updatesList.Libraries[lib] = filesInfo.libraries[lib];
                        }
                    }
                }
            }

            //проверяем natives
            if (!Directory.Exists(directory + "/instances/" + instanceId + "/version/natives/"))
            {
                foreach (string key in filesInfo.natives.Keys) //добавляем natives в обновления
                {
                    updatesList.Natives.Add(key);
                }
            }
            else
            {
                if (!updates.ContainsKey("natives") || filesInfo.version.nativesLastUpdate != updates["natives"]) //если версия natives старая, то отправляем на обновления
                {
                    foreach (string key in filesInfo.natives.Keys)
                    {
                        if (filesInfo.natives[key] == "windows" || filesInfo.natives[key] == "all")
                        {
                            updatesList.Natives.Add(key);
                        }
                    }
                }
                else
                {
                    foreach (string n in filesInfo.natives.Keys) //ищем недостающие файлы
                    {
                        if (filesInfo.natives[n] == "windows" || filesInfo.natives[n] == "all")
                        {
                            if (!File.Exists(directory + "/instances/" + instanceId + "/version/natives/" + n))
                            {
                                updatesList.Natives.Add(n);
                            }
                        }
                    }
                }
            }

            //проверяем assets
            // TODO: сделать нормальное обновление асетсов
            if (!File.Exists(directory + "/assets/indexes/" + filesInfo.version.assetsVersion + ".json"))
            {
                updatesList.Assets = true;
            }

            return updatesList;
        }

        //функция для удаления файла при его существовании 
        private static void DelFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        //функция для скачивания файлов клиента в zip формате, без проверки хеша
        private static bool UnsafeDownloadZip(string url, string to, string file, string temp, WebClient wc)
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
            string[] foldersPath = to.Replace(directory, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // TODO: это всё можно одной функцией заменить

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
            string[] foldersPath = (to + file).Replace(directory, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string path = directory; 
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
            string[] foldersPath = to.Replace(directory, "").Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // TODO: это всё можно одной функцией заменить

            string path = directory;
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
                    isDownload = SaveDownloadJar(addr, minecraftJar.name, directory + "/instances/" + instanceId + "/version/" + minecraftJar.name, temp, minecraftJar.sha1, minecraftJar.size, wc);
                }
                else
                {
                    isDownload = SaveDownloadZip(addr, minecraftJar.name, directory + "/instances/" + instanceId + "/version/" + minecraftJar.name, temp, minecraftJar.sha1, minecraftJar.size, wc);
                }

                if (isDownload)
                {
                    updates["version"] = minecraftJar.lastUpdate;
                }
                else
                {
                    errors.Add("version/" + minecraftJar.name);
                }

            }

            //скачиваем natives
            if (filesList.version.nativesUrl == null)
            {
                addr = LaunсherSettings.URL.Upload + "natives/";
            }
            else
            {
                addr = filesList.version.nativesUrl;
            }

            foreach (string native in updateList.Natives)
            {

                if (!UnsafeDownloadZip(addr + native, directory + "/instances/" + instanceId + "/version/natives/", native, temp, wc))
                {
                    //скачивание не удалось
                    errors.Add("natives/" + native);
                    DelFile(directory + "/instances/" + instanceId + "/version/natives/" + native);
                }
                else
                {
                    //UpdateProgressBar();
                }
            }

            updates["natives"] = filesList.version.nativesLastUpdate;

            //скачиваем libraries
            folders = null;
            List<string> executedMethods = new List<string>();
            List<string> downloadedLibs = new List<string>(); // сюда мы пихаем файлы, которые удачно скачались. При каждом удачном скачивании сохраняем список в файл. Если все файлы скачались удачно - удаляем этот список
            string downloadedLibsAddr = directory + "/versions/libraries/" + GetLibName(instanceId, filesList.version) + "-downloaded.json"; // адрес файла в котором убдет храниться список downloadedLibs

            if (updateList.Libraries.Count > 0) //сохраняем версию либририесов если в списке на обновление(updateList.Libraries) есть хотя бы один либрариес
            {
                SaveFile(directory + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesList.version) + ".lver", filesList.version.librariesLastUpdate.ToString());
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
                    if (updateList.Libraries[lib].notArchived)
                    {
                        isDownload = UnsafeDownloadJar(addr, directory + "/libraries/" + ff, folders[folders.Length - 1], wc, tempDir);
                    }
                    else
                    {
                        isDownload = UnsafeDownloadZip(addr, directory + "/libraries/" + ff, folders[folders.Length - 1], temp, wc);
                    }

                    if (isDownload)
                    {
                        //UpdateProgressBar();
                        downloadedLibs.Add(lib);
                        SaveFile(downloadedLibsAddr, JsonConvert.SerializeObject(downloadedLibs));
                    }
                    else
                    {
                        errors.Add("libraries/" + lib);
                        DelFile(directory + "/libraries/" + lib);
                    }
                }
                else
                {
                    Console.WriteLine(" ");
                    try
                    {
                        List<List<string>> obtainingMethod = updateList.Libraries[lib].obtainingMethod; // получаем метод

                        if (!executedMethods.Contains(obtainingMethod[0][0])) //проверяем был ли этот метод уже выполнен
                        {
                            Console.WriteLine("TEMP " + tempDir);
                            int i = 1; //начинаем цикл с первого элемента, т.к нулевой - название метода
                            while (i < obtainingMethod.Count)
                            {
                                Console.WriteLine("METHOD " + obtainingMethod[i][0]);
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
                                            Console.WriteLine("ERROR");
                                            goto EndWhile; //возникла ошибка
                                        }

                                        string command = obtainingMethod[i][2];
                                        command = command.Replace("{DIR}", directory);
                                        command = command.Replace("{TEMP_DIR}", tempDir);
                                        command = command.Replace("{MINECRAFT_JAR}", directory + "/instances/" + instanceId + "/version/" + filesList.version.minecraftJar.name);
                                        Console.WriteLine();
                                        Console.WriteLine(command);

                                        if (!Utils.StartProcess(command, executord))
                                        {
                                            errors.Add("libraries/" + lib);
                                            goto EndWhile; //возникла ошибка
                                        }

                                        break;

                                    case "moveFile":
                                        string to = obtainingMethod[i][2].Replace("{DIR}", directory).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
                                        string from = obtainingMethod[i][1].Replace("{DIR}", directory).Replace("{TEMP_DIR}", tempDir).Replace("//", "/");
                                        if (File.Exists(to))
                                        {
                                            File.Delete(to);
                                        }
                                        if (!Directory.Exists(to.Replace(Path.GetFileName(to), "")))
                                        {
                                            Directory.CreateDirectory(to.Replace(Path.GetFileName(to), ""));
                                        }
                                        File.Move(from, to);
                                    Console.WriteLine(to + " " + from);
                                        break;
                                }
                                i++;
                            }
                        }

                    //теперь добавляем этот метод в уже выполненные и если не существует файла, который мы должны получить - значит произошла ошибка
                    EndWhile: executedMethods.Add(obtainingMethod[0][0]);
                        if (!File.Exists(directory + "/libraries/" + lib))
                        {
                            Console.WriteLine(directory + "/libraries/" + lib);
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
                }
            }

            Directory.Delete(tempDir, true);
            Directory.Delete(temp, true);

            if (downloadedLibs.Count == updateList.Libraries.Count)
            {
                //все либрариесы скачались удачно. Удаляем файл
                DelFile(downloadedLibsAddr);
            }

            //скачиваем assets
            if (!Directory.Exists(directory + "/assets"))
                Directory.CreateDirectory(directory + "/assets");

            if (updateList.Assets)
            {
                if (!Directory.Exists(directory + "/assets/indexes"))
                    Directory.CreateDirectory(directory + "/assets/indexes");

                wc.DownloadFile(filesList.version.assetsIndexes, directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json"); // TODO: заюзать мою функцию для скачивания

                try
                {
                    wc.DownloadFile(filesList.version.assetsIndexes, directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json");
                    //UpdateProgressBar();
                }
                catch
                {
                    errors.Add("asstes/indexes");
                }

                if (!Directory.Exists(directory + "/assets/objects"))
                    Directory.CreateDirectory(directory + "/assets/objects");

                Assets asstes = GetFile<Assets>(directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json");

                if (asstes.objects != null)
                {
                    foreach (string asset in asstes.objects.Keys)
                    {
                        string assetHash = asstes.objects[asset].hash;
                        if (assetHash != null)
                        {
                            string assetPath = "/" + assetHash.Substring(0, 2);
                            if (!File.Exists(directory + "/assets/objects/" + assetPath + "/" + assetHash))
                            {
                                if (!InstallFile("http://resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath))
                                {
                                    Console.WriteLine("ERROR:1 " + "http://resources.download.minecraft.net" + assetPath + "/" + assetHash, assetHash, "/assets/objects/" + assetPath);
                                }
                            }
                        }
                        else
                        {
                            errors.Add("asstes/objects");
                        }

                    }
                }
                else
                {
                    errors.Add("asstes/objects");
                }
            }

            wc.Dispose();

            //сохраняем lastUpdates
            SaveFile(directory + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

            return errors;
        }

        public static List<string> UpdateNightworldInstance(NightworldIntance.ModpackFilesUpdates updatesList, NInstanceManifest filesList, string instanceId, string externalId, ref Dictionary<string, int> updates)
        {
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

                    if (!SaveDownloadZip(addr, folders[folders.Length - 1], directory + "/instances/" + instanceId + "/" + dir + "/" + file, tempDir, filesList.data[dir].objects[file].sha1, filesList.data[dir].objects[file].size, wc))
                    {
                        errors.Add(dir + "/" + file);
                    }
                    else
                    {
                        updates[dir + "/" + file] = filesList.data[dir].objects[file].lastUpdate; //добавляем файл в список последних обновлений
                    }

                    // TODO: где-то тут записывать что файл был обновлен, чтобы если загрузка была первана она началась с того же места
                }
            }     

            wc.Dispose();

            //удаляем старые файлы
            foreach (string file in updatesList.OldFiles)
            {
                if (File.Exists(directory + "/instances/" + instanceId + "/" + file))
                {
                    File.Delete(directory + "/instances/" + instanceId + "/" + file);
                    if (updates.ContainsKey(file))
                    {
                        updates.Remove(file);
                    }
                }
            }

            //сохарняем updates
            SaveFile(directory + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

            Directory.Delete(tempDir, true);

            return errors;
        }

        public static ExportResult ExportInstance(string instanceId, List<string> directoryList, string exportFile, string description)
        {
            // TODO: удалять временную папку в конце
            string targetDir = CreateTempDir() + instanceId + "-export"; //временная папка, куда будем копировать все файлы
            string srcDir = directory + "/instances/" + instanceId;

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
                ["name"] = UserData.Instances.List[instanceId].Name,
                ["author"] = UserData.login,
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

        public static ImportResult ImportInstance(string zipFile, out List<string> errors)
        {
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
            MessageBox.Show(instanceId);

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

                return ImportResult.MovingFilesError;
            }

            LocalInstance instance = new LocalInstance(instanceId);

            instance.Check(); // TODO: тут вовзращать ошибки
            instance.Update();

            try
            {
                Directory.Delete(dir, true);
            }
            catch { }
            // TODO: Тут вырезал строку
            /*
            if (Gui.PageType.Right.Menu.InstanceContainerPage.obj != null)
            {
                Uri logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
                Gui.PageType.Right.Menu.InstanceContainerPage.obj.BuildInstanceForm(instanceId, UserData.InstancesList.Count - 1, logoPath, UserData.InstancesList[instanceId].Name, "NightWorld", "test", new List<string>());
            }
            */
            return ImportResult.Successful;
        }

        public static void RemoveInstanceDirecory(string instanceId)
        {
            try
            {
                if (Directory.Exists(directory + "/instances/" + instanceId))
                {
                    Directory.Delete(directory + "/instances/" + instanceId, true);
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

        public static CurseforgeInstance.InstanceManifest DownloadCurseforgeInstance(string downloadUrl, string fileName, string instanceId, out List<string> errors, ref List<string> localFiles)
        {
            if (localFiles != null)
            {
                //удаляем старые файлы
                foreach (string file in localFiles)
                {
                    DelFile(directory + "/instances/" + instanceId + file);
                }
            }

            errors = new List<string>();
            localFiles = new List<string>();

            try
            {
                string tempDir = CreateTempDir();
                if (File.Exists(tempDir + fileName))
                {
                    File.Delete(tempDir + fileName);
                }

                using (WebClient wc = new WebClient())
                {
                    DelFile(tempDir + fileName);
                    wc.DownloadFile(downloadUrl, tempDir + fileName);
                }

                if (Directory.Exists(tempDir + "dataDownload"))
                {
                    Directory.Delete(tempDir + "dataDownload", true);
                }

                Directory.CreateDirectory(tempDir + "dataDownload");
                ZipFile.ExtractToDirectory(tempDir + fileName, tempDir + "dataDownload");
                DelFile(tempDir + fileName);

                var data = GetFile<CurseforgeInstance.InstanceManifest>(tempDir + "dataDownload/manifest.json");

                if (data != null)
                {
                    foreach (CurseforgeInstance.InstanceManifest.FileData file in data.files)
                    {
                        Dictionary<string, (CurseforgeApi.InstalledAddonInfo, CurseforgeApi.DownloadAddonRes)> result = 
                            CurseforgeApi.DownloadAddon(file.projectID, file.fileID, tempDir.Replace(directory, "") + "dataDownload/overrides/");
                        if (result[result.First().Key].Item2 != CurseforgeApi.DownloadAddonRes.Successful) //скачивание мода не удалось. Добавляем его данные в список ошибок и выходим
                        {
                            errors.Add(file.projectID + " " + file.fileID);
                            return null;
                        }
                    }

                    Console.WriteLine("END MODS");

                    string SourcePath = tempDir + "dataDownload/overrides/";
                    string DestinationPath = directory + "/instances/" + instanceId + "/";

                    foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
                    }

                    foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                        localFiles.Add(newPath.Replace(SourcePath, "/"));
                    }

                    if (Directory.Exists(tempDir + "/dataDownload"))
                    {
                        Directory.Delete(tempDir + "/dataDownload", true);
                    }

                    Console.WriteLine("Return");

                    return data;
                }

                if (Directory.Exists(tempDir + "/dataDownload"))
                {
                    Directory.Delete(tempDir + "/dataDownload", true);
                }

                errors.Add("curseforgeManifestError");

                return null;
            }
            catch
            {
                MessageBox.Show("cath-");
                errors.Add("uncnowError");
                return null;
            }
        }
    }
}
