using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Tools;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lexplosion.Logic.Network.Web
{
    static class ModrinthApi
    {
        public struct SearchFilters
        {
            public const string Relevance = "relevance";
        }

        private class CtalogContainer
        {
            public List<ModrinthCtalogUnit> hits;
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

        public static List<ModrinthProjectInfo> GetProjects(string[] filesId)
        {
            var files = new List<ModrinthProjectInfo>();

            StringBuilder str = new StringBuilder(1950);
            for (int i = 0; i < filesId.Length; i++)
            {
                str.Append(filesId[i]);

                if (i == filesId.Length - 1 || str.Length + 3 + filesId[i + 1].Length > 1950)
                {
                    var data = GetApiData<List<ModrinthProjectInfo>>("https://api.modrinth.com/v2/projects?ids=[\"" + str.ToString() + "\"]");
                    var tes = "https://api.modrinth.com/v2/projects?ids=[\"" + str.ToString() + "\"]";
                    files.AddRange(data);

                    str = new StringBuilder(1950);
                }
                else
                {
                    str.Append("\",\"");
                }
            }

            return files;
        }

        public static List<ModrinthCtalogUnit> GetInstances(int pageSize, int index, IProjectCategory categoriy, string sortField, string searchFilter, string gameVersion)
        {
            string url = "https://api.modrinth.com/v2/search?facets=[[%22project_type:modpack%22]]&offset=" + (index * pageSize) + "&limit" + pageSize;

            if (!string.IsNullOrEmpty(sortField))
            {
                url += "&index=" + sortField;
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                url += "&query=" + WebUtility.UrlEncode(searchFilter);
            }

            if (categoriy.Id != "-1")
            {
                url += "&filters=categories=\"" + categoriy.Id + "\"";
            }

            return GetApiData<CtalogContainer>(url)?.hits ?? new List<ModrinthCtalogUnit>();
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
                        Value2 = DownloadAddonRes.UncnownAddonType
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
                    Value2 = DownloadAddonRes.UncnownError
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
                    Value2 = DownloadAddonRes.UncnownError
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ModrinthProjectType StrProjectTypToEnum(string typeStr)
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
            return DownloadAddon(fileInfo, StrProjectTypToEnum(addonInfo.Type), path, taskArgs);
        }
    }
}
