using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
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
        private static event ManageLogic.ProgressHandlerDelegate ProgressHandler;

        public CurseforgeInstance(string instanceid, ManageLogic.ProgressHandlerDelegate progressHandler)
        {
            InstanceId = instanceid;
            ProgressHandler = progressHandler;
        }

        public string Check()
        {
            ProgressHandler(10);
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
            ProgressHandler(20);

            if (Manifest.version.forgeVersion != null && Manifest.version.forgeVersion != "")
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

            List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetInstanceInfo(InstanceData["cursforgeId"]); //получем информацию об этом модпаке

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
            CurseforgeInstanceInfo info = CurseforgeApi.GetInstance(InstanceData["cursforgeId"]);
            if(info.attachments.Count > 0)
            {
                // TODO: написать где-то отдельную функцию для скачивания файла
                using (WebClient wc = new WebClient())
                {
                    string dir = WithDirectory.directory + "/instances-assets/" + InstanceId;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    string[] a = info.attachments[0].thumbnailUrl.Split('.');

                    wc.DownloadFile(info.attachments[0].thumbnailUrl, dir + "/main." + a[a.Length - 1]);
                }
            }

            List<string> errors = new List<string>();
            ProgressHandler(30);
            //нашелся id, который больше id установленной версии. Значит доступно обновление. Обновляем
            if (Info != null) 
            {
                InstanceManifest manifest = WithDirectory.DownloadCurseforgeInstance(Info.downloadUrl, Info.fileName, InstanceId, out List<string> error);

                if(error.Count > 0)
                {
                    return new InitData
                    {
                        Errors = error,
                        VersionFile = null,
                        Libraries = null
                    };
                }

                if (!BaseFilesIsCheckd)
                {
                    //определяем приоритетную версию форджа
                    string modLoader = "";
                    foreach(var loader in manifest.minecraft.modLoaders)
                    {
                        if (loader.primary)
                        {
                            modLoader = loader.id;
                            break;
                        }
                    }
                    
                    Manifest = ToServer.GetVersionManifest(manifest.minecraft.version, modLoader);

                    if (Manifest != null)
                    {
                        Updates = WithDirectory.GetLastUpdates(InstanceId);
                        BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                        if (BaseFiles == null)
                        {
                            ProgressHandler(97);
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
                        ProgressHandler(99);
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

            MessageBox.Show("gv");
            DataFilesManager.SaveManifest(InstanceId, Manifest);
            ProgressHandler(100);

            return new InitData
            {
                Errors = errors,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }

        public string CheckOnlyBase()
        {
            Manifest = DataFilesManager.GetManifest(InstanceId, false);

            if (Manifest == null || Manifest.version == null || Manifest.version.gameVersion == null)
            {
                return "versionError";
            }

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

        public InitData UpdateOnlyBase()
        {
            List<string> errors = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);

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
