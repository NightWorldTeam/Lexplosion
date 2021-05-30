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
    class LocalInstance : IPrototypeInstance
    {
        WithDirectory.BaseFilesUpdates BaseFiles;

        InstanceFiles Files;
        Dictionary<string, int> Updates;

        private string InstanceId;

        public LocalInstance(string instanceid)
        {
            InstanceId = instanceid;
        }

        public void Check()
        {
            //модпак локальный. получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
            Files = DataFilesManager.GetFilesList(InstanceId);
            Files = ToServer.GetFilesList(Files.version.gameVersion, true);

            if (Files != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);
                BaseFiles = WithDirectory.CheckBaseFiles(Files, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                // TODO: baseFiles может быть null, а VariableFiles содержать false

            }
            else
            {
                // TODO: возвращать ошибку
            }

        }

        public InitData Update()
        {
            List<string> errors = WithDirectory.UpdateBaseFiles(BaseFiles, Files, InstanceId, ref Updates);

            DataFilesManager.SaveFilesList(InstanceId, Files);

            return new InitData
            {
                Errors = errors,
                VersionFile = Files.version,
                Libraries = Files.libraries
            };
        }
    }
}
