using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.FileSystem
{
	public class WithDirectory
	{
		// TODO: во всём WithDirectory я заменяю элементы адресов директорий через replace. Не знаю как на винде, но на линуксе могут появиться проблемы, ведь replace заменяет подстроки в строке, а не только конечную подстроку
		public string DirectoryPath { get; private set; }
		public string InstancesPath { get => $"{DirectoryPath}/instances/"; }
		public string GetInstancePath(string instanceId) => $"{InstancesPath}{instanceId}/";

		private HttpClient _httpClient = new();

		internal WithDirectory()
		{
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
		}

		public void Create(string path)
		{
			try
			{
				path = path.Replace(@"\", "/");
				if (path[path.Length - 1] == '/')
				{
					path = path.TrimEnd('/');
				}

				DirectoryPath = path;

				if (Directory.Exists(DirectoryPath + "/temp"))
				{
					Directory.Delete(DirectoryPath + "/temp", true);
				}

				Runtime.DebugWrite("DirectoryPath: " + DirectoryPath);
				Directory.CreateDirectory(DirectoryPath + "/temp");
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("path: " + path);
				Runtime.DebugWrite("Exception: " + ex);
			}
		}

		public void SetNewDirectory(string path)
		{
			string oldDir = DirectoryPath;
			Create(path);

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
			}
			catch { }
		}

		/// <summary>
		/// Определяет допустимую директорию для хранения файлов, на основе директори path.
		/// </summary>
		/// <param name="path">Директория, в которой должна быть создана папка для хранения файлов</param>
		/// <returns>
		/// Если внутри path нету папки lexplosion, то будет возвращена path/lexplosion.
		/// Если есть, то будет добавлена номерная метка (например path/lexplosion_1)
		/// </returns>
		public string CreateValidPath(string path)
		{
			path += "/" + LaunсherSettings.GAME_FOLDER_NAME;
			string path_ = path;
			int i = 1;
			while (Directory.Exists(path_))
			{
				path_ = path + "_" + i;
				i++;
			}

			return path_;
		}

		private Random random = new Random();

		public string CreateTempDir() // TODO: пр использовании этого метода разными потоками может создаться одна папка на два вызова. Так же сделать try
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
		public string CreateFolder(string name)
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

		public bool InstallZipContent(string url, string fileName, string path, TaskArgs taskArgs)
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

		public bool InstallFile(string url, string fileName, string path, TaskArgs taskArgs)
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

				Runtime.DebugWrite($"Downloading error fileName: {fileName}, path: {path}, gamePath: {DirectoryPath}, url: {url}, Exception:" + ex);

				return false;
			}
		}

		private async Task<bool> DownloadFileAsync(string url, string savePath, TaskArgs taskArgs)
		{
			try
			{
				using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, taskArgs.CancelToken))
				{
					response.EnsureSuccessStatusCode();

					long? contentLength = response.Content.Headers.ContentLength;

					using (var stream = await response.Content.ReadAsStreamAsync())
					{
						using (var fileStream = File.Create(savePath))
						{
							byte[] buffer = new byte[16384];
							long bytesRead = 0;
							int bytesReadTotal = 0;

							int bytesReadThisTime;
							while ((bytesReadThisTime = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
							{
								await fileStream.WriteAsync(buffer, 0, bytesReadThisTime);
								bytesRead += bytesReadThisTime;
								bytesReadTotal += bytesReadThisTime;

								if (contentLength.HasValue)
								{
									double percentage = ((double)bytesRead) / contentLength.Value * 100;
									taskArgs.PercentHandler((int)percentage);
								}
							}

							taskArgs.PercentHandler(100);
						}

						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Downloading error " + savePath + " " + url + " " + ex);
				return false;
			}
		}

		public bool DownloadFile(string url, string fileName, string tempDir, TaskArgs taskArgs)
		{
			DelFile(tempDir + fileName);
			return DownloadFileAsync(url, tempDir + fileName, taskArgs).Result;
		}

		/// <summary>
		/// Удаляет файл, если он существует.
		/// </summary>
		/// <param name="file">Имя файла.</param>
		public void DelFile(string file)
		{
			try
			{
				if (File.Exists(file))
				{
					File.Delete(file);
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception: " + ex);
			}
		}

		public ExportResult ExportInstance(string instanceId, List<string> filesList, string exportFile, string infoFileContent, string logoPath = null)
		{
			// TODO: удалять временную папку в конце
			string targetDir = CreateTempDir() + instanceId + "-export"; //временная папка, куда будем копировать все файлы
			string srcDir = InstancesPath + instanceId;

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
					if (File.Exists(logoPath)) File.Copy(logoPath, targetDir + "/logo.png");
				}
				catch { }
			}

			try
			{
				File.WriteAllText(targetDir + "/instanceInfo.json", infoFileContent);
			}
			catch
			{
				try { Directory.Delete(targetDir, true); } catch { }
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

		public ImportResult UnzipInstance(string zipFile, out string resultingDirectory)
		{
			resultingDirectory = CreateTempDir() + "import/";

			try
			{
				if (!Directory.Exists(resultingDirectory))
				{
					Directory.CreateDirectory(resultingDirectory);
				}
				else
				{
					Directory.Delete(resultingDirectory, true);
				}
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				return ImportResult.DirectoryCreateError;
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

			return ImportResult.Successful;
		}

		public ImportResult MoveUnpackedInstance(string instanceId, string unzipPath)
		{
			string addr = unzipPath + "files/";
			string targetDir = InstancesPath + instanceId + "/";

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
			catch (Exception ex)
			{
				try
				{
					Directory.Delete(unzipPath, true);
				}
				catch { }
				Runtime.DebugWrite("Exception " + ex);

				return ImportResult.MovingFilesError;
			}

			try
			{
				Directory.Delete(unzipPath, true);
			}
			catch { }

			return ImportResult.Successful;
		}

		public FileRecvResult ReceiveFile(FileReceiver reciver, out string file)
		{
			string tempDir = CreateTempDir();
			file = tempDir + "archive.zip";

			return reciver.StartDownload(file);
		}

		public List<byte[]> LoadMcScreenshots(string instanceId)
		{
			string[] files;
			List<byte[]> screenshot = new List<byte[]>();

			try
			{
				if (Directory.Exists(InstancesPath + instanceId + "/screenshots"))
				{
					files = Directory.GetFiles(InstancesPath + instanceId + "/screenshots");
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

		public void DeleteInstance(string instanceId)
		{
			try
			{
				string path = InstancesPath + instanceId;
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
