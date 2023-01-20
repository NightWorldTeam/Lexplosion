using System;
using System.Collections.Generic;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;
using Newtonsoft.Json;

namespace Lexplosion.Logic.Management.Installers
{
    class CurseforgeInstallManager : IInstallManager
    {
        private VersionManifest Manifest;
        private LastUpdates Updates;
        private CurseforgeFileInfo Info = null;
        private CurseforgeInstaller installer;

        private string InstanceId;
        private InstancePlatformData InfoData;

        private bool BaseFilesIsCheckd = false;
        private bool onlyBase;
        private CancellationToken _cancelToken;

        private int updatesCount = 0;

        public event Action<string, int, DownloadFileProgress> FileDownloadEvent
        {
            add
            {
                installer.FileDownloadEvent += value;
            }
            remove
            {
                installer.FileDownloadEvent -= value;
            }
        }

        public event Action DownloadStarted;

        public CurseforgeInstallManager(string instanceid, bool onlyBase_, CancellationToken cancelToken)
        {
            InstanceId = instanceid;
            onlyBase = onlyBase_;
            _cancelToken = cancelToken;
            installer = new CurseforgeInstaller(instanceid);
        }

        private bool _downloadStartedIsCalled = false;
        private void DownloadStartedCall()
        {
            if (!_downloadStartedIsCalled)
                DownloadStarted?.Invoke();
        }

        public InstanceInit Check(out long releaseIndex, string instanceVersion)
        {
            releaseIndex = 0;

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

            if (Manifest.version.modloaderVersion != null && Manifest.version.modloaderVersion != "" && Manifest.version.modloaderType != ClientType.Vanilla)
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
                    else if (updatesCount > 0)
                    {
                        DownloadStartedCall();
                    }
                }
                else
                {
                    return InstanceInit.ServerError;
                }
            }

            if (instanceVersion != null)
            {
                CurseforgeFileInfo ver = CurseforgeApi.GetProjectFile(InfoData.id, instanceVersion);
                if (ver != null)
                {
                    InfoData.instanceVersion = ver.id;
                    Info = ver;
                }
            }
            else if (!onlyBase)
            {
                List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetProjectFiles(InfoData.id); //получем информацию об этом модпаке

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

            releaseIndex = Manifest.version.releaseIndex;
            return InstanceInit.Successful;
        }

        public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
        {
            var localFiles = installer.GetInstanceContent(); //получем список всех файлов модпака

            //нашелся id, который больше id установленной версии. Значит доступно обновление. Или же отсуствуют некоторые файлы модпака. Обновляем
            if (Info != null || installer.InvalidStruct(localFiles))
            {
                DownloadStartedCall();

                if (Info == null)
                {
                    Info = CurseforgeApi.GetProjectFile(InfoData.id, InfoData.instanceVersion.ToString()); //получем информацию об этом модпаке

                    if (Info == null || Info.downloadUrl == null || Info.fileName == null)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.CursforgeIdError,
                        };
                    }
                }

