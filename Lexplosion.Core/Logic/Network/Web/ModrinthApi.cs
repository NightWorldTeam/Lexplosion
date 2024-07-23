using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Network.Web
{
    public static class ModrinthApi
    {
        public struct SearchFilters
        {
            public const string Relevance = "relevance";
            public const string Downloads = "downloads";
            public const string Newest = "newest";
            public const string Updated = "updated";
            public const string Follows = "follows";
        }

        private class CtalogContainer
        {
            public List<ModrinthCtalogUnit> hits;

            [JsonProperty("total_hits")]
            public int TotalHits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetApiData<T>(string url) where T : new()
        {
            try
            {
                string answer = ToServer.HttpGet(url);
                if (answer != null)
                {
                    var data = JsonConvert.DeserializeObject<T>(answer);
                    return data ?? new T();
                }

                return new T();
            }
            catch
            {
                return new T();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetApiData<T, U>(string url, U inputData) where T : new()
        {
            try
            {
                string answer = ToServer.HttpPostJson(url, JsonConvert.SerializeObject(inputData), out _);
                if (answer != null)
                {
                    var data = JsonConvert.DeserializeObject<T>(answer);
                    return data ?? new T();
                }

                return new T();
            }
            catch
            {
                return new T();
            }
        }

        public static List<ModrinthTeam> GetTeam(string teamId)
        {
            return GetApiData<List<ModrinthTeam>>("https://api.modrinth.com/v2/team/" + teamId + "/members");
        }

        public static List<ModrinthCategory> GetCategories()
        {
            return GetApiData<List<ModrinthCategory>>("https://api.modrinth.com/v2/tag/category");
        }

        public static ModrinthProjectInfo GetProject(string projectId)
        {
            return GetApiData<ModrinthProjectInfo>("https://api.modrinth.com/v2/project/" + projectId);
        }

        public static List<ModrinthProjectFile> GetProjectFiles(string projectId)
        {
            return GetApiData<List<ModrinthProjectFile>>("https://api.modrinth.com/v2/project/" + projectId + "/version");
        }

        public static List<ModrinthProjectFile> GetProjectFiles(string projectId, ClientType modloader, string gameVersion)
        {
            string param = "?game_versions=" + WebUtility.UrlEncode($"[\"{gameVersion}\"]");
            if (modloader != ClientType.Vanilla)
            {
                param += "&loaders=" + WebUtility.UrlEncode($"[\"{modloader.ToString().ToLower()}\"]");
            }
            string url = "https://api.modrinth.com/v2/project/" + projectId + "/version" + param;
            return GetApiData<List<ModrinthProjectFile>>(url);
        }

        public static ModrinthProjectFile GetProjectFile(string fileId)
        {
            return GetApiData<ModrinthProjectFile>("https://api.modrinth.com/v2/version/" + fileId);
        }

        public static List<ModrinthProjectFile> GetFilesFromHash(string hash)
        {
            return GetApiData<List<ModrinthProjectFile>>("https://api.modrinth.com/v2/version_file/" + hash + "?algorithm=sha512&multiple=true");
        }

        private class HashesContainer
        {
            public List<string> hashes;
            public string algorithm;
        }

        /// <summary>
        /// Возврщает файлы Modrinth по списку хэшей файлов
        /// </summary>
        /// <param name="hashes">Хэши</param>
        /// <returns>Ключ - хэш, значение - Modrinth файл, которому принадлежит этот хэш</returns>
        public static Dictionary<string, ModrinthProjectFile> GetFilesFromHashes(List<string> hashes)
        {
            return GetApiData<Dictionary<string, ModrinthProjectFile>, HashesContainer>("https://api.modrinth.com/v2/version_files", new HashesContainer
            {
                hashes = hashes,
                algorithm = "sha512"
            });
        }

        public static List<ModrinthProjectInfo> GetProjects(string[] ids)
        {
            // не понятно на кой хуй modrinth сделал это через get запрос. Максимальные размер url 1024,
            // в него все айдишники могут не вместится поэтому мутим костыли.
            // Если айдишников слишком много, то делаем несколько запросов.
            var files = new List<ModrinthProjectInfo>();

            var str = new StringBuilder(950);
            str.Append("%22"); // это "
            for (int i = 0; i < ids.Length; i++)
            {
                str.Append(WebUtility.UrlEncode(ids[i]));

                // Если это последний элемент, или если при добавлении еще одного id строка будет длиннее 950, то отправляем запрос.
                // На учитывание дополнительных символов кладем хуй, ибо максимальная длинна url 1024, минус 47 символов начало ссылки
                // это будет 977. Мы с запасом взяли 950.
                if (i == ids.Length - 1 || str.Length + ids[i + 1].Length > 950)
                {
                    str.Append("%22");
                    string url = $"https://api.modrinth.com/v2/projects?ids=%5B{str.ToString()}%5D";
                    var data = GetApiData<List<ModrinthProjectInfo>>(url);
                    files.AddRange(data);

                    str = new StringBuilder(950);
                    str.Append("%22");
                }
                else
                {
                    str.Append("%22%2C%22"); // это "," 
                }
            }

            return files;
        }

        public static List<ModrinthCtalogUnit> GetInstances(int pageSize, int index, IEnumerable<IProjectCategory> categories, string sortField, string searchFilter, string gameVersion)
        {
            string url = "https://api.modrinth.com/v2/search?facets=[[%22project_type:modpack%22]]&offset=" + (index * pageSize) + "&limit" + pageSize;

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                url += "&index=" + sortField;
            }

            if (!string.IsNullOrWhiteSpace(searchFilter))
            {
                url += "&query=" + WebUtility.UrlEncode(searchFilter);
            }

            var isFiltersExists = false;
            foreach (var category in categories)
            {
                if (category == null || category.Id == "-1")
                {
                    continue;
                }

                if (isFiltersExists)
                {
                    url += " AND categories=\"" + category.Id + "\"";
                }
                else
                {
                    url += "&filters=(categories=\"" + category.Id + "\"";
                    isFiltersExists = true;
                }
            }

            if (isFiltersExists)
            {
                url += ")";
            }

            return GetApiData<CtalogContainer>(url)?.hits ?? new List<ModrinthCtalogUnit>();
        }


        /// <summary>
        /// Получает список аддонов
        /// </summary>
        /// <param name="type">Тип аддона</param>
        /// <param name="searchParams">параметры поиска</param>
        /// <returns>Первое значение - список аддонов, второе - общее количество аддонов, доступных по запросу</returns>
        public static (List<ModrinthProjectInfo>, int) GetAddonsList(AddonType type, ModrinthSearchParams searchParams)
        {
            string _type;
            switch (type)
            {
                case AddonType.Mods:
                    _type = "mod";
                    break;
                case AddonType.Resourcepacks:
                    _type = "resourcepack";
                    break;
                case AddonType.Shaders:
                    _type = "shader";
                    break;
                default:
                    _type = "resourcepack";
                    break;
            }

            string url = "https://api.modrinth.com/v2/search?facets=";
            string facets = $"[[\"project_type:{_type}\"]";

            if (!string.IsNullOrWhiteSpace(searchParams.GameVersion))
            {
                facets += $",[\"versions:{searchParams.GameVersion}\"]";
            }

            if (type == AddonType.Mods)
            {
                string modloadersFilter = string.Join(",", searchParams.Modloaders.Select(x => ($"\"categories:{x}\"")));
                if (modloadersFilter != string.Empty)
                {
                    facets += $",[{modloadersFilter}]";
                }
            }

            string ctrs = string.Empty;
            foreach (var category in searchParams.Categories)
            {
                if (category.Id == "-1")
                {
                    ctrs = string.Empty;
                    break;
                }

                ctrs += $"\"categories:{category.Id}\",";
            }

            if (ctrs != string.Empty)
            {
                facets += $",[{ctrs.RemoveLastChars(1)}]";
            }

            facets += "]";

            url += WebUtility.UrlEncode(facets) + "&offset=" + (searchParams.PageIndex * searchParams.PageSize) + "&limit" + searchParams.PageSize;

            if (!string.IsNullOrWhiteSpace(searchParams.SearchFilter))
            {
                url += "&query=" + WebUtility.UrlEncode(searchParams.SearchFilter);
            }

            url += "&index=" + searchParams.SortFieldString;

            Runtime.DebugWrite(url);
            CtalogContainer catalogList = GetApiData<CtalogContainer>(url);
            var result = new List<ModrinthProjectInfo>();

            if (catalogList.hits != null)
            {
                foreach (var hit in catalogList.hits)
                {
                    if (hit == null) continue;
                    result.Add(new ModrinthProjectInfo(hit));
                }
            }

            return (result, catalogList.TotalHits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(string fileUrl, string fileName, ModrinthProjectType addonType, string path, string projectID, string fileID, TaskArgs taskArgs)
        {
            // определяем папку в которую будет установлен данный аддон
            string folderName = "";
            AddonType baseAddonType;
            switch (addonType)
            {
                case ModrinthProjectType.Mod:
                    folderName = "mods";
                    baseAddonType = AddonType.Mods;
                    break;
                case ModrinthProjectType.Shader:
                    baseAddonType = AddonType.Shaders;
                    folderName = "shaderpacks";
                    break;
                case ModrinthProjectType.Resourcepack:
                    baseAddonType = AddonType.Resourcepacks;
                    folderName = "resourcepacks";
                    break;
                default:
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.unknownAddonType
                    };
            }

            // устанавливаем
            if (!WithDirectory.InstallFile(fileUrl, fileName, path + folderName, taskArgs))
            {
                return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                {
                    Value1 = null,
                    Value2 = taskArgs.CancelToken.IsCancellationRequested ? DownloadAddonRes.IsCanselled : DownloadAddonRes.DownloadError
                };
            }

            Runtime.DebugWrite("SYS " + fileUrl);

            return new SetValues<InstalledAddonInfo, DownloadAddonRes>
            {
                Value1 = new InstalledAddonInfo
                {
                    ProjectID = projectID,
                    FileID = fileID,
                    Path = folderName + "/" + fileName,
                    Type = baseAddonType,
                    Source = ProjectSource.Modrinth

                },
                Value2 = DownloadAddonRes.Successful
            };
        }

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(ModrinthProjectFile fileInfo, ModrinthProjectType addonType, string path, string fileName, TaskArgs taskArgs)
        {
            Runtime.DebugWrite("PR ID " + fileInfo.ProjectId);
            string projectID = fileInfo.ProjectId;
            string fileID = fileInfo.FileId;
            try
            {
                if (fileInfo.Files == null || fileInfo.Files.Count == 0)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.UrlError
                    };
                }

                string fileUrl = fileInfo.Files[0].Url;

                if (fileUrl == null)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.UrlError
                    };
                }

                Runtime.DebugWrite(fileUrl);

                return DownloadAddon(fileUrl, fileName, addonType, path, projectID, fileID, taskArgs);
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

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(ModrinthProjectFile fileInfo, ModrinthProjectType addonType, string path, TaskArgs taskArgs)
        {
            Runtime.DebugWrite("PR ID " + fileInfo.ProjectId);
            string projectID = fileInfo.ProjectId;
            string fileID = fileInfo.FileId;
            try
            {
                if (fileInfo.Files == null || fileInfo.Files.Count == 0)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.UrlError
                    };
                }

                string fileUrl = fileInfo.Files[0].Url;

                if (fileUrl == null)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.UrlError
                    };
                }

                Runtime.DebugWrite(fileUrl);

                string fileName = fileInfo.Files[0].Filename;

                // проверяем имя файла на валидность
                char[] invalidFileChars = Path.GetInvalidFileNameChars();
                bool isInvalidFilename = invalidFileChars.Any(s => fileName?.Contains(s) != false);

                if (isInvalidFilename)
                {
                    return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = DownloadAddonRes.FileNameError
                    };
                }

                return DownloadAddon(fileUrl, fileName, addonType, path, projectID, fileID, taskArgs);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ModrinthProjectType StrProjectTypeToEnum(string typeStr)
        {
            ModrinthProjectType type;
            switch (typeStr)
            {
                case "mod":
                    type = ModrinthProjectType.Mod;
                    break;
                case "resourcepack":
                    type = ModrinthProjectType.Resourcepack;
                    break;
                case "shader":
                    type = ModrinthProjectType.Shader;
                    break;
                default:
                    type = ModrinthProjectType.Unknown;
                    break;
            }

            return type;
        }

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(ModrinthProjectInfo addonInfo, string fileID, string path, TaskArgs taskArgs)
        {
            ModrinthProjectFile fileInfo = GetProjectFile(fileID);
            return DownloadAddon(fileInfo, StrProjectTypeToEnum(addonInfo.Type), path, taskArgs);
        }
    }
}
