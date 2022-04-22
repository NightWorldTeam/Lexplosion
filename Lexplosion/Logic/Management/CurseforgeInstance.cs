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
        private VersionManifest Manifest;
        private LastUpdates Updates;
        private CurseforgeFileInfo Info = null;
        private CurseforgeInstaller installer;

        private string InstanceId;
        private InstancePlatformData InfoData;

        private bool BaseFilesIsCheckd = false;
        private bool onlyBase;
        private ProgressHandlerCallback ProgressHandler;

        int updatesCount = 0;

        public CurseforgeInstance(string instanceid, bool onlyBase_, ProgressHandlerCallback progressHandler)
        {
            InstanceId = instanceid;
            ProgressHandler = progressHandler;
            onlyBase = onlyBase_;
            installer = new CurseforgeInstaller(instanceid);
        }

        public InstanceInit Check()
        {
            ProgressHandler(1, 0, 0);
            Manifest = DataFilesManager.GetManifest(InstanceId, false);
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null || !Int32.TryParse(InfoData.id, out _))
            {
                return InstanceInit.CursforgeIdError;
            }

            // TODO: думаю, если манифест равен null, вполне можно продолжить работу скачав всё заново 
            if (Manifest == null || Manifest.version == null || Manifest.version.gameVersion == null)
            {
                return InstanceInit.VersionError;
            }

            if (Manifest.version.modloaderVersion != null && Manifest.version.modloaderVersion != "" && Manifest.version.modloaderType != ModloaderType.None)
            {
                BaseFilesIsCheckd = true;

                Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);

                if (Manifest != null)
                {
                    Updates = WithDirectory.GetLastUpdates(InstanceId);
                    updatesCount = installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

                    if (updatesCount == -1)
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

            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));

            return InstanceInit.Successful;
        }

        public InitData Update()
        {
            // асинхронно скачиваем иконку
            Lexplosion.Run.TaskRun(delegate () {
                try
                {
                    CurseforgeInstanceInfo info = CurseforgeApi.GetInstance(InfoData.id);
                    string dir = WithDirectory.DirectoryPath + "/instances-assets/" + InstanceId;
                  
                    InstanceAssets assets = new InstanceAssets();

                    if (info.attachments.Count > 0)
                    {
                        // TODO: написать где-то отдельную функцию для скачивания файла
                        string attachmentUrl = info.attachments[0].thumbnailUrl;
                        foreach (var attachment in info.attachments)
                        {
                            if (attachment.isDefault)
                            {
                                attachmentUrl = attachment.thumbnailUrl;
                            }
                        }

                        string[] a = attachmentUrl.Split('/');
                        string fileName = dir + "/" + a[a.Length - 1];

                        using (WebClient wc = new WebClient())
                        {
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            if (File.Exists(fileName)) // TODO: вылетает исключение о том что файл уже используется. видимо из-за того что этот файл используется интерфейсом
                            {
                                File.Delete(fileName);
                            }

                            wc.DownloadFile(attachmentUrl, fileName);
                        }

                        assets.mainImage = InstanceId + "/" + a[a.Length - 1];
                    }

                    //устанавливаем описание
                    assets.description = (info.summary != null) ? info.summary : "";

                    //устанавливаем автора
                    if (info.authors.Count > 0 && info.authors[0].name != null)
                    {
                        assets.author = info.authors[0].name;
                    }

                    assets.categories = info.categories; // устанавливаем теги

                    // сохраняем асетсы модпака
                    UserData.Instances.SetAssets(InstanceId, assets);
                    DataFilesManager.SaveFile(dir + "/assets.json", JsonConvert.SerializeObject(UserData.Instances.Assets[InstanceId]));
                }
                catch { }
            });

            var localFiles = DataFilesManager.GetFile<CurseforgeInstaller.LocalFiles>(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/localFiles.json"); //получем список всех файлов модпака

            if (localFiles == null)
            {
                localFiles = new CurseforgeInstaller.LocalFiles();
            }

            //нашелся id, который больше id установленной версии. Значит доступно обновление. Или же отсуствуют некоторые файлы модпака. Обновляем
            if (Info != null || installer.InvalidStruct(localFiles)) 
            {
                if (Info == null)
                {
                    Info = CurseforgeApi.GetInstanceInfo(InfoData.id, InfoData.instanceVersion); //получем информацию об этом модпаке

                    if (Info == null || Info.downloadUrl == null || Info.fileName == null)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.CursforgeIdError,
                        };
                    }
                }

                installer.MainFileDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    if (nowDataCount != 0)
                    {
                        ProgressHandler(3, 1, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                    }
                    else
                    {
                        ProgressHandler(3, 1, 0);
                    }
                };

                // скачиваем архив модпака и из него получаем манифест
                var manifest = installer.DownloadInstance(Info.downloadUrl, Info.fileName, ref localFiles);

                if (manifest == null || manifest.minecraft == null || manifest.minecraft.modLoaders == null || manifest.minecraft.version == null)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.ManifestError,
                    };
                }

                ProgressHandler(3, 2, 0);

                // Скачиваем основные файлы майкнрафта

                // если BaseFilesIsCheckd равно true, то это значтт что в манифесте уже была версия форджа
                if (!BaseFilesIsCheckd) // в данном случае в манифесте версии форджа не была и нам надо её получить
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

                    Console.WriteLine("modLoaderVersion " + modLoaderVersion);

                    Manifest = ToServer.GetVersionManifest(manifest.minecraft.version, modloader, modLoaderVersion);

                    if (Manifest != null)
                    {
                        DataFilesManager.SaveManifest(InstanceId, Manifest);

                        Updates = WithDirectory.GetLastUpdates(InstanceId);
                        updatesCount = installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

                        if (updatesCount == -1)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.GuardError,
                            };
                        }
                    }
                    else
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.ServerError,
                        };
                    }
                }

                if (updatesCount > 0)
                {
                    installer.ProcentUpdateEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        ProgressHandler(3, 2, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                    };
                }

                installer.UpdateBaseFiles(Manifest, ref Updates);
                ProgressHandler(3, 2, 100);

                installer.AddonsDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    if (nowDataCount != 0)
                    {
                        ProgressHandler(3, 3, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                    }
                    else
                    {
                        ProgressHandler(3, 3, 0);
                    }
                };

                // скачиваем аддоны
                List<string> errors = installer.InstallInstance(manifest, localFiles);

                if (errors.Count > 0)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.DownloadFilesError,
                        DownloadErrors = errors,
                    };
                }
            }
            else
            {
                if (BaseFilesIsCheckd)
                {
                    if (updatesCount > 0)
                    {
                        installer.ProcentUpdateEvent += delegate (int totalDataCount, int nowDataCount)
                        {
                            ProgressHandler(1, 1, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                        };
                    }

                    installer.UpdateBaseFiles(Manifest, ref Updates);
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

            return new InitData
            {
                InitResult = InstanceInit.Successful,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }
    }
}
