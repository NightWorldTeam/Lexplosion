using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management
{
    class NightworldIntance : IPrototypeInstance
    {
        private class NwInstancePlatformData : InstancePlatformData
        {
            public bool CustomVersion;
        }

        private NightWorldManifest nightworldManifest = null;
        private VersionManifest manifest = null;
        private LastUpdates Updates;
        private NightWorldInstaller installer;

        private string InstanceId;
        private NwInstancePlatformData InfoData;

        private bool requiresUpdates = true;
        private bool onlyBase;
        private int stagesCount = 0;
        private int baseFaliseUpdatesCount = 0;

        private int actualVersion = -1;

        public NightworldIntance(string instanceid, bool onlyBase_)
        {
            Console.WriteLine("onlyBase_ = " + onlyBase_);
            InstanceId = instanceid;
            onlyBase = onlyBase_;
            installer = new NightWorldInstaller(instanceid);
        }

        public InstanceInit Check(out string gameVersion)
        {
            gameVersion = "";
            InfoData = DataFilesManager.GetFile<NwInstancePlatformData>(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null)
            {
                return InstanceInit.NightworldIdError;
            }

            int version = 0;
            if (!onlyBase)
            {
                version = NightWorldApi.GetInstanceVersion(InfoData.id);
                requiresUpdates = version > InfoData.instanceVersion;
                actualVersion = version;
            }
            else
            {
                requiresUpdates = false;
            }

            if (!requiresUpdates)
            {
                VersionManifest manifest_ = DataFilesManager.GetManifest(InstanceId, false);
                if (manifest_ == null || manifest_.version == null || manifest_.version.gameVersion == null || manifest_.version.gameVersion == "")
                {
                    nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                    if (nightworldManifest == null)
                    {
                        return InstanceInit.ServerError;
                    }

                    var versionInfo = nightworldManifest.version;
                    if (nightworldManifest.CustomVersion)
                    {
                        manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                        InfoData.CustomVersion = true;
                        DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
                    }
                    else
                    {
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

                    nightworldManifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                    if (nightworldManifest == null)
                    {
                        return InstanceInit.ServerError;
                    }

                    if (manifest == null)
                    {
                        var mcVersion = nightworldManifest.version;
                        if (nightworldManifest.CustomVersion)
                        {
                            InfoData.CustomVersion = true;
                            DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
                            manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                        }
                        else
                        {
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

                var versionInfo = nightworldManifest.version;
                if (nightworldManifest.CustomVersion)
                {
                    InfoData.CustomVersion = true;
                    DataFilesManager.SaveFile(WithDirectory.DirectoryPath + "/instances/" + InstanceId + "/instancePlatformData.json", JsonConvert.SerializeObject(InfoData));
                    manifest = NightWorldApi.GetVersionManifest(InfoData.id);
                }
                else
                {
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

                requiresUpdates = (requiresUpdates || Updates.Count == 0);

                baseFaliseUpdatesCount = installer.CheckBaseFiles(manifest, ref Updates); // проверяем основные файлы клиента на обновление
                if (baseFaliseUpdatesCount == -1)
                {
                    return InstanceInit.GuardError;
                }

                if (baseFaliseUpdatesCount > 0)
                {
                    stagesCount++;
                }

                if (requiresUpdates || installer.InvalidStruct(Updates))
                {
                    int variableFilesUpdatesCount = 0;
                    variableFilesUpdatesCount = installer.CheckInstance(nightworldManifest, ref Updates); // проверяем дополнительные файлы клиента (моды и прочее)
                    if (variableFilesUpdatesCount == -1)
                    {
                        return InstanceInit.GuardError;
                    }

                    if (variableFilesUpdatesCount > 0)
                    {
                        stagesCount++;
                    }

                    requiresUpdates = true;
                }

                gameVersion = manifest.version.gameVersion;

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
            Console.WriteLine("NightWorld Update " + requiresUpdates);

            if (stagesCount > 0)
            {
                installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    progressHandler(DownloadStageTypes.Client, stagesCount, 1, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                };
            }

            if (baseFaliseUpdatesCount > 0)
            {
                progressHandler(DownloadStageTypes.Client, stagesCount, 1, 0);
            }

            List<string> errors_ = installer.UpdateBaseFiles(manifest, ref Updates, javaPath);
            List<string> errors = null;

            if (requiresUpdates)
            {
                int stage;
                if (baseFaliseUpdatesCount > 0)
                {
                    stage = 2;
                    progressHandler(DownloadStageTypes.Client, stagesCount, stage, 0);
                }
                else
                {
                    stage = 1;
                    progressHandler(DownloadStageTypes.Client, stagesCount, stage, 0);
                }

                installer.FilesDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    progressHandler(DownloadStageTypes.Client, stagesCount, stage, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                };

                errors = installer.UpdateInstance(nightworldManifest, InfoData.id, ref Updates);
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
                Libraries = manifest.libraries
            };
        }
    }
}
