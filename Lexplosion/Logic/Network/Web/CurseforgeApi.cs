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
using Lexplosion.Tools;

namespace Lexplosion.Logic.Network
{
    static class CurseforgeApi
    {
        private const string Token = "$2a$10$Ky9zG9R9.ha.kf5BRrvwU..OGSvC0I2Wp56hgXI/4aRtGbizrm3we";
        
        private class ProjectTypeInfo
        {
            public class LatestFile
            {
                public long id;
                public List<string> gameVersions;
            }

            public int classId;
            public List<LatestFile> latestFiles;
        }

        class DataContainer<T>
        {
            public T data;
        }

        private static T GetApiData<T>(string url) where T : new()
        {
            try
            {

                List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                headers.Add(new KeyValuePair<string, string>("x-api-key", Token));
                string answer = ToServer.HttpGet(url, headers);

                if (answer != null)
                {
                    var data = JsonConvert.DeserializeObject<DataContainer<T>>(answer).data;
                    return data ?? new T();
                }

                return new T();
            }
            catch (Exception e)
            {
                Console.WriteLine(url + " " + e);
                return new T();
            }
        }

        public static List<CurseforgeInstanceInfo> GetInstances(int pageSize, int index, ModpacksCategories categoriy, string searchFilter = "", string gameVersion = "")
        {
            if (gameVersion != "")
            {
                gameVersion = "&gameVersion=" + gameVersion;
            }

            string url;
            if (categoriy == ModpacksCategories.All)
            {
                url = "https://api.curseforge.com/v1/mods/search?gameId=432&classId=4471&sortField=1&sortOrder=desc&pageSize=" + pageSize + "&index=" + index + gameVersion + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
            }
            else
            {
                url = "https://api.curseforge.com/v1/mods/search?gameId=432&classId=4471&&sortField=1&sortOrder=desc&pageSize=" + pageSize + "&index=" + index + gameVersion + "&categoryId=" + ((int)categoriy) + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
            }

            return GetApiData<List<CurseforgeInstanceInfo>>(url);
        }

        public static List<CurseforgeAddonInfo> GetAddonsList(int pageSize, int index, AddonType type, int category, ModloaderType modloader, string searchFilter = "", string gameVersion = "")
        {
            if (gameVersion != "")
            {
                gameVersion = "&gameVersion=" + gameVersion;
            }

            string categoryStr = "";
            if (category != -1)
            {
                categoryStr = "&categoryId=" + category;
            }

            string url = "https://api.curseforge.com/v1/mods/search?gameId=432&sortField=1&sortOrder=desc&classId=" + (int)type + "&pageSize=" + pageSize + "&index=" + index + gameVersion + categoryStr + "&modLoaderType=" + (int)modloader + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
            return GetApiData<List<CurseforgeAddonInfo>>(url);
        }

        public static List<CurseforgeFileInfo> GetProjectFiles(string projectId, string gameVersion, ModloaderType modloader)
        {
            // TODO: у курсфорджа ограничения на 50 файлов, поэтому нужный нам файл иногда может просто не найтись
            return GetApiData<List<CurseforgeFileInfo>>("https://api.curseforge.com/v1/mods/" + projectId + "/files?gameVersion=" + gameVersion + "&modLoaderType=" + (int)modloader);
        }

        public static List<CurseforgeFileInfo> GetProjectFiles(string projectId)
        {
            return GetApiData<List<CurseforgeFileInfo>>("https://api.curseforge.com/v1/mods/" + projectId + "/files");
        }

        public static CurseforgeFileInfo GetProjectFile(string projecrId, string fileId)
        {
            return GetApiData<CurseforgeFileInfo>("https://api.curseforge.com/v1/mods/" + projecrId + "/files/" + fileId);
        }

        public static CurseforgeAddonInfo GetAddonInfo(string id)
        {
            return GetApiData<CurseforgeAddonInfo>("https://api.curseforge.com/v1/mods/" + id + "/");
        }

