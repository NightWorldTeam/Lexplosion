using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Global;
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
            Console.WriteLine("INSTALL " + url);

            try
            {
                tempDir = CreateTempDir();

                if (!Directory.Exists(DirectoryPath + "/" + path))
                {
                    Directory.CreateDirectory(DirectoryPath + "/" + path);
                }

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(url, tempDir + fileName);
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                    File.Move((tempDir + fileName).Replace("/", "\\"), (DirectoryPath + "/" + path + "/" + fileName).Replace("/", "\\"));
                    Directory.Delete(tempDir, true);
                }

                Console.WriteLine("RETURN TRUE ");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(url + " " + ex);
                if (tempDir != null)
                {
                    DelFile(tempDir + fileName);
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                }

                return false;
            }
        }

        public static bool InstallFile(string url, string fileName, string path, Action<int> percentHandler)
        {
            Console.WriteLine("INSTALL " + url);

            string tempDir = null;
            //try
            {
                tempDir = CreateTempDir();
                if (!Directory.Exists(DirectoryPath + "/" + path))
                {
                    Directory.CreateDirectory(DirectoryPath + "/" + path);
                }

                if (DownloadFile(url, fileName, tempDir, percentHandler))
                {
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                    File.Move((tempDir + fileName).Replace("/", "\\"), (DirectoryPath + "/" + path + "/" + fileName).Replace("/", "\\"));
                    Directory.Delete(tempDir, true);
                    return true;
                }
                else
                {
                    DelFile(tempDir + fileName);
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                    return false;
                }
            }
            //catch
            //{
            //    if (tempDir != null)
            //    {
            //        DelFile(tempDir + fileName);
            //        DelFile(DirectoryPath + "/" + path + "/" + fileName);
            //    }

            //    return false;
            //}
        }

        public static bool DownloadFile(string url, string fileName, string tempDir)
        {
            //try
            {
                using (WebClient wc = new WebClient())
                {
                    DelFile(tempDir + fileName);
                    wc.DownloadFile(url, tempDir + fileName);
                }

                return true;
            }
            //catch
            //{
            //    return false;
            //}
        }

        public static bool DownloadFile(string url, string fileName, string tempDir, Action<int> percentHandler)
        {
            using (var webClient = new WebClient())
            {
                DelFile(tempDir + fileName);
                bool result = true;

                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    percentHandler(e.ProgressPercentage);
                };
                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    result = (e.Error == null);
                };

                //try
                {
                    Task task = webClient.DownloadFileTaskAsync(url, tempDir + fileName);
                    task.Wait();

                    return result;
                }
                //catch
                //{
                //    return false;
                //} 
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

        public static ExportResult ExportInstance<T>(string instanceId, List<string> filesList, string exportFile, T parameters, string logoPath = null)
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

            foreach (string dirUnit_ in filesList)
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

            if (logoPath != null)
            {
                try
                {
                    if (File.Exists(logoPath))
                    {
                        File.Copy(logoPath, targetDir + "/logo.png");
                    }
                }
                catch { }
            }

            string jsonData = JsonConvert.SerializeObject(parameters);
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
                DelFile(exportFile);
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

        public static ImportResult UnzipInstance<T>(string zipFile, out T parameters, out string resultingDirectory)
        {
            parameters = default(T);
            resultingDirectory = CreateTempDir() + "import/";

            if (!Directory.Exists(resultingDirectory))
            {
                try
                {
                    Directory.CreateDirectory(resultingDirectory);
                }
                catch
                {
                    return ImportResult.DirectoryCreateError;
                }
            }
            else
            {
                Directory.Delete(resultingDirectory, true);
            }

            try
            {
                ZipFile.ExtractToDirectory(zipFile, resultingDirectory);
            }
            catch
            {
                Directory.Delete(resultingDirectory, true);

                return ImportResult.ZipFileError;
            }

            parameters = GetFile<T>(resultingDirectory + "instanceInfo.json");

            return ImportResult.Successful;
        }

        public static ImportResult MoveUnpackedInstance(string instanceId, string unzipPath)
        {
            string addr = unzipPath + "files/";
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
                Directory.Delete(unzipPath, true);

                return ImportResult.MovingFilesError;
            }

            try
            {
                Directory.Delete(unzipPath, true);
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
                //MainWindow.Obj.Dispatcher.Invoke(delegate
                //{
                //    MainWindow.Obj.SetMessageBox("Произошла ошибка при удалении.");
                //});
            }

            //MainWindow.Obj.Dispatcher.Invoke(delegate
            //{
            //    //MainWindow.window.InitProgressBar.Visibility = Visibility.Collapsed;
            //});
        }

        public static bool DonwloadJava(string javaName, Action<int> percentHandler)
        {
            string tempDir = CreateTempDir();
            string fileName = javaName + ".zip";

            //try
            {
                if (!DownloadFile(LaunсherSettings.URL.JavaData + "download/" + fileName, fileName, tempDir, percentHandler))
                {
                    return false;
                }

                string javaPath = DirectoryPath + "/java/";
                if (!Directory.Exists(javaPath))
                {
                    Directory.CreateDirectory(javaPath);
                }
                else
                {
                    if (Directory.Exists(javaPath + javaName))
                    {
                        Directory.Delete(javaPath, true);
                    }
                }

                ZipFile.ExtractToDirectory(tempDir + fileName, javaPath);
            }
            //catch
            //{
            //    return false;
            //}

            try
            {
                Directory.Delete(tempDir, true);
            }
            catch { }

            return true;
        }

        public static List<byte[]> LoadMcScreenshots(string instanceId)
        {
            string[] files;
            List<byte[]> screenshot = new List<byte[]>();

            try
            {
                if (Directory.Exists(DirectoryPath + "/instances/" + instanceId + "/screenshots"))
                {
                    files = Directory.GetFiles(DirectoryPath + "/instances/" + instanceId + "/screenshots");
                }
                else
                {
                    return screenshot;
                }
            }
            catch
            {
                return screenshot;
            }

            try
            {
                foreach (string file in files)
                {
                    using (FileStream fstream = File.OpenRead(file))
                    {
                        byte[] fileBytes = new byte[fstream.Length];
                        fstream.Read(fileBytes, 0, fileBytes.Length);
                        fstream.Close();

                        screenshot.Add(fileBytes);
                    }
                }

                return screenshot;
            }
            catch
            {
                return screenshot;
            }
        }
    }
}
