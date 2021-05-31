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

        VersionManifest Manifest;
        Dictionary<string, int> Updates;

        private string InstanceId;

        public LocalInstance(string instanceid)
        {
            InstanceId = instanceid;
        }

        public void Check()
        {
            //модпак локальный. получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
            Manifest = DataFilesManager.GetFilesList(InstanceId);
            Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion);

            if (Manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);
                BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                // TODO: baseFiles может быть null, а VariableFiles содержать false

            }
            else
            {
                // TODO: возвращать ошибку
            }

        }

        public InitData Update()
        {
            List<string> errors = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);

            DataFilesManager.SaveFilesList(InstanceId, Manifest);

            return new InitData
            {
                Errors = errors,
                VersionFile = Manifest.version,
                Libraries = Manifest.libraries
            };
        }
    }
}
