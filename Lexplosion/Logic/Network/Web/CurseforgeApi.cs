using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using System.IO.Compression;

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

        public static List<CurseforgeInstanceInfo> GetInstances(int pageSize, int index, int categoriy, string searchFilter = "", string gameVersion = "")
        {
            if (gameVersion != "")
            {
                gameVersion = "&gameVersion=" + gameVersion;
            }

            string url;
            if (categoriy == -1)
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
            string jsonContent = "{\"modIds\": [" + string.Join(",", ids) + "]}";

            try
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
            catch
            {
                return null;
            }
        }

        public static CurseforgeInstanceInfo GetInstance(string id)
        {
            try
            {
                return GetApiData<CurseforgeInstanceInfo>("https://api.curseforge.com/v1/mods/" + id + "/");
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

        public static List<CurseforgeCategory> GetCategories(CfProjectType type)
        {
            List<CurseforgeCategory> categories = GetApiData<List<CurseforgeCategory>>("https://api.curseforge.com/v1/categories?gameId=432&classId=" + (int)type);
            categories.Add(new CurseforgeCategory
            {
                id = -1,
                name = "All",
                iconUrl = null,
                classId = (int)type,
                parentCategoryId = (int)type
            });

            return categories;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValuePair<InstalledAddonInfo, DownloadAddonRes> InstallAddon(AddonType addonType, string fileUrl, string fileName, string path, string folderName, int projectID, int fileID, Action<int> percentHandler)
        {
            if (addonType != AddonType.Maps)
            {
                if (!WithDirectory.InstallFile(fileUrl, fileName, path + folderName, percentHandler))
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.DownloadError
                    };
                }

                Console.WriteLine("SYS " + fileUrl);
            }
            else
            {
                if (!WithDirectory.InstallZipContent(fileUrl, fileName, path + folderName, percentHandler))
                {

                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.DownloadError
                    };
                }

                Console.WriteLine("SYS " + fileUrl);
            }

            return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
            {
                Value1 = new InstalledAddonInfo
                {
                    ProjectID = projectID,
                    FileID = fileID,
                    Path = (addonType != AddonType.Maps) ? (folderName + "/" + fileName) : (folderName + "/"),
                    Type = addonType
                },
                Value2 = DownloadAddonRes.Successful
            };
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

                if (String.IsNullOrWhiteSpace(addonInfo.downloadUrl))
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.UrlError
                    };
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
                return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, percentHandler);
            }
            //catch
            //{
            //    return new Dictionary<string, (InstalledAddonInfo, DownloadAddonRes)>
            //    {
            //        [projectID.ToString()] = (null, DownloadAddonRes.UncnownError)
            //    };
            //}
        }

        public static ValuePair<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(CurseforgeAddonInfo addonInfo, int fileID, string path, Action<int> percentHandler)
        {
            //try
            {
                int projectID = addonInfo.id;
                Console.WriteLine("");
                Console.WriteLine("PR ID " + projectID);

                if (addonInfo.latestFiles == null)
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.ProjectDataError
                    };
                }

                // получем информацию о файле
                CurseforgeFileInfo fileData = null;
                //ищем нужный файл
                foreach (CurseforgeFileInfo data in addonInfo.latestFiles)
                {
                    if (data.id == fileID)
                    {
                        fileData = data;
                        break;
                    }
                }
                //не нашли, делаем дополнительный запрос и получаем его
                if (fileData == null)
                {
                    fileData = GetProjectFile(projectID.ToString(), fileID.ToString());
                }

                Console.WriteLine("fileData " + fileData.downloadUrl + " " + projectID.ToString() + " " + fileID.ToString());

                string fileUrl = fileData.downloadUrl;
                if (String.IsNullOrWhiteSpace(fileUrl))
                {
                    // пробуем второй раз
                    fileData = GetProjectFile(projectID.ToString(), fileID.ToString());
                    if (String.IsNullOrWhiteSpace(fileData.downloadUrl))
                    {
                        Console.WriteLine("URL ERROR - " + fileData.downloadUrl + " - " + fileData.fileName);
                        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.UrlError
                        };
                    }
                }     

                Console.WriteLine(fileUrl);

                string fileName = fileData.fileName;

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
                AddonType addonType = (AddonType)addonInfo.classId;
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
                return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, percentHandler);
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
