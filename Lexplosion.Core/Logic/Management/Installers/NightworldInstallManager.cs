using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Installers
{
    class NightworldInstallManager : IInstallManager
    {
        private class NwInstancePlatformData : InstancePlatformData
        {
            public bool CustomVersion;
        }

        private NightWorldManifest nightworldManifest = null;
        private VersionManifest manifest = null;
        private LastUpdates Updates;
        private Dictionary<string, string> _instanceContent;
        private NightWorldInstaller installer;
        private CancellationToken _cancelToken;

        private string InstanceId;
        private NwInstancePlatformData InfoData;

        private bool _requiresUpdates = true;
        private bool _onlyBase;
        private int stagesCount = 0;
        private int _baseFaliseUpdatesCount = 0;
        private int _modpackFilesUpdatesCount = 0;

        private int actualVersion = -1;

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

        public NightworldInstallManager(string instanceid, bool onlyBase, CancellationToken cancelToken)
        {
            InstanceId = instanceid;
            _onlyBase = onlyBase;
            _cancelToken = cancelToken;
            installer = new NightWorldInstaller(instanceid);
        }

        private bool _downloadStartedIsCalled = false;
        private void DownloadStartedCall()
        {
            if (!_downloadStartedIsCalled)
                DownloadStarted?.Invoke();
        }

        public InstanceInit Check(out string javaVersionName, string instanceVersion)
        {
            javaVersionName = "";
            InfoData = DataFilesManager.GetExtendedPlatfromData<NwInstancePlatformData>(InstanceId);

            if (InfoData?.id == null)
            {
                return InstanceInit.NightworldIdError;
            }

            int version = 0;
            if (!_onlyBase)
            {
                version = NightWorldApi.GetInstanceVersion(InfoData.id);
                _requiresUpdates = version > InfoData.instanceVersion.ToInt32();
                actualVersion = version;
            }
            else
            {
                _requiresUpdates = false;
            }

            if (!_requiresUpdates)
            {
                VersionManifest manifest_ = DataFilesManager.GetManifest(InstanceId, false);
                if (string.IsNullOrWhiteSpace(manifest_?.version?.GameVersion))
                {
                    nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                    if (nightworldManifest == null)
                    {
                        // TODO: сделать как с локлаьными и курсфорджевкими сборками, чтобы при ошибке сеервера загружался локальный манифест
                        return InstanceInit.ServerError;
                    }

                    if (nightworldManifest.CustomVersion)
                    {
                        manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                        InfoData.CustomVersion = true;
                        DataFilesManager.SavePlatfromData(InstanceId, InfoData);
                    }
                    else
                    {
                        bool isNwClient = manifest_?.version?.IsNightWorldClient == true;
                        var versionInfo = nightworldManifest.version;
                        manifest = ToServer.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, isNwClient, versionInfo.modloaderVersion);
                    }

                    if (manifest == null)
                    {
                        return InstanceInit.ServerError;
                    }
                }
                else
                {
                    var versionInfo = manifest_.version;
                    if (InfoData.CustomVersion)
                    {
                        // TODO: здесь надо учитывать NightWorldClient. Он сейчас всегда выключаться будет
                        manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                    }
                    else
                    {
                        manifest = ToServer.GetVersionManifest(versionInfo.GameVersion, versionInfo.ModloaderType, versionInfo.IsNightWorldClient, versionInfo.ModloaderVersion);
                    }

                    if (manifest == null)
                    {
                        nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                        if (nightworldManifest == null)
                        {
                            return InstanceInit.ServerError;
                        }

                        if (nightworldManifest.CustomVersion)
                        {
                            InfoData.CustomVersion = true;
                            DataFilesManager.SavePlatfromData(InstanceId, InfoData);
                            manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                        }
                        else
                        {
                            var mcVersion = nightworldManifest.version;
                            bool isNwClient = versionInfo.IsNightWorldClient == true;
                            manifest = ToServer.GetVersionManifest(mcVersion.gameVersion, mcVersion.modloaderType, isNwClient, mcVersion.modloaderVersion);
                        }

                        if (manifest == null)
                        {
                            return InstanceInit.ServerError;
                        }
                    }
                }
            }
            else
            {
                nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                if (nightworldManifest?.version == null)
                {
                    return InstanceInit.ServerError;
                }

                VersionManifest manifest_ = DataFilesManager.GetManifest(InstanceId, false);
                bool isNwClient = manifest_?.version?.IsNightWorldClient == true;
                if (nightworldManifest.CustomVersion)
                {
                    // TODO: аналогично подобному месту вышле. Учитывать NightWorldClient
                    InfoData.CustomVersion = true;
                    DataFilesManager.SavePlatfromData(InstanceId, InfoData);
                    manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                }
                else
                {
                    var versionInfo = nightworldManifest.version;
                    manifest = ToServer.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, isNwClient, versionInfo.modloaderVersion);
                }

                if (manifest == null)
                {
                    return InstanceInit.ServerError;
                }
            }

            if (manifest != null)
            {
                Updates = DataFilesManager.GetLastUpdates(InstanceId);
                _instanceContent = installer.GetInstanceContent();

                _requiresUpdates = (_requiresUpdates || Updates.Count == 0);

                _baseFaliseUpdatesCount = installer.CheckBaseFiles(manifest, ref Updates); // проверяем основные файлы клиента на обновление
                if (_baseFaliseUpdatesCount == -1)
                {
                    return InstanceInit.GuardError;
                }

                if (_baseFaliseUpdatesCount > 0)
                {
                    DownloadStartedCall();
                    stagesCount++;
                }

                if (_requiresUpdates || installer.InvalidStruct(Updates, _instanceContent))
                {
                    if (nightworldManifest == null)
                    {
                        nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                        if (nightworldManifest == null)
                        {
                            return InstanceInit.ServerError;
                        }
                    }

                    _modpackFilesUpdatesCount = installer.CheckInstance(nightworldManifest, ref Updates, _instanceContent); // проверяем дополнительные файлы клиента (моды и прочее)
                    if (_modpackFilesUpdatesCount == -1)
                    {
                        return InstanceInit.GuardError;
                    }

                    if (_modpackFilesUpdatesCount > 0)
                    {
                        DownloadStartedCall();
                        stagesCount++;
                    }

                    _requiresUpdates = true;
                }

                javaVersionName = manifest.version.JavaVersionName;

                if (actualVersion == -1)
                {
                    int version_ = NightWorldApi.GetInstanceVersion(InfoData.id);
                    if (version_ > 0)
                        actualVersion = version_;
                }

                return InstanceInit.Successful;
            }
            else
            {
                return InstanceInit.ServerError;
            }
        }

        public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
        {
            Runtime.DebugWrite("NightWorld Update " + _requiresUpdates);

            Action<string, int, DownloadFileProgress> singleDownloadMethod = null;

            if (stagesCount > 0)
            {
                if (_baseFaliseUpdatesCount > 1)
                {
                    installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = stagesCount,
                            Stage = 1,
                            Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
                            TotalFilesCount = totalDataCount,
                            FilesCount = nowDataCount
                        });
                    };
                }
                else
                {
                    singleDownloadMethod = delegate (string file, int pr, DownloadFileProgress stage_)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = stagesCount,
                            Stage = 1,
                            Procents = pr,
                            TotalFilesCount = 1,
                            FilesCount = 0
                        });
                    };

                    installer.FileDownloadEvent += singleDownloadMethod;
                }
            }

            if (_baseFaliseUpdatesCount > 0)
            {
                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    StagesCount = stagesCount,
                    Stage = 1,
                    Procents = 0
                });
            }

            if (_cancelToken.IsCancellationRequested)
            {
                return new InitData
                {
                    InitResult = InstanceInit.IsCancelled
                };
            }

            List<string> errors_ = installer.UpdateBaseFiles(manifest, ref Updates, javaPath, _cancelToken);
            List<string> errors = null;

            if (_cancelToken.IsCancellationRequested)
            {
                return new InitData
                {
                    InitResult = InstanceInit.IsCancelled
                };
            }

            if (_requiresUpdates)
            {
                int stage;
                if (_baseFaliseUpdatesCount > 0)
                {
                    stage = 2;
                }
                else
                {
                    stage = 1;
                }

                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    StagesCount = stagesCount,
                    Stage = stage,
                    Procents = 0
                });

                if (_modpackFilesUpdatesCount > 1)
                {
                    if (singleDownloadMethod != null)
                    {
                        installer.FileDownloadEvent -= singleDownloadMethod;
                    }

                    installer.FilesDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = stagesCount,
                            Stage = stage,
                            Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
                            TotalFilesCount = totalDataCount,
                            FilesCount = nowDataCount
                        });
                    };
                }
                else if (singleDownloadMethod == null)
                {
                    installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = stagesCount,
                            Stage = stage,
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

                errors = installer.UpdateInstance(nightworldManifest, InfoData.id, ref Updates, _instanceContent, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    return new InitData
                    {
                        InitResult = InstanceInit.IsCancelled
                    };
                }
            }

            DataFilesManager.SaveManifest(InstanceId, manifest);

            if (errors != null)
            {
                foreach (string error in errors_)
                {
                    errors.Add(error);
                }
            }
            else
            {
                errors = errors_;
            }

            InstanceInit result;
            if (errors.Count > 0)
            {
                result = InstanceInit.DownloadFilesError;
            }
            else
            {
                result = InstanceInit.Successful;
                if (actualVersion != -1)
                {
                    InfoData.instanceVersion = actualVersion.ToString();
                }

                DataFilesManager.SavePlatfromData(InstanceId, InfoData);
            }

            return new InitData
            {
                InitResult = result,
                DownloadErrors = errors,
                VersionFile = manifest.version,
                Libraries = manifest.libraries,
                UpdatesAvailable = (result != InstanceInit.Successful)
            };
        }
    }
}
