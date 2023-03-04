using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        public static ModrinthProjectInfo GetInstance(string projectId)
        {
            return GetApiData<ModrinthProjectInfo>("https://api.modrinth.com/v2/project/" + projectId);
        }

        public static ModrinthProjectInfo GetProjectFiles(string projectId)
        {
            return GetApiData<ModrinthProjectInfo>("https://api.modrinth.com/v2/project/" + projectId);
        }

        public static List<ModrinthCtalogUnit> GetInstances(int pageSize, int index, int categoriy, string sortField, string searchFilter, string gameVersion)
        {
            if (gameVersion != "")
            {
                gameVersion = "&gameVersion=" + gameVersion;
            }

            string url = "https://api.modrinth.com/v2/search?facets=[[%22project_type:modpack%22]]&offset=" + (index * pageSize) + "&limit" + pageSize + "&index=" + sortField + "&query=" + WebUtility.UrlEncode(searchFilter);
            Runtime.DebugWrite(url);
            return GetApiData<CtalogContainer>(url)?.hits ?? new List<ModrinthCtalogUnit>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValuePair<InstalledAddonInfo, DownloadAddonRes> InstallAddon(AddonType addonType, string fileUrl, string fileName, string path, string folderName, string projectID, string fileID, TaskArgs taskArgs)
        {
            if (addonType != AddonType.Maps)
            {
                if (!WithDirectory.InstallFile(fileUrl, fileName, path + folderName, taskArgs))
                {
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
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
                    return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
                    {
                        Value1 = null,
                        Value2 = taskArgs.CancelToken.IsCancellationRequested ? DownloadAddonRes.IsCanselled : DownloadAddonRes.DownloadError
                    };
                }

                Runtime.DebugWrite("SYS " + fileUrl);
            }

            return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
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

        //public static ValuePair<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(CurseforgeFileInfo addonInfo, AddonType addonType, string path, TaskArgs taskArgs)
        //{
        //    Runtime.DebugWrite("PR ID " + addonInfo.id);
        //    string projectID = addonInfo.modId;
        //    string fileID = addonInfo.id.ToString();
        //    try
        //    {
        //        Runtime.DebugWrite("fileData " + addonInfo.downloadUrl + " " + projectID.ToString() + " " + fileID.ToString());

        //        string fileUrl = addonInfo.downloadUrl;
        //        string fileName = addonInfo.fileName;

        //        if (String.IsNullOrWhiteSpace(addonInfo.downloadUrl))
        //        {
        //            return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //            {
        //                Value1 = null,
        //                Value2 = DownloadAddonRes.UrlError
        //            };
        //        }

        //        Runtime.DebugWrite(fileUrl);

        //        // проверяем имя файла на валидность
        //        char[] invalidFileChars = Path.GetInvalidFileNameChars();
        //        bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

        //        if (isInvalidFilename)
        //        {
        //            return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //            {
        //                Value1 = null,
        //                Value2 = DownloadAddonRes.FileNameError
        //            };
        //        }

        //        // определяем папку в которую будет установлен данный аддон
        //        string folderName = "";
        //        switch (addonType)
        //        {
        //            case AddonType.Mods:
        //                folderName = "mods";
        //                break;
        //            case AddonType.Maps:
        //                folderName = "saves";
        //                break;
        //            case AddonType.Resourcepacks:
        //                folderName = "resourcepacks";
        //                break;
        //            case AddonType.Unknown:
        //                return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //                {
        //                    Value1 = null,
        //                    Value2 = DownloadAddonRes.UncnownAddonType
        //                };
        //        }

        //        // устанавливаем
        //        return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, taskArgs);
        //    }
        //    catch
        //    {
        //        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //        {
        //            Value1 = null,
        //            Value2 = DownloadAddonRes.UncnownError
        //        };
        //    }
        //}

        //public static ValuePair<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(ModrinthAddonInfo addonInfo, string fileID, string path, TaskArgs taskArgs)
        //{
        //    try
        //    {
        //        string projectID = addonInfo.ProjectId;
        //        Runtime.DebugWrite("");
        //        Runtime.DebugWrite("PR ID " + projectID);

        //        Runtime.DebugWrite(fileUrl);

        //        string fileName = fileData.fileName;

        //        // проверяем имя файла на валидность
        //        char[] invalidFileChars = Path.GetInvalidFileNameChars();
        //        bool isInvalidFilename = invalidFileChars.Any(s => fileName.Contains(s));

        //        if (isInvalidFilename)
        //        {
        //            return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //            {
        //                Value1 = null,
        //                Value2 = DownloadAddonRes.FileNameError
        //            };
        //        }

        //        // определяем папку в которую будет установлен данный аддон
        //        string folderName = "";
        //        AddonType addonType = (AddonType)addonInfo.classId;
        //        switch (addonType)
        //        {
        //            case AddonType.Mods:
        //                folderName = "mods";
        //                break;
        //            case AddonType.Maps:
        //                folderName = "saves";
        //                break;
        //            case AddonType.Resourcepacks:
        //                folderName = "resourcepacks";
        //                break;
        //            case AddonType.Unknown:
        //                return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //                {
        //                    Value1 = null,
        //                    Value2 = DownloadAddonRes.UncnownAddonType
        //                };
        //        }

        //        // устанавливаем
        //        return InstallAddon(addonType, fileUrl, fileName, path, folderName, projectID, fileID, taskArgs);
        //    }
        //    catch
        //    {
        //        return new ValuePair<InstalledAddonInfo, DownloadAddonRes>
        //        {
        //            Value1 = null,
        //            Value2 = DownloadAddonRes.UncnownError
        //        };
        //    }
        //}

    }
}
