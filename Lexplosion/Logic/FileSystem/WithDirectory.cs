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
using static Lexplosion.Logic.FileSystem.DataFilesManager;
using System.Windows;
using System.Linq;
using Lexplosion.Logic.Management;

namespace Lexplosion.Logic.FileSystem
{
    static class WithDirectory
    {
        //этот класс возвращает метод CheckVariableFiles
        public class VariableFilesUpdates
        {
            public Dictionary<string, List<string>> Data = new Dictionary<string, List<string>>(); //сюда записываем файлы, которые нужно обновить
            public List<string> OldFiles = new List<string>(); // список старых файлов, которые нуждаются в обновлении
            public bool Successful = true; // удачна или неудачна ли проверка
        }

        // этот класс возвращает метод CheckBaseFiles
        public class BaseFilesUpdates
        {
            public List<string> Natives = new List<string>();
            public Dictionary<string, LibInfo> Libraries = new Dictionary<string, LibInfo>();
            public bool MinecraftJar = false;
            public bool AssetsObjects = false;
            public bool AssetsIndexes = false;
            public bool AssetsVirtual = false;
        }

        // Эти три класса нужны для декодирования json при инфы о фордже
        class ForgeArtifact
        {
            public string path;
            public string url;
            public int size;
        }

        class ForgeLib
        {
            public string name;
            public Dictionary<string, ForgeArtifact> downloads;
        }

        class ForgeVersionFile
        {
            public string mainClass;
            public string id;
            public List<ForgeLib> libraries;
        }

        private class LauncherAssets //этот класс нужен для декодирования json
        {
            public int version;
            public Dictionary<string, InstanceAssets> data;
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

