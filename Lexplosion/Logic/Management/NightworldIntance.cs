using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management
{
    class NightworldIntance : IPrototypeInstance
    {
        WithDirectory.BaseFilesUpdates BaseFiles;
        WithDirectory.VariableFilesUpdates VariableFiles;

        NInstanceManifest Manifest;
        Dictionary<string, int> Updates;

        private string InstanceId;
        public InstancePlatformData InfoData;

        private bool requiresUpdates = true;
        private bool onlyBase;
        private static event ManageLogic.ProgressHandlerDelegate ProgressHandler;

        public NightworldIntance(string instanceid, bool onlyBase_, ManageLogic.ProgressHandlerDelegate progressHandler)
        {
            InstanceId = instanceid;
            ProgressHandler = progressHandler;
            onlyBase = onlyBase_;
        }

        public InstanceInit Check()
        {
            ProgressHandler(0);
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null)
            {
                return InstanceInit.NightworldIdError;
            }

            int version = 0;
            if (!onlyBase)
            {
                version = NightWorldApi.GetInstanceVersion(InfoData.id);
                requiresUpdates = version > InfoData.instanceVersion;
            }
            else
            {
                requiresUpdates = false;
            }

            if (!requiresUpdates)
            {
                VersionManifest manifest = DataFilesManager.GetManifest(InstanceId, false);
                if (manifest == null || manifest.version == null || manifest.version.gameVersion == null || manifest.version.gameVersion == "")
                {
                    NInstanceManifest manifest_ = NightWorldApi.GetInstanceManifest(InfoData.id);
                    VersionManifest tempManifest = ToServer.GetVersionManifest(manifest_.version.gameVersion, manifest_.version.forgeVersion);

                    if (tempManifest == null)
                    {
                        return InstanceInit.ServerError;
                    }

                    Manifest = new NInstanceManifest 
                    {
                        libraries = tempManifest.libraries,
                        natives = tempManifest.natives,
                        version = tempManifest.version
                    };
                }
                else
                {
                    VersionManifest tempManifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.forgeVersion);
                    if (tempManifest == null)
                    {
                        NInstanceManifest manifest_ = NightWorldApi.GetInstanceManifest(InfoData.id);
                        tempManifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.forgeVersion);

                        if (tempManifest == null)
                        {
                            return InstanceInit.ServerError;
                        }
                    }

                    Manifest = new NInstanceManifest
                    {
                        libraries = tempManifest.libraries,
                        natives = tempManifest.natives,
                        version = tempManifest.version
                    };
                }
            }
            else
            {
                Manifest = NightWorldApi.GetInstanceManifest(InfoData.id);
                if (Manifest == null || Manifest.version == null)
                {
                    return InstanceInit.ServerError;
                }

                VersionManifest manifest_ = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.forgeVersion);
                if (manifest_ == null)
                {
                    return InstanceInit.ServerError;
                }

                Manifest.version = manifest_.version;
                Manifest.libraries = manifest_.libraries;
                Manifest.natives = manifest_.natives;
            }

            if (Manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);

                BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление
                if (BaseFiles == null) 
                { 
                    return InstanceInit.GuardError; 
                }

                if (requiresUpdates)
                {
                    VariableFiles = WithDirectory.CheckVariableFiles(Manifest, InstanceId, ref Updates); // проверяем дополнительные файлы клиента (моды и прочее)
                    if (!VariableFiles.Successful)
                    {
                        return InstanceInit.GuardError;
                    }
                }

                return InstanceInit.Successful;
            }
            else
            {
                return InstanceInit.ServerError;
            }
        }

        public InitData Update()
        {
            ProgressHandler(20);
            List<string> errors_ = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
            List<string> errors = null;
            ProgressHandler(30);

            if (requiresUpdates)
            {
                errors = WithDirectory.UpdateVariableFiles(VariableFiles, Manifest, InstanceId, InfoData.id, ref Updates);
            }
            ProgressHandler(40);

            Manifest.data = null;
            Manifest.natives = null;

            DataFilesManager.SaveManifest(InstanceId, Manifest);

            if(errors != null)
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

            ProgressHandler(100);

            InstanceInit result = InstanceInit.Successful;
            if(errors.Count > 0)
            {
                result = InstanceInit.DownloadFilesError;
            }

            return new InitData
            {
                InitResult = result,
                DownloadErrors = errors,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }
    }
}
