using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using Lexplosion.Core.Logic.Objects;

namespace Lexplosion.Logic.Network.Web
{
    public static class CurseforgeApi
    {
        private const string Token = "$2a$10$Ky9zG9R9.ha.kf5BRrvwU..OGSvC0I2Wp56hgXI/4aRtGbizrm3we";

        private class DataContainer<T>
        {
            [JsonProperty("data")]
            public T Data;
            [JsonProperty("pagination")]
            public Pagination Paginator { get; set; }
        }

        private class Pagination
        {
            [JsonProperty("index")]
            public int Index { get; set; }
            [JsonProperty("pageSize")]
            public int PageSize { get; set; }
            [JsonProperty("resultCount")]
            public int ResultCount { get; set; }
            [JsonProperty("totalCount")]
            public int TotalCount { get; set; }
        }

        public class FingerprintSearchAnswer
        {
            public class SearchedFiles
            {
                public int id;
                public CurseforgeFileInfo file;
            }

            public List<SearchedFiles> exactMatches;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetApiData<T>(string url, out Pagination pagination) where T : new()
        {
            pagination = null;

            try
            {
                var headers = new Dictionary<string, string>()
                {
                    ["x-api-key"] = Token
                };

                string answer = ToServer.HttpGet(url, headers);
                if (answer != null)
                {
                    var data = JsonConvert.DeserializeObject<DataContainer<T>>(answer);
                    if (data == null) return new T();

                    pagination = data.Paginator;
                    return data.Data ?? new T();
                }

                return new T();
            }
            catch
            {
                return new T();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetApiData<T>(string url, string jsonInputData, out Pagination pagination) where T : new()
        {
            pagination = null;

            try
            {
                var headers = new Dictionary<string, string>()
                {
                    ["x-api-key"] = Token
                };

                string answer = ToServer.HttpPostJson(url, jsonInputData, out _, headers);
                if (answer != null)
                {
                    var data = JsonConvert.DeserializeObject<DataContainer<T>>(answer);
                    if (data == null) return new T();

                    pagination = data.Paginator;
                    return data.Data ?? new T();
                }

                return new T();
            }
            catch
            {
                return new T();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetApiData<T>(string url, string jsonInputData) where T : new() => GetApiData<T>(url, jsonInputData, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetApiData<T>(string url) where T : new() => GetApiData<T>(url, out _);


        public static CatalogResult<CurseforgeInstanceInfo> GetInstances(CurseforgeSearchParams searchParams)
        {
            string url = "https://api.curseforge.com/v1/mods/search?";

            if (!string.IsNullOrWhiteSpace(searchParams.SearchFilter))
            {
                url += "searchFilter=" + WebUtility.UrlEncode(searchParams.SearchFilter) + "&";
            }

            string gameVersion = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchParams.GameVersion))
            {
                gameVersion = "&gameVersion=" + searchParams.GameVersion;
            }

            string ctrs = string.Empty;
            foreach (var ct in searchParams.Categories)
            {
                if (ct.Id == "-1")
                {
                    ctrs = string.Empty;
                    break;
                }

                ctrs += ct.Id + ",";
            }

            string categoryStr = "&categoryIds=" + WebUtility.UrlEncode("[" + ctrs.RemoveLastChars(1) + "]");

            url += $"gameId=432&classId=4471&sortOrder=desc&pageSize={searchParams.PageSize}&index={searchParams.PageIndex}{gameVersion}{categoryStr}&sortField={(int)searchParams.SortField}";

            Runtime.DebugWrite(url);

            var result = GetApiData<List<CurseforgeInstanceInfo>>(url, out Pagination paginator);
            return new(result, paginator?.TotalCount ?? 1);
        }

        public static CatalogResult<CurseforgeAddonInfo> GetAddonsList(AddonType type, CurseforgeSearchParams searchParams)
        {
            string gameVersion = string.Empty;
            if (!string.IsNullOrWhiteSpace(searchParams.GameVersion))
            {
                gameVersion = "&gameVersion=" + searchParams.GameVersion;
            }

            string ctrs = string.Empty;
            foreach (var ct in searchParams.Categories)
            {
                if (ct.Id == "-1")
                {
                    ctrs = string.Empty;
                    break;
                }

                ctrs += ct.Id + ",";
            }

            string categoryStr = "&categoryIds=" + WebUtility.UrlEncode("[" + ctrs.RemoveLastChars(1) + "]");

            string _modloader = string.Empty;
            if (type == AddonType.Mods)
            {
                var modloadersList = searchParams.Modloaders.Select(x => x == Modloader.NeoForged.ToString() ? "NeoForge" : x);
                _modloader = "&modLoaderTypes=" + WebUtility.UrlEncode("[" + string.Join(",", modloadersList) + "]");
            }

            string url = "https://api.curseforge.com/v1/mods/search?gameId=432&sortOrder=desc&classId=" + (int)type + "&pageSize=" + searchParams.PageSize + "&index=" + searchParams.PageIndex + gameVersion + categoryStr + _modloader + "&sortField=" + (int)searchParams.SortField + "&searchFilter=" + WebUtility.UrlEncode(searchParams.SearchFilter);
            Runtime.DebugWrite(url);

            var result = GetApiData<List<CurseforgeAddonInfo>>(url, out Pagination paginator);
            return new(result, paginator?.TotalCount ?? 1);
        }

        public static List<CurseforgeFileInfo> GetProjectFiles(string projectId, string gameVersion, ClientType modloader)
        {
            string modloaderStr = "";
            if (modloader != ClientType.Vanilla)
            {
                modloaderStr = "&modLoaderType=" + (int)modloader;
            }

            // TODO: у курсфорджа ограничения на 50 файлов, поэтому нужный нам файл иногда может просто не найтись
            return GetApiData<List<CurseforgeFileInfo>>("https://api.curseforge.com/v1/mods/" + projectId + "/files?gameVersion=" + gameVersion + modloaderStr);
        }

        public static List<CurseforgeFileInfo> GetFilesFromFingerprints(List<string> fingerprint)
        {
            var jsonContent = "{\"fingerprints\": [" + string.Join(",", fingerprint) + "]}";

            var data = GetApiData<FingerprintSearchAnswer>("https://api.curseforge.com/v1/fingerprints/432", jsonContent);
            var result = new List<CurseforgeFileInfo>();
            if (data?.exactMatches != null)
            {
                foreach (var item in data?.exactMatches)
                {
                    result.Add(item.file);
                }
            }

            return result;
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

        public static List<CurseforgeAddonInfo> GetAddonsInfo(string[] ids)
        {
            string jsonContent = "{\"modIds\": [" + string.Join(",", ids) + "]}";

            var data = GetApiData<List<CurseforgeAddonInfo>>("https://api.curseforge.com/v1/mods", jsonContent);
            return data ?? new List<CurseforgeAddonInfo>();
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

        public static string GetProjectDescription(string projectId)
        {
            try
            {
                string result = ToServer.HttpGet($"https://api.curseforge.com/v1/mods/{projectId}/description", new Dictionary<string, string>()
                {
                    ["x-api-key"] = Token
                });

                if (result == null) return string.Empty;

                var data = JsonConvert.DeserializeObject<DataContainer<string>>(result);
                return data?.Data ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static List<CurseforgeCategory> GetCategories(CfProjectType type)
        {
            List<CurseforgeCategory> categories = GetApiData<List<CurseforgeCategory>>("https://api.curseforge.com/v1/categories?gameId=432&classId=" + (int)type);
            categories.Insert(0, new CurseforgeCategory
            {
                Id = "-1",
                Name = "All",
                ClassId = ((int)type).ToString(),
                ParentCategoryId = ((int)type).ToString()
            });

            return categories;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SetValues<InstalledAddonInfo, DownloadAddonRes> InstallAddon(AddonType addonType, string fileUrl, string fileName, string path, string folderName, string projectID, string fileID, TaskArgs taskArgs)
        {
            if (addonType != AddonType.Maps)
            {
                if (!WithDirectory.InstallFile(fileUrl, fileName, path + folderName, taskArgs))
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = taskArgs.CancelToken.IsCancellationRequested ? DownloadAddonRes.IsCanselled : DownloadAddonRes.DownloadError
                    };
                }

                Runtime.DebugWrite("SYS " + fileUrl);
            }
            else
            {
                if (!WithDirectory.InstallZipContent(fileUrl, fileName, path + folderName, taskArgs))
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = taskArgs.CancelToken.IsCancellationRequested ? DownloadAddonRes.IsCanselled : DownloadAddonRes.DownloadError
                    };
                }

                Runtime.DebugWrite("SYS " + fileUrl);
            }

            return new SetValues<InstalledAddonInfo, DownloadAddonRes>
            {
                Value1 = new InstalledAddonInfo
                {
                    ProjectID = projectID,
                    FileID = fileID,
                    Path = (addonType != AddonType.Maps) ? (folderName + "/" + fileName) : (folderName + "/"),
                    Type = addonType,
                    Source = ProjectSource.Curseforge

                },
                Value2 = DownloadAddonRes.Successful
            };
        }

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(CurseforgeFileInfo addonInfo, AddonType addonType, string path, TaskArgs taskArgs)
        {
            Runtime.DebugWrite("PR ID " + addonInfo.id);
            string projectID = addonInfo.modId;
            string fileID = addonInfo.id.ToString();
            try
            {
                Runtime.DebugWrite("fileData " + addonInfo.downloadUrl + " " + projectID + " " + fileID);

                string fileUrl = addonInfo.downloadUrl;
                string fileName = addonInfo.fileName;

                if (String.IsNullOrWhiteSpace(addonInfo.downloadUrl))
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.UrlError
                    };
                }

                Runtime.DebugWrite(fileUrl);

                // проверяем имя файла на валидность
                char[] invalidFileChars = Path.GetInvalidFileNameChars();
                bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

                if (isInvalidFilename)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
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
                    case AddonType.Shaders:
                        folderName = "shaderpacks";
                        break;
                    default:
                        return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.unknownAddonType
                        };
                }

                // устанавливаем
                return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, taskArgs);
            }
            catch
            {
                return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                {
                    Value1 = null,
                    Value2 = DownloadAddonRes.unknownError
                };
            }
        }

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(CurseforgeAddonInfo addonInfo, string fileID, string path, TaskArgs taskArgs)
        {
            try
            {
                string projectID = addonInfo.id;
                Runtime.DebugWrite("");
                Runtime.DebugWrite("PR ID " + projectID);

                if (addonInfo.latestFiles == null)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
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
                    if (data.id.ToString() == fileID)
                    {
                        fileData = data;
                        break;
                    }
                }
                //не нашли, делаем дополнительный запрос и получаем его
                if (fileData == null)
                {
                    fileData = GetProjectFile(projectID, fileID);
                }

                Runtime.DebugWrite("fileData " + fileData.downloadUrl + " " + projectID + " " + fileID);

                string fileUrl = fileData.downloadUrl;
                if (String.IsNullOrWhiteSpace(fileUrl))
                {
                    // пробуем второй раз
                    fileData = GetProjectFile(projectID, fileID);
                    if (String.IsNullOrWhiteSpace(fileData.downloadUrl))
                    {
                        Runtime.DebugWrite("URL ERROR - " + fileData.downloadUrl + " - " + fileData.fileName);
                        return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.UrlError
                        };
                    }
                }

                Runtime.DebugWrite(fileUrl);

                string fileName = fileData.fileName;

                // проверяем имя файла на валидность
                char[] invalidFileChars = Path.GetInvalidFileNameChars();
                bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

                if (isInvalidFilename)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.FileNameError
                    };
                }

                // определяем папку в которую будет установлен данный аддон
                string folderName = "";
                AddonType addonType = (AddonType)(addonInfo.classId ?? 0);
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
                    case AddonType.Shaders:
                        folderName = "shaderpacks";
                        break;
                    default:
                        return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                        {
                            Value1 = null,
                            Value2 = DownloadAddonRes.unknownAddonType
                        };
                }

                // устанавливаем
                return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, taskArgs);
            }
            catch
            {
                return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                {
                    Value1 = null,
                    Value2 = DownloadAddonRes.unknownError
                };
            }
        }
    }
}