        public static VariableFilesUpdates CheckVariableFiles(NInstanceManifest filesInfo, string instanceId, ref Dictionary<string, int> updates)
        {
            VariableFilesUpdates filesUpdates = new VariableFilesUpdates();

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

                    foreach (string lib in filesInfo.libraries.Keys) //ищем недостающие файлы
                    {
                        if (!File.Exists(directory + "/libraries/" + lib))
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
            if (!Directory.Exists(directory + "/assets/virtual/" + filesInfo.version.assetsVersion))
            {
                updatesList.AssetsVirtual = true;
            }

            if (!File.Exists(directory + "/assets/indexes/" + filesInfo.version.assetsVersion + ".json"))
            {
                updatesList.AssetsIndexes = true;
            }

            if (!Directory.Exists(directory + "/assets/objects"))
            {
                updatesList.AssetsObjects = true;
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

        //функция для скачивания libraries и natives (в zip файле)
        private static bool DownloadLibFiles(string url, string to, string file, WebClient wc)
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

        //функция для скачивания libraries и natives (в jar файле)
        private static bool DownloadJarLibFiles(string url, string to, string file, WebClient wc)
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

        //функция для скачивания файлов (кроме libraries и natives) (в zip формате)
        private static bool DownloadFile(string url, string file, string to, string sha1, int size, WebClient wc)
        {
            string temp = directory + "/temp/";
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

        //функция для скачивания файлов (кроме libraries и natives) (в jar формате)
        private static bool DownloadJarFile(string url, string file, string to, string sha1, int size, WebClient wc)
        {
            string temp = directory + "/temp/";

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

            //скачивание файла версии
            if (updateList.MinecraftJar)
            {
                Objects.FileInfo minecraftJar = filesList.version.minecraftJar;
                if (minecraftJar.url == null)
                {
                    addr = LaunсherSettings.serverUrl + "upload/versions/" + minecraftJar.name;
                }
                else
                {
                    addr = minecraftJar.url;
                }

                bool isDownload;
                if (minecraftJar.notArchived)
                {
                    isDownload = DownloadJarFile(addr, minecraftJar.name, directory + "/instances/" + instanceId + "/version/" + minecraftJar.name, minecraftJar.sha1, minecraftJar.size, wc);
                }
                else
                {
                    isDownload = DownloadFile(addr, minecraftJar.name, directory + "/instances/" + instanceId + "/version/" + minecraftJar.name, minecraftJar.sha1, minecraftJar.size, wc);
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
                addr = LaunсherSettings.serverUrl + "upload/natives/" + filesList.version.gameVersion + "/";
            }
            else
            {
                addr = filesList.version.nativesUrl;
            }

            foreach (string native in updateList.Natives)
            {

                if (!DownloadLibFiles(addr + native, directory + "/instances/" + instanceId + "/version/natives/", native, wc))
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
            string ff;
            foreach (string lib in updateList.Libraries.Keys)
            {
                if (updateList.Libraries[lib].url == null)
                    addr = LaunсherSettings.serverUrl + "upload/libraries/";
                else
                    addr = updateList.Libraries[lib].url;

                folders = lib.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                ff = lib.Replace(folders[folders.Length - 1], "");

                bool isDownload;
                if (updateList.Libraries[lib].notArchived)
                {
                    isDownload = DownloadJarLibFiles(addr + lib, directory + "/libraries/" + ff, folders[folders.Length - 1], wc);
                }
                else
                {
                    isDownload = DownloadLibFiles(addr + lib, directory + "/libraries/" + ff, folders[folders.Length - 1], wc);
                }

                if (isDownload)
                {
                    //UpdateProgressBar();
                }
                else
                {
                    errors.Add("libraries/" + lib);
                    DelFile(directory + "/libraries/" + lib);
                }

            }

            if (updateList.Libraries.Count > 0) //сохраняем версию либририесов если в списке на обновление(updateList.Libraries) есть хотя бы один либрариес
            {
                SaveFile(directory + "/versions/libraries/lastUpdates/" + GetLibName(instanceId, filesList.version) + ".lver", filesList.version.librariesLastUpdate.ToString()); 
            }

            //скачиваем assets
            if (!Directory.Exists(directory + "/assets"))
                Directory.CreateDirectory(directory + "/assets");

            if (updateList.AssetsObjects)
            {
                if (!Directory.Exists(directory + "/assets/objects"))
                    Directory.CreateDirectory(directory + "/assets/objects");

                try
                {
                    wc.DownloadFile(LaunсherSettings.serverUrl + "upload/assets/" + filesList.version.assetsVersion + "/objects.zip", directory + "/temp/objects.zip");

                    ZipFile.ExtractToDirectory(directory + "/temp/objects.zip", directory + "/assets/objects");
                    File.Delete(directory + "/temp/objects.zip");

                    //UpdateProgressBar();
                }
                catch
                {
                    errors.Add("asstes/objects");
                }
            }

            if (updateList.AssetsIndexes)
            {
                if (!Directory.Exists(directory + "/assets/indexes"))
                    Directory.CreateDirectory(directory + "/assets/indexes");

                wc.DownloadFile(filesList.version.assetsIndexes, directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json");

                try
                {
                    wc.DownloadFile(filesList.version.assetsIndexes, directory + "/assets/indexes/" + filesList.version.assetsVersion + ".json");
                    //UpdateProgressBar();
                }
                catch
                {
                    errors.Add("asstes/indexes");
                }
            }

            if (updateList.AssetsVirtual)
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

                    //UpdateProgressBar();

                }
                catch
                {
                    errors.Add("asstes/virtuals");
                }
            }

            wc.Dispose();

            //сохраняем lastUpdates
            SaveFile(directory + "/instances/" + instanceId + "/lastUpdates.json", JsonConvert.SerializeObject(updates));

            return errors;

        }

        public static List<string> UpdateVariableFiles(VariableFilesUpdates updatesList, NInstanceManifest filesList, string instanceId, ref Dictionary<string, int> updates)
        {
            WebClient wc = new WebClient();

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
                        addr = LaunсherSettings.serverUrl + "upload/modpacks/" + instanceId + "/" + dir + "/" + file;

                    }
                    else
                    {
                        addr = filesList.data[dir].objects[file].url;

                    }

                    if (!DownloadFile(addr, folders[folders.Length - 1], directory + "/instances/" + instanceId + "/" + dir + "/" + file, filesList.data[dir].objects[file].sha1, filesList.data[dir].objects[file].size, wc))
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

            return errors;

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

                    using (WebClient wc = new WebClient())
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

            }
            catch { }

        }

        public static ExportResult ExportInstance(string instanceId, List<string> directoryList, string exportFile, string description)
        {
            string targetDir = directory + "/temp/" + instanceId + "-export"; //временная папка, куда будем копировать все файлы
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
                ["name"] = UserData.InstancesList[instanceId].Name,
                ["author"] = UserData.login,
                ["forgeVersion"] = instanceFile.version.forgeVersion
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

            if (!instanceInfo.ContainsKey("forgeVersion") || string.IsNullOrEmpty(instanceInfo["forgeVersion"]))
            {
                instanceInfo["forgeVersion"] = "";
            }

            string instanceId = ManageLogic.CreateInstance(instanceInfo["name"], InstanceType.Local, instanceInfo["gameVersion"], instanceInfo["forgeVersion"]);
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

            if (Gui.Pages.Right.Menu.InstanceContainerPage.obj != null)
            {
                Uri logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
                Gui.Pages.Right.Menu.InstanceContainerPage.obj.BuildInstanceForm(instanceId, UserData.InstancesList.Count - 1, logoPath, UserData.InstancesList[instanceId].Name, "NightWorld", "test", new List<string>());
            }

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

        public static bool DownloadMod(int projectID, int fileID, string path)
        {
            return true;
            try
            {
                string answer;

                WebRequest req = WebRequest.Create("https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID + "/files");
                using (WebResponse resp = req.GetResponse())
                {
                    using (Stream stream = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            answer = sr.ReadToEnd();
                        }
                    }
                }

                if (answer == null)
                {
                    return false;
                }

                List<ModInfo> data = JsonConvert.DeserializeObject<List<ModInfo>>(answer);

                string fileUrl = "";
                string fileName = "";

                foreach (ModInfo v in data)
                {
                    if (v.id == fileID && !String.IsNullOrWhiteSpace(v.downloadUrl) && !String.IsNullOrWhiteSpace(v.fileName))
                    {
                        char[] invalidFileChars = Path.GetInvalidFileNameChars();
                        bool isInvalidFilename = invalidFileChars.Any(s => v.fileName.Contains(s));

                        if (isInvalidFilename)
                        {
                            continue;
                        }

                        fileUrl = v.downloadUrl;
                        fileName = v.fileName;
                        break;
                    }
                }

                if (fileUrl != "")
                {
                    using (WebClient wc = new WebClient())
                    {
                        DelFile(directory + "/temp/" + fileName);
                        wc.DownloadFile(fileUrl, directory + "/temp/" + fileName);
                        File.Move(directory + "/temp/" + fileName, path + fileName);
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static InstanceManifest DownloadCurseforgeInstance(string downloadUrl, string fileName, string instanceId, out List<string> errors)
        {
            errors = new List<string>();

            try
            {
                if (File.Exists(directory + "/temp/" + fileName))
                {
                    File.Delete(directory + "/temp/" + fileName);
                }

                using (WebClient wc = new WebClient())
                {
                    DelFile(directory + "/temp/" + fileName);
                    wc.DownloadFile(downloadUrl, directory + "/temp/" + fileName);
                }

                if (Directory.Exists(directory + "/temp/dataDownload"))
                {
                    Directory.Delete(directory + "/temp/dataDownload");
                }

                Directory.CreateDirectory(directory + "/temp/dataDownload");
                ZipFile.ExtractToDirectory(directory + "/temp/" + fileName, directory + "/temp/dataDownload");
                DelFile(directory + "/temp/" + fileName);

                InstanceManifest data = GetFile<InstanceManifest>(directory + "/temp/dataDownload/manifest.json");

                if (data != null)
                {
                    if (!Directory.Exists(directory + "/temp/dataDownload/overrides/mods"))
                    {
                        Directory.CreateDirectory(directory + "/temp/dataDownload/overrides/mods");
                    }

                    foreach (InstanceManifest.FileData file in data.files)
                    {
                        bool anw = DownloadMod(file.projectID, file.fileID, directory + "/temp/dataDownload/overrides/mods/");

                        //скачивание мода не удалось. Добавляем его данные в список ошибок
                        if (!anw)
                        {
                            errors.Add(file.projectID + " " + file.fileID);
                        }
                    }

                    string SourcePath = directory + "/temp/dataDownload/overrides/";
                    string DestinationPath = directory + "/instances/" + instanceId + "/";

                    foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
                    }

                    foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                    }

                    if (Directory.Exists(directory + "/temp/dataDownload"))
                    {
                        Directory.Delete(directory + "/temp/dataDownload", true);
                    }

                    return data;
                }

                if (Directory.Exists(directory + "/temp/dataDownload"))
                {
                    Directory.Delete(directory + "/temp/dataDownload", true);
                }

                errors.Add("curseforgeManifestError");

                return null;
            }
            catch
            {
                return null;
            }

        }

    }

}
