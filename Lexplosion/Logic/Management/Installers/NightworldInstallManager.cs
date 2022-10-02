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
        private Dictionary<string, int> _instanceContent;
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
            Console.WriteLine("onlyBase = " + onlyBase);
            InstanceId = instanceid;
            _onlyBase = onlyBase;
            _cancelToken = cancelToken;
            installer = new NightWorldInstaller(instanceid);
        }

        private bool _downloadStartedIsCalled = false;
        private void DownloadStartedCall()
        {
            if(!_downloadStartedIsCalled)
                DownloadStarted?.Invoke();
        }

        public InstanceInit Check(out long releaseIndex, string instanceVersion)
        {
            releaseIndex = 0;
            InfoData = DataFilesManager.GetFile<NwInstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null)
            {
                return InstanceInit.NightworldIdError;
            }

            int version = 0;
            if (!_onlyBase)
            {
                version = NightWorldApi.GetInstanceVersion(InfoData.id);
                _requiresUpdates = version > InfoData.instanceVersion;
                actualVersion = version;
            }
            else
            {
                _requiresUpdates = false;
            }

            if (!_requiresUpdates)
            {
                VersionManifest manifest_ = DataFilesManager.GetManifest(InstanceId, false);
                if (manifest_ == null || manifest_.version == null || manifest_.version.gameVersion == null || manifest_.version.gameVersion == "")
                {
                    nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                    if (nightworldManifest == null)
                    {
                        return InstanceInit.ServerError;
                    }

                    if (nightworldManifest.CustomVersion)
                    {
                        manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                        InfoData.CustomVersion = true;
                        DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
                    }
                    else
                    {
                        var versionInfo = nightworldManifest.version;
                        manifest = ToServer.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, versionInfo.modloaderVersion);
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
                        manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                    }
                    else
                    {
                        manifest = ToServer.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, versionInfo.modloaderVersion);
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
                            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
                            manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                        }
                        else
                        {
                            var mcVersion = nightworldManifest.version;
                            manifest = ToServer.GetVersionManifest(mcVersion.gameVersion, mcVersion.modloaderType, mcVersion.modloaderVersion);
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
                if (nightworldManifest == null || nightworldManifest.version == null)
                {
                    return InstanceInit.ServerError;
                }

                if (nightworldManifest.CustomVersion)
                {
                    InfoData.CustomVersion = true;
                    DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
                    manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                }
                else
                {
                    var versionInfo = nightworldManifest.version;
                    manifest = ToServer.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, versionInfo.modloaderVersion);
                }

                if (manifest == null)
                {
                    return InstanceInit.ServerError;
                }
            }

            if (manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);
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

                releaseIndex = manifest.version.releaseIndex;

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
            Console.WriteLine("NightWorld Update " + _requiresUpdates);

            Action<string, int, DownloadFileProgress> singleDownloadMethod = null;

            if (stagesCount > 0)
            {
                if (_baseFaliseUpdatesCount > 1)
                {
                    installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
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
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
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
                progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
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

                progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
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
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
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
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
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
                    InfoData.instanceVersion = actualVersion;
                }

                DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
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
