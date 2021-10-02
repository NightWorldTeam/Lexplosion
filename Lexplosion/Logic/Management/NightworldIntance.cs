using System;
using System.Collections.Generic;
using System.IO;
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
        public class ModpackFilesUpdates
        {
            public Dictionary<string, List<string>> Data = new Dictionary<string, List<string>>(); //сюда записываем файлы, которые нужно обновить
            public List<string> OldFiles = new List<string>(); // список старых файлов, которые нуждаются в обновлении
            public bool Successful = true; // удачна или неудачна ли проверка
        }

        WithDirectory.BaseFilesUpdates BaseFiles;
        ModpackFilesUpdates VariableFiles;

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

        private bool InvalidStruct()
        {
            foreach(string file in Updates.Keys)
            {
                if (!File.Exists(file) && !Directory.Exists(file))
                {
                    return true;
                }
            }

            return false;
        }

        public InstanceInit Check()
        {
            ProgressHandler(0);
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json");

            if (InfoData == null || InfoData.id == null)
            {
                return InstanceInit.NightworldIdError;
            }
            ProgressHandler(1);

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
            ProgressHandler(2);

            if (!requiresUpdates)
            {
                VersionManifest manifest = DataFilesManager.GetManifest(InstanceId, false);
                if (manifest == null || manifest.version == null || manifest.version.gameVersion == null || manifest.version.gameVersion == "")
                {
                    NInstanceManifest manifest_ = NightWorldApi.GetInstanceManifest(InfoData.id);
                    if (manifest_ == null)
                    {
                        return InstanceInit.ServerError;
                    }

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
                        if (manifest_ == null)
                        {
                            return InstanceInit.ServerError;
                        }

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
                ProgressHandler(3);
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
                ProgressHandler(4);
            }

            if (Manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);

                BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление
                if (BaseFiles == null) 
                { 
                    return InstanceInit.GuardError; 
                }

                if (requiresUpdates || InvalidStruct())
                {
                    VariableFiles = WithDirectory.CheckNigntworldInstance(Manifest, InstanceId, ref Updates); // проверяем дополнительные файлы клиента (моды и прочее)
                    if (!VariableFiles.Successful)
                    {
                        return InstanceInit.GuardError;
                    }
                }
                ProgressHandler(5);

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
                errors = WithDirectory.UpdateNightworldInstance(VariableFiles, Manifest, InstanceId, InfoData.id, ref Updates);
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
