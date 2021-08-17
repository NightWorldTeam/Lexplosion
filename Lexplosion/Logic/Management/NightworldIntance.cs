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

        public NightworldIntance(string instanceid)
        {
            InstanceId = instanceid;
        }

        public string Check()
        {
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json");
            if (InfoData == null || InfoData.id == null)
            {
                return "nightworldIdError";
            }

            Manifest = ToServer.GetInstanceManifest(InfoData.id);

            if (Manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);

                BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление
                if (BaseFiles == null) 
                { 
                    return "guardError"; 
                }

                VariableFiles = WithDirectory.CheckVariableFiles(Manifest, InstanceId, ref Updates); // проверяем дополнительные файлы клиента (моды и прочее)
                if (!VariableFiles.Successful) 
                { 
                    return "guardError"; 
                }

                return "";
            }
            else
            {
                return "serverError";
            }
        }

        public InitData Update()
        {
            List<string> errors_ = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
            List<string> errors = WithDirectory.UpdateVariableFiles(VariableFiles, Manifest, InstanceId, ref Updates);

            Manifest.data = null;
            Manifest.natives = null;

            DataFilesManager.SaveManifest(InstanceId, Manifest);

            foreach (string error in errors_)
            {
                errors.Add(error);
            }

            return new InitData
            {
                Errors = errors,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }

        public string CheckOnlyBase()
        {
            InfoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.directory + "/instances/" + InstanceId + "/instancePlatformData.json");
            if (InfoData == null || InfoData.id == null)
            {
                return "nightworldIdError";
            }

            Manifest = ToServer.GetInstanceManifest(InfoData.id);

            if (Manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);

                BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление
                if (BaseFiles == null)
                {
                    return "guardError";
                }

                return "";
            }
            else
            {
                return "serverError";
            }

        }

        public InitData UpdateOnlyBase()
        {
            List<string> errors = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);

            Manifest.data = null;
            Manifest.natives = null;

            DataFilesManager.SaveManifest(InstanceId, Manifest);

            return new InitData
            {
                Errors = errors,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };

        }
    }
}
