using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management
{
    class NightworldIntance : IPrototypeInstance
    {
        WithDirectory.BaseFilesUpdates BaseFiles;
        WithDirectory.VariableFilesUpdates VariableFiles;

        InstanceFiles Files;
        Dictionary<string, int> Updates;

        private string InstanceId;

        public NightworldIntance(string instanceid)
        {
            InstanceId = instanceid;
        }

        public void Check()
        {
            Files = ToServer.GetFilesList(InstanceId, false);
            if(Files != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);

                BaseFiles = WithDirectory.CheckBaseFiles(Files, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление
                VariableFiles = WithDirectory.CheckVariableFiles(Files, InstanceId, ref Updates); // проверяем дополнительные файлы клиента (моды и прочее)

                // TODO: baseFiles может быть null, а VariableFiles содержать false

            }
            else
            {
                // TODO: возвращать ошибку 
            }
        }

        public InitData Update()
        {
            List<string> errors_ = WithDirectory.UpdateBaseFiles(BaseFiles, Files, InstanceId, ref Updates);
            List<string> errors = WithDirectory.UpdateVariableFiles(VariableFiles, Files, InstanceId, ref Updates);

            Files.data = null;
            Files.natives = null;

            DataFilesManager.SaveFilesList(InstanceId, Files);

            foreach (string error in errors_)
            {
                errors.Add(error);
            }

            return new InitData
            {
                Errors = errors,
                VersionFile = Files.version,
                Libraries = Files.libraries
            };
        }
    }
}
