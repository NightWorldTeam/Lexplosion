using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Tools;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lexplosion.Logic.FileSystem.Extensions
{
    public static class ModrinthApiExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(string fileUrl, string fileName, ModrinthProjectType addonType, string path, string projectID, string fileID, WithDirectory withDirectory, TaskArgs taskArgs)
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
            if (!withDirectory.InstallFile(fileUrl, fileName, path + folderName, taskArgs))
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

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(this ModrinthApi api, ModrinthProjectFile fileInfo, ModrinthProjectType addonType, string path, string fileName, WithDirectory withDirectory, TaskArgs taskArgs)
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

                return DownloadAddon(fileUrl, fileName, addonType, path, projectID, fileID, withDirectory, taskArgs);
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

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(this ModrinthApi api, ModrinthProjectFile fileInfo, ModrinthProjectType addonType, string path, WithDirectory withDirectory, TaskArgs taskArgs)
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

                return DownloadAddon(fileUrl, fileName, addonType, path, projectID, fileID, withDirectory, taskArgs);
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
        public static ModrinthProjectType StrProjectTypeToEnum(this ModrinthApi api, string typeStr)
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

        public static SetValues<InstalledAddonInfo, DownloadAddonRes> DownloadAddon(this ModrinthApi api, ModrinthProjectInfo addonInfo, string fileID, string path, WithDirectory withDirectory, TaskArgs taskArgs)
        {
            ModrinthProjectFile fileInfo = api.GetProjectFile(fileID);
            return api.DownloadAddon(fileInfo, api.StrProjectTypeToEnum(addonInfo.Type), path, withDirectory, taskArgs);
        }
    }
}
