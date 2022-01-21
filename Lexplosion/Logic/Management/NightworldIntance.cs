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

            public int UpdatesCount = 0;
            public delegate void ProcentUpdate(int totalDataCount, int nowDataCount);
            public ProcentUpdate ProcentUpdateFunc;
        }

        WithDirectory.BaseFilesUpdates BaseFiles;
        ModpackFilesUpdates VariableFiles;

        NInstanceManifest Manifest;
        Dictionary<string, int> Updates;

        private string InstanceId;
        public InstancePlatformData InfoData;

        private bool requiresUpdates = true;
        private bool onlyBase;
        private int stagesCount = 0;
        private ManageLogic.ProgressHandlerCallback ProgressHandler;

        public NightworldIntance(string instanceid, bool onlyBase_, ManageLogic.ProgressHandlerCallback progressHandler)
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
                    if (manifest_ == null)
                    {
                        return InstanceInit.ServerError;
                    }

                    VersionManifest tempManifest = ToServer.GetVersionManifest(manifest_.version.gameVersion, manifest_.version.modloaderType, manifest_.version.modloaderVersion);
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
                    VersionManifest tempManifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);
                    if (tempManifest == null)
                    {
                        NInstanceManifest manifest_ = NightWorldApi.GetInstanceManifest(InfoData.id);
                        if (manifest_ == null)
                        {
                            return InstanceInit.ServerError;
                        }

                        tempManifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);
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

                VersionManifest manifest_ = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);
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

                if(BaseFiles.UpdatesCount > 0)
                {
                    stagesCount++;
                }

                if (requiresUpdates || InvalidStruct())
                {
                    VariableFiles = WithDirectory.NightWorld.CheckInstance(Manifest, InstanceId, ref Updates); // проверяем дополнительные файлы клиента (моды и прочее)
                    if (!VariableFiles.Successful)
                    {
                        return InstanceInit.GuardError;
                    }

                    if (VariableFiles.UpdatesCount > 0)
                    {
                        stagesCount++;
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
            if (stagesCount > 0)
            {
                BaseFiles.ProcentUpdateFunc = delegate (int totalDataCount, int nowDataCount)
                {
                    ProgressHandler(stagesCount, 1, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                };
            }
            else
            {
                BaseFiles.ProcentUpdateFunc = delegate (int totalDataCount, int nowDataCount) { };
            }

            if(BaseFiles.UpdatesCount > 0)
            {
                ProgressHandler(stagesCount, 1, 0);
            }

            List<string> errors_ = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
            List<string> errors = null;

            if (requiresUpdates)
            {
                if (BaseFiles.UpdatesCount > 0)
                    ProgressHandler(stagesCount, 2, 0);
                else
                    ProgressHandler(stagesCount, 1, 0);
       
                errors = WithDirectory.NightWorld.UpdateInstance(VariableFiles, Manifest, InstanceId, InfoData.id, ref Updates);
            }

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
