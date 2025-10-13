using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lexplosion.Logic.Network.Web
{
    public class ModrinthApi
    {
        private readonly ToServer _toServer;

        public struct SearchFilters
        {
            public const string Relevance = "relevance";
            public const string Downloads = "downloads";
            public const string Newest = "newest";
            public const string Updated = "updated";
            public const string Follows = "follows";
        }

        private class CatalogContainer
        {
            public List<ModrinthCtalogUnit> hits;

            [JsonProperty("total_hits")]
            public int TotalHits;
        }

        public ModrinthApi(ToServer toServer)
        {
            _toServer = toServer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetApiData<T>(string url) where T : new()
        {
            try
            {
                string answer = _toServer.HttpGet(url);
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
        private T GetApiData<T, U>(string url, U inputData) where T : new()
        {
            try
            {
                string answer = _toServer.HttpPostJson(url, JsonConvert.SerializeObject(inputData), out _);
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

        public List<ModrinthTeam> GetTeam(string teamId)
        {
            return GetApiData<List<ModrinthTeam>>("https://api.modrinth.com/v2/team/" + teamId + "/members");
        }

        public List<ModrinthCategory> GetCategories()
        {
            return GetApiData<List<ModrinthCategory>>("https://api.modrinth.com/v2/tag/category");
        }

        public ModrinthProjectInfo GetProject(string projectId)
        {
            return GetApiData<ModrinthProjectInfo>("https://api.modrinth.com/v2/project/" + projectId);
        }

        public List<ModrinthProjectFile> GetProjectFiles(string projectId)
        {
            return GetApiData<List<ModrinthProjectFile>>("https://api.modrinth.com/v2/project/" + projectId + "/version");
        }

        /// <summary>
        /// Возвращает список файлов проекта
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="modloaders">Белый список модлоадеров. Если он не нужен, то null</param>
        /// <param name="gameVersion">Версия игры</param>
        public List<ModrinthProjectFile> GetProjectFiles(string projectId, IEnumerable<Modloader> modloaders, string gameVersion)
        {
            string param = "?game_versions=" + WebUtility.UrlEncode($"[\"{gameVersion}\"]");
            if (modloaders != null)
            {
                param += "&loaders=" + WebUtility.UrlEncode($"[{string.Join(",", modloaders.Select(x => $"\"{x.ToString().ToLower()}\""))}]");
            }
            string url = "https://api.modrinth.com/v2/project/" + projectId + "/version" + param;
            return GetApiData<List<ModrinthProjectFile>>(url);
        }

        /// <summary>
        /// Возвращает список файлов проекта
        /// </summary>
        /// <param name="projectId">id проекта</param>
        /// <param name="modloader">Допустимый модлоадер. Если модлоадер не важен, то null</param>
        /// <param name="gameVersion">Версия игры</param>
        public List<ModrinthProjectFile> GetProjectFiles(string projectId, Modloader? modloader, string gameVersion)
        {
            Modloader[] modloaders = null;
            if (modloader != null) modloaders = new Modloader[] { modloader ?? (Modloader)modloader };

            return GetProjectFiles(projectId, modloaders, gameVersion);

        }

        public ModrinthProjectFile GetProjectFile(string fileId)
        {
            return GetApiData<ModrinthProjectFile>("https://api.modrinth.com/v2/version/" + fileId);
        }

        public List<ModrinthProjectFile> GetFilesFromHash(string hash)
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
        public Dictionary<string, ModrinthProjectFile> GetFilesFromHashes(List<string> hashes)
        {
            return GetApiData<Dictionary<string, ModrinthProjectFile>, HashesContainer>("https://api.modrinth.com/v2/version_files", new HashesContainer
            {
                hashes = hashes,
                algorithm = "sha512"
            });
        }

        public List<ModrinthProjectInfo> GetProjects(string[] ids)
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

        public CatalogResult<ModrinthCtalogUnit> GetInstances(int pageSize, int index, IEnumerable<IProjectCategory> categories, string sortField, string searchFilter, string gameVersion)
        {
            var queryBuilder = new QueryApiBuilder("https://api.modrinth.com/v2/search");


            queryBuilder.Add("offset", (index * pageSize));
            queryBuilder.Add("limit", pageSize);

            if (!string.IsNullOrWhiteSpace(sortField))
                queryBuilder.Add("index", sortField);

            if (!string.IsNullOrWhiteSpace(searchFilter))
                queryBuilder.Add("query", WebUtility.UrlEncode(searchFilter));

            var categoriesQuery = BuildCategoriesToQuery(categories);
            queryBuilder.Add("facets", $"[[%22project_type:modpack%22]{categoriesQuery}]");

            var url = queryBuilder.Build();
            Runtime.DebugWrite(url, color: ConsoleColor.Cyan);

            var catalogContainer = GetApiData<CatalogContainer>(url);
            return new(catalogContainer?.hits ?? new(), catalogContainer.TotalHits);
        }


        /// <summary>
        /// Возвращает строку формата ,["categories: ..."]
        /// </summary>
        private string BuildCategoriesToQuery(IEnumerable<IProjectCategory> categories)
        {
            var result = string.Empty;

            foreach (var category in categories)
            {
                if (category.Id == "-1")
                {
                    return string.Empty;
                }

                result += $",[\"categories:{category.Id}\"]";
            }

            return result;
        }

        /// <summary>
        /// Возвращает строку формата ,["categories:forge","categories:neoforge" ..."]
        /// </summary>
        private string BuildModloadersToQuery(IEnumerable<string> modloaders)
        {
            var facets = string.Empty;
            string modloadersFilter = string.Join(",", modloaders.Select(x => ($"\"categories:{x}\"")));
            if (modloadersFilter != string.Empty)
            {
                facets += $",[{modloadersFilter}]";
            }
            return facets;
        }

        /// <summary>
        /// Получает список аддонов
        /// </summary>
        /// <param name="type">Тип аддона</param>
        /// <param name="searchParams">параметры поиска</param>
        /// <returns>Первое значение - список аддонов, второе - общее количество аддонов, доступных по запросу</returns>
        public CatalogResult<ModrinthProjectInfo> GetAddonsList(AddonType type, ModrinthSearchParams searchParams)
        {
            string _type = type switch
            {
                AddonType.Mods => _type = "mod",
                AddonType.Resourcepacks => _type = "resourcepack",
                AddonType.Shaders => _type = "shader",
                _ => _type = "resourcepack"
            };

            var queryBuilder = new QueryApiBuilder("https://api.modrinth.com/v2/search");

            queryBuilder.Add("limit", searchParams.PageSize);
            queryBuilder.Add("index", searchParams.SortFieldString);

            var offset = searchParams.PageIndex * searchParams.PageSize;
            if (offset > 0)
                queryBuilder.Add("offset", offset);

            if (!string.IsNullOrWhiteSpace(searchParams.SearchFilter))
                queryBuilder.Add("query", WebUtility.UrlEncode(searchParams.SearchFilter));

            var facets = $"[[\"project_type:{_type}\"], [\"versions:{searchParams.GameVersion}\"]";
            facets += BuildCategoriesToQuery(searchParams.Categories);

            if (type == AddonType.Mods)
                facets += BuildModloadersToQuery(searchParams.Modloaders);

            facets += "]";

            queryBuilder.Add("facets", facets);

            var url = queryBuilder.Build();

            Runtime.DebugWrite(url, color: ConsoleColor.Cyan);

            CatalogContainer catalogList = GetApiData<CatalogContainer>(url);
            var result = new List<ModrinthProjectInfo>();

            if (catalogList.hits != null)
            {
                foreach (var hit in catalogList.hits)
                {
                    if (hit == null) continue;
                    result.Add(new ModrinthProjectInfo(hit));
                }
            }

            return new(result, catalogList.TotalHits);
        }
    }
}
