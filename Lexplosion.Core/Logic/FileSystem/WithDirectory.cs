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
using Lexplosion.Tools;
using static Lexplosion.Logic.FileSystem.DataFilesManager;

namespace Lexplosion.Logic.FileSystem
{
    public static class WithDirectory
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

                if (DirectoryPath[DirectoryPath.Length - 1] == '/')
                {
                    DirectoryPath.TrimEnd('/');
                }

                Directory.CreateDirectory(DirectoryPath + "/temp");
            }
            catch { }
        }

        public static void SetNewDirectory(string path)
        {
            string oldDir = DirectoryPath;
            Create(path);

            bool suuccessfull = false;

            try
            {
                foreach (string dirPath in Directory.GetDirectories(oldDir, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(oldDir, path));
                }

                foreach (string newPath in Directory.GetFiles(oldDir, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(oldDir, path), true);
                }

                suuccessfull = true;

            }
            catch { }

            try
            {
                if (suuccessfull)
                {
                    Directory.Delete(oldDir, true);
                }
            }
            catch { }
        }

        private static Random random = new Random();

        public static string CreateTempDir() // TODO: пр использовании этого метода разными потоками может создаться одна папка на два вызова. Так же сделать try
        {
            string dirName = DirectoryPath + "/temp";
            string dirName_ = dirName;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            do
            {
                dirName_ = dirName + "/" + new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
            } while (Directory.Exists(dirName_));

            Directory.CreateDirectory(dirName_);

            return (dirName_ + "/").Replace("//", "/");
        }

        /// <summary>
        /// Создает папку по указанному адресу.
        /// </summary>
        /// <param name="name">Адрес, по которому должна быть создана папка.</param>
        /// <returns>Вовзращает имя созданной папки. может отличаться от параметра name, ведь такая папка может уже существовтаь и нужно будет добавить символы в имя.</returns>
        public static string CreateFolder(string name)
        {
            try
            {
                string dirName = name, dirName_ = name;
                int i = 0;
                while (Directory.Exists(dirName))
                {
                    dirName = dirName_ + "_" + i;
                    i++;
                }

                Directory.CreateDirectory(dirName);

                return dirName;
            }
            catch
            {
                return null;
            }
        }

        public static bool InstallZipContent(string url, string fileName, string path, TaskArgs taskArgs)
        {
            path = DirectoryPath + "/" + path;
            string tempDir = CreateTempDir();
            if (!DownloadFile(url, fileName, tempDir, taskArgs))
            {
                return false;
            }

            try
            {
                string unzipPath = tempDir + "unzip/";
                Directory.CreateDirectory(unzipPath);
                ZipFile.ExtractToDirectory(tempDir + fileName, unzipPath);

                DirectoryInfo[] directories = (new DirectoryInfo(unzipPath)).GetDirectories();
                foreach (DirectoryInfo directoryInfo in directories)
                {
                    string dirName = directoryInfo.Name;
                    string resultFolder = CreateFolder(path + "/" + dirName);

                    foreach (string dirPath in Directory.GetDirectories(directoryInfo.FullName, "*", SearchOption.AllDirectories))
                        Directory.CreateDirectory(dirPath.Replace(directoryInfo.FullName, resultFolder));

                    foreach (string newPath in Directory.GetFiles(directoryInfo.FullName, "*.*", SearchOption.AllDirectories))
                        File.Copy(newPath, newPath.Replace(directoryInfo.FullName, resultFolder), true);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }

            return true;
        }

        public static bool InstallFile(string url, string fileName, string path, TaskArgs taskArgs)
        {
            Runtime.DebugWrite("INSTALL " + url);

            string tempDir = null;
            try
            {
                tempDir = CreateTempDir();
                if (!Directory.Exists(DirectoryPath + "/" + path))
                {
                    Directory.CreateDirectory(DirectoryPath + "/" + path);
                }

                if (DownloadFile(url, fileName, tempDir, taskArgs))
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
            catch (Exception ex)
            {
                if (tempDir != null)
                {
                    DelFile(tempDir + fileName);
                    DelFile(DirectoryPath + "/" + path + "/" + fileName);
                }

                Runtime.DebugWrite("Downloading error " + fileName + " " + url + " " + ex);

                return false;
            }
        }

        public static bool DownloadFile(string url, string fileName, string tempDir, TaskArgs taskArgs)
        {
            using (var webClient = new WebClient())
            {
                DelFile(tempDir + fileName);
                bool result = true;

                taskArgs.CancelToken.Register(webClient.CancelAsync);
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    taskArgs.PercentHandler(e.ProgressPercentage);
                };

                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    result = (e.Error == null);
                };

                try
                {
                    Task task = webClient.DownloadFileTaskAsync(url, tempDir + fileName);
                    task.Wait();

                    return result;
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("Downloading error " + fileName + " " + url + " " + ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Удаляет файл, если он существует.
        /// </summary>
        /// <param name="file">Имя файла.</param>
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
                    catch (Exception e)
                    {
                        Runtime.DebugWrite("FileCopyError exception " + e);
                        return ExportResult.FileCopyError;
                    }
                }
                else
                {
                    Runtime.DebugWrite("File not exists " + dirUnit);
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
                try { Directory.Delete(targetDir, true); } catch { }

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

        public static FileRecvResult ReceiveFile(FileReceiver reciver, out string file)
        {
            string tempDir = CreateTempDir();
            file = tempDir + "archive.zip";

            return reciver.StartDownload(file);
        }

        public static bool DonwloadJava(string javaName, string bitDepth, TaskArgs taskArgs)
        {
            string tempDir = CreateTempDir();
            string fileName = javaName + ".zip";

            try
            {
                if (!DownloadFile(LaunсherSettings.URL.JavaData + "download/windows/" + bitDepth + "/" + fileName + "?1", fileName, tempDir, taskArgs))
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
                        Directory.Delete(javaPath + javaName, true);
                    }
                }

                ZipFile.ExtractToDirectory(tempDir + fileName, javaPath);
            }
            catch
            {
                return false;
            }

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

        public static void DeleteInstance(string instanceId)
        {
            try
            {
                string path = DirectoryPath + "/instances/" + instanceId;
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch { }

            try
            {
                string path = DirectoryPath + "/instances-assets/" + instanceId;
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch { }
        }
    }
}
