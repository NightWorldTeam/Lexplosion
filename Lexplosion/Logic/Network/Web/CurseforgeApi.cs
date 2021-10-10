using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Logic.Network
{
    static class CurseforgeApi
    {
        public enum AddonType
        {
            Mod = 6,
            Resourcepacks = 12,
            Maps = 17,
            Unknown
        }

        public static List<CurseforgeInstanceInfo> GetInstances(int pageSize, int index, ModpacksCategories categoriy, string searchFilter = "")
        {
            try
            {
                if (pageSize > 50)
                {
                    pageSize = 50;
                }

                string url;
                if (categoriy == ModpacksCategories.All)
                {
                    url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=4471&pageSize=" + pageSize + "&index=" + index + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
                }
                else
                {
                    url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=4471&pageSize=" + pageSize + "&index=" + index + "&categoryId=" + ((int)categoriy) + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
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
                    return JsonConvert.DeserializeObject<CurseforgeInstanceInfo>(answer);
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

            public CategorySection categorySection;
            public List<FileData> files;
        }

        public static bool DownloadAddon(int projectID, int fileID, string path, bool downloadDependencies = false)
        {
            try
            {
                string answer = ToServer.HttpGet("https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID);
                if (answer == null)
                {
                    return false;
                }

                ProjectTypeInfo data = JsonConvert.DeserializeObject<ProjectTypeInfo>(answer);

                answer = ToServer.HttpGet("https://addons-ecs.forgesvc.net/api/v2/addon/" + projectID + "/files");
                if (answer == null)
                {
                    return false;
                }

                data.files = JsonConvert.DeserializeObject<List<ProjectTypeInfo.FileData>>(answer);

                string fileUrl = "";
                string fileName = "";

                foreach (ProjectTypeInfo.FileData v in data.files)
                {
                    if (v.id == fileID && !String.IsNullOrWhiteSpace(v.downloadUrl) && !String.IsNullOrWhiteSpace(v.fileName))
                    {
                        char[] invalidFileChars = Path.GetInvalidFileNameChars();
                        bool isInvalidFilename = invalidFileChars.Any(s => v.fileName.Contains(s));

                        if (isInvalidFilename)
                        {
                            continue;
                        }

                        fileUrl = v.downloadUrl;
                        fileName = v.fileName;

                        if (downloadDependencies && v.dependencies.Count > 0)
                        {
                            foreach (Dictionary<string, int> value in v.dependencies)
                            {
                                if (value.ContainsKey("type") && value["type"] == 3 && value.ContainsKey("addonId") && value.ContainsKey("fileId"))
                                {
                                    bool res = DownloadAddon(value["addonId"], value["fileId"], path, true);
                                    if (!res)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }

                        break;
                    }
                }

                if (fileUrl != "")
                {
                    string folderName = "";
                    AddonType addonType = (AddonType)data.categorySection.packageType;
                    switch (addonType)
                    {
                        case CurseforgeApi.AddonType.Mod:
                            folderName = "mods/";
                            break;
                        case CurseforgeApi.AddonType.Maps:
                            folderName = "saves/";
                            break;
                        case CurseforgeApi.AddonType.Resourcepacks:
                            folderName = "resourcepacks/";
                            break;
                        case CurseforgeApi.AddonType.Unknown:
                            return false;
                    }

                    return WithDirectory.InstallFile(fileUrl, fileName, path + folderName);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
