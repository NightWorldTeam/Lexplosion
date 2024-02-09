using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Installers
{
    /// <summary>
    /// Описывает менеджер установки для инсталлеров, работающими со сборками в формате архивов.
    /// В таких сборках основные данные (модлоадер, версия игры и тп) хранятся в манифесете, который находится внутри архива. 
    /// И чтобы эти данные определить, нужно сначала скачать архив.
    /// </summary>
    /// <typeparam name="TInstaller">Тип инсталлера сборки</typeparam>
    /// <typeparam name="UManifest">Тип манифеста, который содержится в архиве</typeparam>
    /// <typeparam name="BProjectInfo">Тип, описывающий информацию о сборки, получаемую с сервера</typeparam>
    /// <typeparam name="CPlatformData">Тип, описывающий данные, которые будут храниться в файле instancePlatformData.json</typeparam>
    abstract class ArchiveInstallManager<TInstaller, UManifest, BProjectInfo, CPlatformData> : IInstallManager where TInstaller : InstanceInstaller, IArchivedInstanceInstaller<UManifest> where CPlatformData : InstancePlatformData
    {
        private VersionManifest Manifest;
        private LastUpdates Updates;
        private TInstaller _installer;

        private string InstanceId;
        private CPlatformData InfoData;
        protected BProjectInfo ProjectInfo;

        private bool BaseFilesIsCheckd = false;
        private bool onlyBase;
        private CancellationToken _cancelToken;

        private int updatesCount = 0;

        private bool _serverManifestIsNull = false;

        public event Action<string, int, DownloadFileProgress> FileDownloadEvent
        {
            add
            {
                _installer.FileDownloadEvent += value;
            }
            remove
            {
                _installer.FileDownloadEvent -= value;
            }
        }

        public event Action DownloadStarted;

        public ArchiveInstallManager(TInstaller installer, string instanceid, bool onlyBase_, CancellationToken cancelToken)
        {
            InstanceId = instanceid;
            onlyBase = onlyBase_;
            _cancelToken = cancelToken;
            _installer = installer;
        }

        /// <summary>
        /// Получает информацию о проекте.
        /// </summary>
        /// <param name="projectId">ID проекта</param>
        /// <param name="projectVersion">Версия проекта</param>
        /// <returns>
        /// Объект, содержащий нужную инфу о пректе. Если ничего не найдено, то null.
        /// </returns>
        protected abstract BProjectInfo GetProjectInfo(string projectId, string projectVersion);

        /// <summary>
        /// Аналогично GetProjectInfo, за исключением того, что он должнн возвращать версию по умочланию (обычно последнюю)
        /// </summary>
        /// /// <returns>
        /// Объект, содержащий нужную инфу о пректе. Если ничего не найдено, то null.
        /// </returns>
        protected abstract BProjectInfo GetProjectDefaultInfo(string projectId, string actualInstanceVersion);

        /// <summary>
        /// Должен возвращать версию проекта из его объъекта информации (объект, что возвращают GetProjectInfo и GetProjectDefaultInfo)
        /// </summary>
        protected abstract string GetProjectVersion(BProjectInfo projectData);

        protected abstract bool ManifestIsValid(UManifest manifest);

        /// <summary>
        /// Должен определять тип майкрафта
        /// </summary>
        /// <param name="clienType">Тип клиента</param>
        /// <param name="modloaderVersion">Версия модлоадера (если его нет, то должна возвращаться пустая строка)</param>
        protected abstract void DetermineGameType(UManifest manifest, out ClientType clienType, out string modloaderVersion);

        protected abstract string DetermineGameVersion(UManifest manifest);

        protected abstract bool LocalInfoIsValid(CPlatformData data);

        public abstract string ProjectId { get; }
        protected abstract bool ProfectInfoIsValid { get; }
        protected abstract string ArchiveDownloadUrl { get; }
        protected abstract string ArchiveFileName { get; }

        private bool _downloadStartedIsCalled = false;
        private void DownloadStartedCall()
        {
            if (!_downloadStartedIsCalled)
                DownloadStarted?.Invoke();
        }

        public InstanceInit Check(out string javaVersionName, string instanceVersion)
        {
            javaVersionName = string.Empty;

            Manifest = DataFilesManager.GetManifest(InstanceId, false);
            InfoData = DataFilesManager.GetFile<CPlatformData>(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (!LocalInfoIsValid(InfoData))
            {
                return InstanceInit.CurseforgeIdError;
            }

            // TODO: думаю, если манифест равен null, вполне можно продолжить работу скачав всё заново 
            if (string.IsNullOrWhiteSpace(Manifest?.version?.GameVersion))
            {
                return InstanceInit.VersionError;
            }

            if (!string.IsNullOrWhiteSpace(Manifest.version.ModloaderVersion) && Manifest.version.ModloaderType != ClientType.Vanilla)
            {
                BaseFilesIsCheckd = true;

                var ver = Manifest.version;
                Manifest = ToServer.GetVersionManifest(ver.GameVersion, ver.ModloaderType, ver.IsNightWorldClient, ver.ModloaderVersion);

                if (Manifest != null)
                {
                    Updates = WithDirectory.GetLastUpdates(InstanceId);
                    updatesCount = _installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

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
                    Runtime.DebugWrite("Manifest from server is null. Load local manifest");
                    _serverManifestIsNull = true;
                    Manifest = DataFilesManager.GetManifest(InstanceId, true);

                    if (string.IsNullOrWhiteSpace(Manifest?.version?.GameVersion))
                    {
                        Runtime.DebugWrite("Local manifest is null (" + (Manifest == null) + ", " + (Manifest?.version == null) + ")");
                        return InstanceInit.VersionError;
                    }

                    javaVersionName = Manifest.version.JavaVersionName ?? string.Empty;
                    return InstanceInit.Successful;
                }
            }

            // если версия сборки определена, то получаем инфу о ней
            BProjectInfo info = default;
            if (instanceVersion != null)
            {
                info = GetProjectInfo(InfoData.id, instanceVersion);
            }
            else if (!onlyBase) //версия сборки не определена. Получаем версию по умолчанию
            {
                info = GetProjectDefaultInfo(InfoData.id, InfoData.instanceVersion);
            }

            if (info != null)
            {
                ProjectInfo = info;
                InfoData.instanceVersion = GetProjectVersion(info);
            }

            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));

            javaVersionName = Manifest.version.JavaVersionName ?? string.Empty;
            return InstanceInit.Successful;
        }

        public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
        {
            if (_serverManifestIsNull)
            {
                return new InitData
                {
                    InitResult = InstanceInit.Successful,
                    VersionFile = Manifest.version,
                    Libraries = Manifest.libraries,
                    UpdatesAvailable = false,
                    ClientVersion = ProjectId ?? string.Empty
                };
            }

            var localFiles = _installer.GetInstanceContent(); //получем список всех файлов модпака

            //нашелся id, который больше id установленной версии. Значит доступно обновление. Или же отсуствуют некоторые файлы модпака. Обновляем
            if (ProjectInfo != null || _installer.InvalidStruct(localFiles))
            {
                DownloadStartedCall();

                if (ProjectInfo == null)
                {
                    ProjectInfo = GetProjectInfo(InfoData.id, InfoData.instanceVersion); //получем информацию об этом модпаке

                    if (!ProfectInfoIsValid)
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.CurseforgeIdError,
                        };
                    }
                }

                _installer.MainFileDownloadEvent += delegate (int percent)
                {
                    progressHandler(StageType.Client, new ProgressHandlerArguments()
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
                var manifest = _installer.DownloadInstance(ArchiveDownloadUrl, ArchiveFileName, ref localFiles, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                if (!ManifestIsValid(manifest))
                {
                    Runtime.DebugWrite("Manifest is invalid");
                    return new InitData
                    {
                        InitResult = InstanceInit.ManifestError,
                        UpdatesAvailable = true,
                        ClientVersion = ProjectId
                    };
                }

                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    StagesCount = 3,
                    Stage = 2,
                    Procents = 0
                });

                DetermineGameType(manifest, out ClientType gameType, out string modLoaderVersion);
                Runtime.DebugWrite("modLoaderVersion " + modLoaderVersion);

                var versionData = Manifest?.version;
                bool baseFFilesIsUpdates = modLoaderVersion != versionData?.ModloaderVersion || gameType != versionData?.ModloaderType || DetermineGameVersion(manifest) != versionData?.GameVersion;

                // Скачиваем основные файлы майкнрафта

                // если BaseFilesIsCheckd равно true, то это значит что в манифесте уже была версия форджа
                if (!BaseFilesIsCheckd || baseFFilesIsUpdates) // в данном случае в манифесте версии форджа не была и нам надо её получить. Или же были измнения в модлоадере или версии игры
                {
                    bool isNwClient = versionData?.IsNightWorldClient == true;
                    Manifest = ToServer.GetVersionManifest(DetermineGameVersion(manifest), gameType, isNwClient, modLoaderVersion);

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
                        updatesCount = _installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

                        if (updatesCount == -1)
                        {
                            return new InitData
                            {
                                InitResult = InstanceInit.GuardError,
                                UpdatesAvailable = true,
                                ClientVersion = ProjectId
                            };
                        }
                    }
                    else
                    {
                        return new InitData
                        {
                            InitResult = InstanceInit.ServerError,
                            UpdatesAvailable = true,
                            ClientVersion = ProjectId
                        };
                    }
                }

                Action<string, int, DownloadFileProgress> singleDownloadMethod = null;

                if (updatesCount > 1)
                {
                    _installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
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
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 2,
                            Procents = pr,
                            TotalFilesCount = 1,
                            FilesCount = 0
                        });
                    };

                    _installer.FileDownloadEvent += singleDownloadMethod;
                }

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }

                List<string> errors = _installer.UpdateBaseFiles(Manifest, ref Updates, javaPath, _cancelToken);

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
                        ClientVersion = ProjectId
                    };
                }

                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    StagesCount = 3,
                    Stage = 2,
                    Procents = 100
                });

                if (singleDownloadMethod != null)
                {
                    _installer.FileDownloadEvent -= singleDownloadMethod;
                }

                _installer.AddonsDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    if (nowDataCount != 0)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
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
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
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
                errors = _installer.InstallInstance(manifest, localFiles, _cancelToken);

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
                        ClientVersion = ProjectId
                    };
                }
            }
            else
            {
                if (BaseFilesIsCheckd)
                {
                    if (updatesCount > 1)
                    {
                        _installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                        {
                            progressHandler(StageType.Client, new ProgressHandlerArguments()
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
                        _installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
                        {
                            progressHandler(StageType.Client, new ProgressHandlerArguments()
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

                    List<string> errors = _installer.UpdateBaseFiles(Manifest, ref Updates, javaPath, _cancelToken);

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
                            ClientVersion = ProjectId ?? ""
                        };
                    }
                }
                else
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.ForgeVersionError,
                        UpdatesAvailable = true,
                        ClientVersion = ProjectId ?? ""
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
                ClientVersion = ProjectId ?? String.Empty
            };
        }
    }
}
