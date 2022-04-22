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
        public static string DirectoryPath { get; private set; }

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

        public static string CreateTempDir() // TODO: пр использовании этого метода разными потоками может создаться одна папка на два вызова
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

        //функция для удаления файла при его существовании 
        public static void DelFile(string file)
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

        public static LastUpdates GetLastUpdates(string instanceId)
        {
            LastUpdates updates = new LastUpdates();

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
                        var data = JsonConvert.DeserializeObject<LastUpdates>(Encoding.UTF8.GetString(fileBytes));
                        if (data != null)
                        {
                            updates = data;
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
