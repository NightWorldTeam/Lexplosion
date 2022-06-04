using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;

namespace Lexplosion.Logic.Network
{
    static class CurseforgeApi
    {
        public enum DownloadAddonRes
        {
            Successful,
            ProjectIdError,
            FileIdError,
            DownloadError,
            UncnownAddonType,
            FileVersionError,
            UrlError,
            FileNameError,
            UncnownError
        }

        public static List<CurseforgeInstanceInfo> GetInstances(int pageSize, int index, ModpacksCategories categoriy, string searchFilter = "", string gameVersion = "")
        {
            try
            {
                if (gameVersion != "")
                {
                    gameVersion = "&gameVersion=" + gameVersion;
                }

                string url;
                if (categoriy == ModpacksCategories.All)
                {
                    url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=4471&pageSize=" + pageSize + "&index=" + index + gameVersion + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
                }
                else
                {
                    url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=4471&pageSize=" + pageSize + "&index=" + index + gameVersion + "&categoryId=" + ((int)categoriy) + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
                }
                string answer;

                WebRequest req = WebRequest.Create(url);
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<List<CurseforgeInstanceInfo>>(answer);
                }

                return new List<CurseforgeInstanceInfo>();
            }
            catch
            {
                return new List<CurseforgeInstanceInfo>();
            }
        }