        public static List<CurseforgeAddonInfo> GetAddonsInfo(int[] ids)
        {
            Console.WriteLine("GetAddonsInfo");
            string jsonContent = "{\"modIds\": ["+string.Join(",", ids) +"]}";

            //try
            {
                WebRequest req = WebRequest.Create("https://api.curseforge.com/v1/mods");
                req.Method = "POST";
                req.Headers.Add("x-api-key", Token);
                req.ContentType = "application/json";

                byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
                req.ContentLength = byteArray.Length;

                using (Stream dataStream = req.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                string answer;
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

                Console.WriteLine("End GetAddonsInfo");

                var data = JsonConvert.DeserializeObject<DataContainer<List<CurseforgeAddonInfo>>>(answer);

                return data.data;
            }
            //catch
            //{
            //    successful = false;
            //    return null;
            //}
        }

        public static CurseforgeInstanceInfo GetInstance(string id)
        {
            try
            {
                var data = GetApiData<CurseforgeInstanceInfo>("https://api.curseforge.com/v1/mods/" + id + "/");

                if (data.latestFiles != null && data.latestFiles.Count > 0)
                {
                    long maxId = data.latestFiles[0].id;
                    foreach (var value in data.latestFiles)
                    {
                        if (value.id > maxId || data.ModloaderType == ModloaderType.None)
                        {
                            if (value.gameVersion != null)
                            {
                                if (value.gameVersion.Contains("Forge"))
                                {
                                    data.ModloaderType = ModloaderType.Forge;
                                }
                                else if (value.gameVersion.Contains("Fabric"))
                                {
                                    data.ModloaderType = ModloaderType.Fabric;
                                }
                                else if (value.gameVersion.Contains("Quilt"))
                                {
                                    data.ModloaderType = ModloaderType.Quilt;
                                }
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
            catch
            {
                return new CurseforgeInstanceInfo();
            }
        }

        public static string GetProjectChangelog(string projectID, string fileID)
        {
            //return ToServer.HttpGet("https://api.curseforge.com/v1/mods/" + projectID + "/files/" + fileID + "/changelog");
            // TODO: придумать как эту хуйню красиво сделать
            return "";
        }

        public static ValuePair<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(CurseforgeFileInfo addonInfo, AddonType addonType, string path, Action<int> percentHandler)
        {
            //Console.WriteLine("");
            Console.WriteLine("PR ID " + addonInfo.id);
            int projectID = addonInfo.modId;
            int fileID = addonInfo.id;
            //try
            {

                Console.WriteLine("fileData " + addonInfo.downloadUrl + " " + projectID.ToString() + " " + fileID.ToString());

                string fileUrl = addonInfo.downloadUrl;
                string fileName = addonInfo.fileName;
                // т.к разрабы курсфорджа дефектные рукодопы и конченные недоумки, которые не умеют писать код, то url иногда может быть null, поэтому придётся мутить костыли
                if (String.IsNullOrWhiteSpace(addonInfo.downloadUrl))
                {
                    if (!String.IsNullOrWhiteSpace(addonInfo.fileName))
                    {
                        // ручками формируем url
                        fileUrl = "https://edge.forgecdn.net/files/" + (addonInfo.id / 1000) + "/" + (addonInfo.id % 1000) + "/" + addonInfo.fileName;
                    }
                    else
                    {
                        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.UrlError
                        };
                    }
                }

                Console.WriteLine(fileUrl);

                // проверяем имя файла на валидность
                char[] invalidFileChars = Path.GetInvalidFileNameChars();
                bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

                if (isInvalidFilename)
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.FileNameError
                    };
                }

                // определяем папку в которую будет установлен данный аддон
                string folderName = "";
                switch (addonType)
                {
                    case AddonType.Mods:
                        folderName = "mods";
                        break;
                    case AddonType.Maps:
                        folderName = "saves";
                        break;
                    case AddonType.Resourcepacks:
                        folderName = "resourcepacks";
                        break;
                    case AddonType.Unknown:
                        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.UncnownAddonType
                        };
                }

                // устанавливаем
                if (WithDirectory.InstallFile(fileUrl, fileName, path + folderName, percentHandler))
                {
                    Console.WriteLine("SYS " + fileUrl);

                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = new InstalledAddonInfo
                        {
                            ProjectID = projectID,
                            FileID = fileID,
                            Path = folderName + "/" + fileName,
                            Type = addonType
                        },
                        Value2 = DownloadAddonRes.Successful
                    };
                }
                else
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.DownloadError
                    };
                }

            }
            //catch
            //{
            //    return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
            //    {
            //        [projectID.ToString()] = (null, DownloadAddonRes.UncnownError)
            //    };
            //}
        }

        public static ValuePair<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(int projectID, int fileID, string path)
        {
            Console.WriteLine("");
            Console.WriteLine("PR ID " + projectID);
            //try
            {
                ProjectTypeInfo data = GetApiData<ProjectTypeInfo>("https://api.curseforge.com/v1/mods/" + projectID + "/");
                if (data.classId == 0 || data.latestFiles == null)
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.ProjectIdError
                    };
                }

                // получем информацию о файле
                CurseforgeFileInfo fileData = GetProjectFile(projectID.ToString(), fileID.ToString());

                Console.WriteLine("fileData " + fileData.downloadUrl + " " + projectID.ToString() + " " + fileID.ToString());

                string fileUrl = fileData.downloadUrl;
                string fileName = fileData.fileName;
                // т.к разрабы курсфорджа дефектные рукодопы и конченные недоумки, которые не умеют писать код, то url иногда может быть null, поэтому придётся мутить костыли
                if (String.IsNullOrWhiteSpace(fileData.downloadUrl))
                {
                    if (!String.IsNullOrWhiteSpace(fileData.fileName))
                    {
                        // ручками формируем url
                        fileUrl = "https://edge.forgecdn.net/files/" + (fileData.id / 1000) + "/" + (fileData.id % 1000) + "/" + fileData.fileName;
                    }
                    else
                    {
                        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.UrlError
                        };
                    }
                }

                Console.WriteLine(fileUrl);

                // проверяем имя файла на валидность
                char[] invalidFileChars = Path.GetInvalidFileNameChars();
                bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

                if (isInvalidFilename)
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.FileNameError
                    };
                }

                // определяем папку в которую будет установлен данный аддон
                string folderName = "";
                AddonType addonType = (AddonType)data.classId;
                switch (addonType)
                {
                    case AddonType.Mods:
                        folderName = "mods";
                        break;
                    case AddonType.Maps:
                        folderName = "saves";
                        break;
                    case AddonType.Resourcepacks:
                        folderName = "resourcepacks";
                        break;
                    case AddonType.Unknown:
                        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.UncnownAddonType
                        };
                }

                // устанавливаем
                if (WithDirectory.InstallFile(fileUrl, fileName, path + folderName))
                {
                    Console.WriteLine("SYS " + fileUrl);
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = new InstalledAddonInfo
                        {
                            ProjectID = projectID,
                            FileID = fileID,
                            Path = folderName + "/" + fileName,
                            Type = addonType
                        },
                        Value2 = DownloadAddonRes.Successful
                    };
                }
                else
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.DownloadError
                    };
                }
            }
            //catch
            //{
            //    return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
            //    {
            //        [projectID.ToString()] = (null, DownloadAddonRes.UncnownError)
            //    };
            //}
        }
    }
}
