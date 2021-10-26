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
        public class InstanceManifest
        {
            public class McVersionInfo
            {
                public string version;
                public List<ModLoaders> modLoaders;
            }

            public class ModLoaders
            {
                public string id;
                public bool primary;
            }

            public class FileData
            {
                public int projectID;
                public int fileID;
            }

            public McVersionInfo minecraft;
            public string name;
            public string version;
            public string author;
            public List<FileData> files;
        }

        WithDirectory.BaseFilesUpdates BaseFiles;

        VersionManifest Manifest;
        Dictionary<string, int> Updates;
        CurseforgeFileInfo Info = null;

        private string InstanceId;
        public InstancePlatformData InfoData;

        private bool BaseFilesIsCheckd = false;
        private bool onlyBase;
        private static event ManageLogic.ProgressHandlerDelegate ProgressHandler;

        public CurseforgeInstance(string instanceid, bool onlyBase_, ManageLogic.ProgressHandlerDelegate progressHandler)
        {
            InstanceId = instanceid;
            ProgressHandler = progressHandler;
            onlyBase = onlyBase_;
        }

        public InstanceInit Check()
        {
            //ProgressHandler(10);
            Manifest = DataFilesManager.GetManifest(InstanceId, false);
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null || !Int32.TryParse(InfoData.id, out _))
            {
                return InstanceInit.CursforgeIdError;
            }

            if (Manifest == null || Manifest.version == null || Manifest.version.gameVersion == null)
            {
                return InstanceInit.VersionError;
            }

            //ProgressHandler(20);

            if (Manifest.version.modloaderVersion != null && Manifest.version.modloaderVersion != "")
            {
                BaseFilesIsCheckd = true;

                Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);

                if (Manifest != null)
                {
                    Updates = WithDirectory.GetLastUpdates(InstanceId);
                    BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                    if (BaseFiles == null)
                    {
                        return InstanceInit.GuardError;
                    }
                }
                else
                {
                    return InstanceInit.ServerError;
                }
            }

            List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetInstanceInfo(InfoData.id); //получем информацию об этом модпаке

            if (!onlyBase)
            {
                //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии
                foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
                {
                    if (ver.id > InfoData.instanceVersion)
                    {
                        InfoData.instanceVersion = ver.id;
                        Info = ver;
                    }
                }
            }

            return InstanceInit.Successful;
        }

        public InitData Update()
        {
            try
            {
                // скачивание иконки
                CurseforgeInstanceInfo info = CurseforgeApi.GetInstance(InfoData.id);
                string dir = WithDirectory.directory + "/instances-assets/" + InstanceId;
                InstanceAssets assets = new InstanceAssets();

                if (info.attachments.Count > 0)
                {
                    // TODO: написать где-то отдельную функцию для скачивания файла
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

                    assets.description = "";
                    assets.mainImage = InstanceId + "/" + a[a.Length - 1];
                }

                //устанавливаем описание
                if (info.summary != null)
                {
                    assets.description = info.summary;
                }

                //устанавливаем автора
                if (info.authors.Count > 0 && info.authors[0].name != null)
                {
                    assets.author = info.authors[0].name;
                }

                // сохраняем асетсы модпака
                UserData.Instances.SetAssets(InstanceId, assets);
                DataFilesManager.SaveFile(dir + "/assets.json", JsonConvert.SerializeObject(UserData.Instances.Assets[InstanceId]));
            }
            catch { }

            //ProgressHandler(30);
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
                        InitResult = InstanceInit.DownloadFilesError,
                        DownloadErrors = error,
                    };
                }

                if (!BaseFilesIsCheckd)
                {
                    //определяем приоритетную версию модлоадера
                    string modLoaderVersion = "";
                    ModloaderType modloader = ModloaderType.None;
                    foreach (var loader in manifest.minecraft.modLoaders)
                    {
                        if (loader.primary)
                        {
                            modLoaderVersion = loader.id;
                            break;
                        }
                    }

                    if (modLoaderVersion != "")
                    {
                        if (modLoaderVersion.Contains("forge-"))
                        {
                            modloader = ModloaderType.Forge;
                            modLoaderVersion = modLoaderVersion.Replace("forge-", "");
                        }
                        else if (modLoaderVersion.Contains("fabric-"))
                        {
                            modloader = ModloaderType.Fabric;
                            modLoaderVersion = modLoaderVersion.Replace("fabric-", "");
                        }
                    }

                    Manifest = ToServer.GetVersionManifest(manifest.minecraft.version, modloader, modLoaderVersion);

                    if (Manifest != null)
                    {
                        Updates = WithDirectory.GetLastUpdates(InstanceId);
                        BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                        if (BaseFiles == null)
                        {
                            //ProgressHandler(97);
                            return new InitData
                            {
                                InitResult = InstanceInit.GuardError,
                            };
                        }
                    }
                    else
                    {
                        //ProgressHandler(99);
                        return new InitData
                        {
                            InitResult = InstanceInit.ServerError,
                        };
                    }
                }

                WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
                DataFilesManager.SaveFile(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
            }
            else
            {
                if(BaseFilesIsCheckd)
                {
                    WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
                }
                else
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.ForgeVersionError,
                    };
                }
            }

            DataFilesManager.SaveManifest(InstanceId, Manifest);
            //ProgressHandler(100);

            return new InitData
            {
                InitResult = InstanceInit.Successful,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }
    }
}