        public static List<CurseforgeModInfo> GetAddonsList(int pageSize, int index, AddonType type, string searchFilter = "", string gameVersion = "")
        {
            try
            {
                if (gameVersion != "")
                {
                    gameVersion = "&gameVersion=" + gameVersion;
                }

                string url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=" + (int)type + "&pageSize=" + pageSize + "&index=" + index + gameVersion + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);

                string answer;
                WebRequest req = WebRequest.Create(url);
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<List<CurseforgeModInfo>>(answer);
                }

                return new List<CurseforgeModInfo>();
            }
            catch
            {
                return new List<CurseforgeModInfo>();
            }
        }

        public static List<CurseforgeFileInfo> GetInstanceInfo(string id)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create("https://addons-ecs.forgesvc.net/api/v2/addon/" + id + "/files");
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<List<CurseforgeFileInfo>>(answer);
                }
                else
                {
                    return new List<CurseforgeFileInfo>();
                }
            }
            catch
            {
                return new List<CurseforgeFileInfo>();
            }
        }

        public static CurseforgeFileInfo GetInstanceInfo(string id, int fileId)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create("https://addons-ecs.forgesvc.net/api/v2/addon/" + id + "/file/" + fileId);
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<CurseforgeFileInfo>(answer);
                }
                else
                {
                    return new CurseforgeFileInfo();
                }
            }
            catch
            {
                return new CurseforgeFileInfo();
            }
        }

        public static CurseforgeInstanceInfo GetInstance(string id)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create("https://addons-ecs.forgesvc.net/api/v2/addon/" + id + "/");
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

                if (answer != null)
                {
                    var data = JsonConvert.DeserializeObject<CurseforgeInstanceInfo>(answer);

                    if (data.latestFiles != null && data.latestFiles.Count > 0)
                    {
                        long maxId = data.latestFiles[0].id;
                        foreach (var value in data.latestFiles)
                        {
                            if (value.id > maxId || data.Modloader == ModloaderType.None)
                            {
                                if (value.gameVersion.Contains("Forge"))
                                {
                                    data.Modloader = ModloaderType.Forge;
                                }
                                else if (value.gameVersion.Contains("Fabric"))
                                {
                                    data.Modloader = ModloaderType.Fabric;
                                }

                                if (value.id > maxId)
                                {
                                    maxId = value.id;
                                }
                            }
                        }
                    }

                    return data;
                }
                else
                {
                    Console.WriteLine("null");
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private class ProjectTypeInfo
        {
            public class CategorySection
            {
                public int packageType;
            }

            public class FileData
            {
                public int id;
                public string fileName;
                public string downloadUrl;
                public string displayName;
                public List<Dictionary<string, int>> dependencies;
            }

            public class LatestFile
            {
                public int projectFileId;
                public string gameVersion;
            }

            public CategorySection categorySection;
            public List<LatestFile> gameVersionLatestFiles;
        }

        public static Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)> DownloadAddon(int projectID, int fileID, string path, bool downloadDependencies = false, string gameVersion = "")
        {
            Console.WriteLine("");
            Console.WriteLine("PR ID " + projectID);
            var addonsList = new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>();
            try
            {
                string answer = ToServer.HttpGet("https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID);
                if (answer == null)
                {
                    return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                    {
                        [projectID.ToString()] = (null, DownloadAddonRes.ProjectIdError)
                    };
                }

                ProjectTypeInfo data = JsonConvert.DeserializeObject<ProjectTypeInfo>(answer);

                if (fileID == -1) // если fileID == -1 значит нужно установить последнюю версию аддона
                {
                    // TODO: тут еещ учитывать тип модлоадера
                    foreach (var latestFile in data.gameVersionLatestFiles) // ищем последнюю версию мода для данной версии майнкрафта
                    {
                        // TODO: там вроде был параметр defaultFileId, можно его заюзать
                        if (latestFile.gameVersion == gameVersion)
                        {
                            fileID = latestFile.projectFileId;
                        }
                    }

                    if (fileID == -1)
                    {
                        return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                        {
                            [projectID.ToString()] = (null, DownloadAddonRes.FileIdError)
                        };
                    }
                }

                // получем информацию о файле
                answer = ToServer.HttpGet("https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID + "/file/" + fileID);
                ProjectTypeInfo.FileData fileData;
                if (answer == null)
                {
                    Console.WriteLine("GFDSGFSHGFHGFHGFHFG " + "https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID + "/file/" + fileID + "/download-url");
                    answer = ToServer.HttpGet("https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID + "/file/" + fileID + "/download-url");
                    Console.WriteLine("ANSWER " + answer);

                    if (answer == null)
                    {
                        return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                        {
                            [projectID.ToString()] = (null, DownloadAddonRes.ProjectIdError)
                        };
                    }

                    fileData = new ProjectTypeInfo.FileData
                    {
                        dependencies = new List<Dictionary<string, int>>(),
                        downloadUrl = answer,
                        fileName = Path.GetFileName(answer)
                    };
                }
                else
                {
                    fileData = JsonConvert.DeserializeObject<ProjectTypeInfo.FileData>(answer);
                }

                Console.WriteLine("fileData " + fileData.downloadUrl);

                if (!String.IsNullOrWhiteSpace(fileData.downloadUrl) && !String.IsNullOrWhiteSpace(fileData.fileName))
                {
                    // проверяем имя файла на валидность

                    char[] invalidFileChars = Path.GetInvalidFileNameChars();
                    bool isInvalidFilename = invalidFileChars.Any(s => fileData.fileName.Contains(s));

                    if (isInvalidFilename)
                    {
                        return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                        {
                            [projectID.ToString()] = (null, DownloadAddonRes.FileNameError)
                        };
                    }

                    string fileUrl = fileData.downloadUrl;
                    string fileName = fileData.fileName;

                    Console.WriteLine("dependencies.Count " + fileData.dependencies.Count);

                    // скачиваем связанные файлы, если это нужно
                    if (downloadDependencies && fileData.dependencies.Count > 0)
                    {
                        foreach (Dictionary<string, int> value in fileData.dependencies)
                        {
                            if (value.ContainsKey("type") && value["type"] == 3 && value.ContainsKey("addonId"))
                            {
                                Console.WriteLine("download " + value["addonId"]);
                                Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)> addonsList_ = DownloadAddon(value["addonId"], -1, path, true, gameVersion);
                                foreach (string file in addonsList_.Keys)
                                {
                                    addonsList[file] = addonsList_[file];
                                }
                            }
                        }
                    }

                    // определяем папку в которую будет установлен данный аддон
                    string folderName = "";
                    AddonType addonType = (AddonType)data.categorySection.packageType;
                    switch (addonType)
                    {
                        case AddonType.Mod:
                            folderName = "mods";
                            break;
                        case AddonType.Maps:
                            folderName = "saves";
                            break;
                        case AddonType.Resourcepacks:
                            folderName = "resourcepacks";
                            break;
                        case AddonType.Unknown:
                            return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                            {
                                [fileName] = (null, DownloadAddonRes.UncnownAddonType)
                            };
                    }

                    // устанавливаем
                    if (WithDirectory.InstallFile(fileUrl, fileName, path + folderName))
                    {
                        Console.WriteLine("SYS " + fileUrl);
                        addonsList[fileName] = (new InstalledAddonInfo
                        {
                            ProjectID = projectID,
                            FileID = fileID,
                            Path = folderName + "/" + fileName
                        }, DownloadAddonRes.Successful);

                        return addonsList;
                    }
                    else
                    {
                        return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                        {
                            [fileName] = (null, DownloadAddonRes.DownloadError)
                        };
                    }
                }
                else
                {
                    return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                    {
                        [projectID.ToString()] = (null, DownloadAddonRes.UrlError)
                    };
                }

            }
            catch
            {
                return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
                {
                    [projectID.ToString()] = (null, DownloadAddonRes.UncnownError)
                };
            }
        }
    }
}
