using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Logic.Management
{
    class CurseforgeInstance : IPrototypeInstance
    {
        WithDirectory.BaseFilesUpdates BaseFiles;

        VersionManifest Manifest;
        Dictionary<string, int> Updates;
        CurseforgeFileInfo Info = null;

        private string InstanceId;
        private Dictionary<string, int> InstanceData;

        private bool BaseFilesIsCheckd = false;

        public CurseforgeInstance(string instanceid)
        {
            InstanceId = instanceid;
        }

        public string Check()
        {
            Manifest = DataFilesManager.GetManifest(InstanceId, false);
            InstanceData = DataFilesManager.GetFile<Dictionary<string, int>>(WithDirectory.directory + "/instances/" + InstanceId + "/cursforgeData.json");

            if (InstanceData == null || !InstanceData.ContainsKey("cursforgeId"))
            {
                return "cursforgeIdError";
            }

            if (!InstanceData.ContainsKey("instanceVersion"))
            {
                InstanceData["instanceVersion"] = 0;
            }

            if (Manifest == null || Manifest.version == null || Manifest.version.gameVersion == null)
            {
                return "versionError";
            }

            if(Manifest.version.forgeVersion != null && Manifest.version.forgeVersion != "")
            {
                BaseFilesIsCheckd = true;

                Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.forgeVersion);

                if (Manifest != null)
                {
                    Updates = WithDirectory.GetLastUpdates(InstanceId);
                    BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                    if (BaseFiles == null)
                    {
                        return "guardError";
                    }

                    return "";
                }
                else
                {
                    return "serverError";
                }

            }

            List<CurseforgeFileInfo> instanceVersionsInfo = ToServer.GetCursforgeInstanceInfo(InstanceData["cursforgeId"]); //получем информацию об этом модпаке

            //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии
            foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
            {
                if (ver.id > InstanceData["instanceVersion"])
                {
                    InstanceData["instanceVersion"] = ver.id;
                    Info = ver;
                }
            }

            return "";

        }

        public InitData Update()
        {
            List<string> errors = new List<string>();

            //нашелся id, который больше id установленной версии. Значит доступно обновление. Обновляем
            if (Info != null) 
            {
                InstanceManifest manifest = WithDirectory.DownloadCurseforgeInstance(Info.downloadUrl, Info.fileName, InstanceId);

                if (!BaseFilesIsCheckd)
                {
                    Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, manifest.minecraft.modLoaders[0].id);

                    if (Manifest != null)
                    {
                        Updates = WithDirectory.GetLastUpdates(InstanceId);
                        BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                        if (BaseFiles == null)
                        {
                            return new InitData
                            {
                                Errors = new List<string>() { "guardError" },
                                VersionFile = null,
                                Libraries = null
                            };
                        }
                    }
                    else
                    {
                        return new InitData
                        {
                            Errors = new List<string>() { "serverError" },
                            VersionFile = null,
                            Libraries = null
                        };
                    }
                }

                WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
                DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + InstanceId + "/cursforgeData.json", JsonConvert.SerializeObject(InstanceData));
            }

            DataFilesManager.SaveManifest(InstanceId, Manifest);

            return new InitData
            {
                Errors = errors,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }
    }
}
