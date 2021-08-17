using Lexplosion.Global;
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
        public InstancePlatformData InfoData;

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
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null || !Int32.TryParse(InfoData.id, out _))
            {
                return "cursforgeIdError";
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
                }
                else
                {
                    return "serverError";
                }
            }

            List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetInstanceInfo(InfoData.id); //получем информацию об этом модпаке

            //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии
            foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
            {
                if (ver.id > InfoData.instanceVersion)
                {
                    InfoData.instanceVersion = ver.id;
                    Info = ver;
                }
            }

            return "";
        }

        public InitData Update()
        {
            try
            {
                CurseforgeInstanceInfo info = CurseforgeApi.GetInstance(InfoData.id);
                if (info.attachments.Count > 0)
                {
                    // TODO: написать где-то отдельную функцию для скачивания файла
                    string dir = WithDirectory.directory + "/instances-assets/" + InstanceId;
                    string[] a = info.attachments[0].thumbnailUrl.Split('/');
                    string fileName = dir + "/" + a[a.Length - 1];

                    using (WebClient wc = new WebClient())
                    {
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        // TODO: в info.attachments нужно брать не первый элемент, а тот у котрого isDefault стоит на true
                        wc.DownloadFile(info.attachments[0].thumbnailUrl, fileName);
                    }

                    if (!UserData.instancesAssets.ContainsKey(InstanceId) || UserData.instancesAssets[InstanceId] == null)
                    {
                        UserData.instancesAssets[InstanceId] = new InstanceAssets
                        {
                            description = "",
                            mainImage = InstanceId + "/" + a[a.Length - 1]
                        };
                    }
                    else
                    {
                        UserData.instancesAssets[InstanceId].mainImage = InstanceId + "/" + a[a.Length - 1];
                    }

                    DataFilesManager.SaveFile(dir + "/assets.json", JsonConvert.SerializeObject(UserData.instancesAssets[InstanceId]));
                }
            }
            catch { }

            List<string> errors = new List<string>();
            ProgressHandler(30);
            //нашелся id, который больше id установленной версии. Значит доступно обновление. Обновляем
            if (Info != null) 
            {
                List<string> localFiles = DataFilesManager.GetFile<List<string>>(WithDirectory.directory + "/instances/" + InstanceId + "/localFiles.json"); //получем список всех файлов модпака
                InstanceManifest manifest = WithDirectory.DownloadCurseforgeInstance(Info.downloadUrl, Info.fileName, InstanceId, out List<string> error, ref localFiles);
                DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + InstanceId + "/localFiles.json", JsonConvert.SerializeObject(localFiles)); // функция DownloadCurseforgeInstance изменила этот список. сохраняем его

                if (error.Count > 0)
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
                DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
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
