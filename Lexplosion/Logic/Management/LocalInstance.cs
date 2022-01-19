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
    class LocalInstance : IPrototypeInstance
    {
        WithDirectory.BaseFilesUpdates BaseFiles;

        VersionManifest Manifest;
        Dictionary<string, int> Updates;

        private string InstanceId;
        private int stagesCount = 0;

        private ManageLogic.ProgressHandlerDelegate ProgressHandler;

        public LocalInstance(string instanceid, ManageLogic.ProgressHandlerDelegate progressHandler)
        {
            InstanceId = instanceid;
            ProgressHandler = progressHandler;
        }

        public InstanceInit Check()
        {
            ProgressHandler(1, 0, 0);

            //модпак локальный. получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
            Manifest = DataFilesManager.GetManifest(InstanceId, false);

            if (Manifest == null || Manifest.version == null || Manifest.version.gameVersion == null)
            {
                return InstanceInit.VersionError;
            }

            Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);

            if (Manifest != null)
            {
                Updates = WithDirectory.GetLastUpdates(InstanceId);
                BaseFiles = WithDirectory.CheckBaseFiles(Manifest, InstanceId, ref Updates); // проверяем основные файлы клиента на обновление

                if(BaseFiles == null)
                {
                    return InstanceInit.GuardError;
                }

                if (BaseFiles.UpdatesCount > 0)
                {
                    stagesCount = 1;
                }

                return InstanceInit.Successful;
            }
            else
            {
                return InstanceInit.Successful;
            }

        }

        public InitData Update()
        {
            if (stagesCount == 1)
            {
                ProgressHandler(1, 1, 0);

                BaseFiles.ProcentUpdateFunc = delegate (int totalDataCount, int nowDataCount)
                {
                    ProgressHandler(1, 1, (nowDataCount / totalDataCount) * 100);
                };
            }
            else
            {
                BaseFiles.ProcentUpdateFunc = delegate (int totalDataCount, int nowDataCount) { };
            }

            List<string> errors = WithDirectory.UpdateBaseFiles(BaseFiles, Manifest, InstanceId, ref Updates);
            DataFilesManager.SaveManifest(InstanceId, Manifest);

            InstanceInit result = InstanceInit.Successful;
            if (errors.Count > 0)
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