                installer.MainFileDownloadEvent += delegate (int percent)
                {
                    progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                    {
                        StagesCount = 3,
                        Stage = 1,
                        Procents = percent,
                        TotalFilesCount = 1,
                        FilesCount = 0
                    });
                };

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                // скачиваем архив модпака и из него получаем манифест
                var manifest = installer.DownloadInstance(Info.downloadUrl, Info.fileName, ref localFiles, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                if (manifest == null || manifest.minecraft == null || manifest.minecraft.modLoaders == null || manifest.minecraft.version == null)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.ManifestError,
                        UpdatesAvailable = true,
                        ClientVersion = Info.id.ToString()
                    };
                }

                progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                {
                    StagesCount = 3,
                    Stage = 2,
                    Procents = 0
                });

                //определяем приоритетную версию модлоадера
                string modLoaderVersion = "";
                ClientType modloader = ClientType.Vanilla;
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
                        modloader = ClientType.Forge;
                        modLoaderVersion = modLoaderVersion.Replace("forge-", "");
                    }
                    else if (modLoaderVersion.Contains("fabric-"))
                    {
                        modloader = ClientType.Fabric;
                        modLoaderVersion = modLoaderVersion.Replace("fabric-", "");
                    }
                    else if (modLoaderVersion.Contains("fabric-"))
                    {
                        modloader = ClientType.Quilt;
                        modLoaderVersion = modLoaderVersion.Replace("quilt-", "");
                    }
                }

                Runtime.DebugWrite("modLoaderVersion " + modLoaderVersion);

                var versionData = Manifest?.version;
                bool baseFFilesIsUpdates = modLoaderVersion != versionData?.modloaderVersion || modloader != versionData?.modloaderType || manifest.minecraft.version != versionData?.gameVersion;

                // Скачиваем основные файлы майкнрафта

                // если BaseFilesIsCheckd равно true, то это значит что в манифесте уже была версия форджа
                if (!BaseFilesIsCheckd || baseFFilesIsUpdates) // в данном случае в манифесте версии форджа не была и нам надо её получить. Или же были измнения в модлоадере или версии игры
                {
                    Manifest = ToServer.GetVersionManifest(manifest.minecraft.version, modloader, modLoaderVersion);

                    if (Manifest != null)
                    {
                        DataFilesManager.SaveManifest(InstanceId, Manifest);

                        if (_cancelToken.IsCancellationRequested)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.IsCancelled
                            };
                        }

                        Updates = WithDirectory.GetLastUpdates(InstanceId);
                        updatesCount = installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

                        if (updatesCount == -1)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.GuardError,
                                UpdatesAvailable = true,
                                ClientVersion = Info.id.ToString()
                            };
                        }
                    }
                    else
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.ServerError,
                            UpdatesAvailable = true,
                            ClientVersion = Info.id.ToString()
                        };
                    }
                }

                Action<string, int, DownloadFileProgress> singleDownloadMethod = null;

                if (updatesCount > 1)
                {
                    installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 2,
                            Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
                            TotalFilesCount = totalDataCount,
                            FilesCount = nowDataCount
                        });
                    };
                }
                else if (updatesCount == 1)
                {
                    singleDownloadMethod = delegate (string file, int pr, DownloadFileProgress stage_)
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 2,
                            Procents = pr,
                            TotalFilesCount = 1,
                            FilesCount = 0
                        });
                    };

                    installer.FileDownloadEvent += singleDownloadMethod;
                }

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                List<string> errors = installer.UpdateBaseFiles(Manifest, ref Updates, javaPath, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                if (errors.Count > 0)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.DownloadFilesError,
                        DownloadErrors = errors,
                        UpdatesAvailable = true,
                        ClientVersion = Info.id.ToString()
                    };
                }

                progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                {
                    StagesCount = 3,
                    Stage = 2,
                    Procents = 100
                });

                if (singleDownloadMethod != null)
                {
                    installer.FileDownloadEvent -= singleDownloadMethod;
                }

                installer.AddonsDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    if (nowDataCount != 0)
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 3,
                            Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
                            TotalFilesCount = totalDataCount,
                            FilesCount = nowDataCount
                        });
                    }
                    else
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 3,
                            Procents = 0,
                            TotalFilesCount = totalDataCount,
                            FilesCount = nowDataCount
                        });
                    }
                };

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                // скачиваем аддоны
                errors = installer.InstallInstance(manifest, localFiles, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                if (errors.Count > 0)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.DownloadFilesError,
                        DownloadErrors = errors,
                        UpdatesAvailable = true,
                        ClientVersion = Info.id.ToString()
                    };
                }
            }
            else
            {
                if (BaseFilesIsCheckd)
                {
                    if (updatesCount > 1)
                    {
                        installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                        {
                            progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                            {
                                StagesCount = 1,
                                Stage = 1,
                                Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
                                TotalFilesCount = totalDataCount,
                                FilesCount = nowDataCount
                            });
                        };
                    }
                    else if (updatesCount == 1)
                    {
                        installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
                        {
                            progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                            {
                                StagesCount = 1,
                                Stage = 1,
                                Procents = pr,
                                TotalFilesCount = 1,
                                FilesCount = 0
                            });
                        };
                    }

                    if (_cancelToken.IsCancellationRequested)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.IsCancelled
                        };
                    }

                    List<string> errors = installer.UpdateBaseFiles(Manifest, ref Updates, javaPath, _cancelToken);

                    if (_cancelToken.IsCancellationRequested)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.IsCancelled
                        };
                    }

                    if (errors.Count > 0)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.DownloadFilesError,
                            DownloadErrors = errors,
                            UpdatesAvailable = true,
                            ClientVersion = Info?.id.ToString() ?? ""
                        };
                    }
                }
                else
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.ForgeVersionError,
                        UpdatesAvailable = true,
                        ClientVersion = Info?.id.ToString() ?? ""
                    };
                }
            }

            DataFilesManager.SaveManifest(InstanceId, Manifest);

            return new InitData
            {
                InitResult = InstanceInit.Successful,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries,
                UpdatesAvailable = false,
                ClientVersion = Info?.id.ToString() ?? ""
            };
        }
    }
}
