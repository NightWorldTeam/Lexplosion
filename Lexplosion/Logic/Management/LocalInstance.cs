using System;
using System.Collections.Generic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management
{
    class LocalInstance : IPrototypeInstance
    {
        private VersionManifest Manifest;
        private LastUpdates Updates;
        private InstanceInstaller installer;

        private string InstanceId;
        private int stagesCount = 0;

        private ProgressHandlerCallback ProgressHandler;

        public LocalInstance(string instanceid, ProgressHandlerCallback progressHandler)
        {
            InstanceId = instanceid;
            ProgressHandler = progressHandler;
            installer = new InstanceInstaller(instanceid);
        }

        public InstanceInit Check()
        {
            //модпак локальный. получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
            Manifest = DataFilesManager.GetManifest(InstanceId, false);

            if (Manifest == null || Manifest.version == null || Manifest.version.gameVersion == null)
            {
                return InstanceInit.VersionError;
            }

            Manifest = ToServer.GetVersionManifest(Manifest.version.gameVersion, Manifest.version.modloaderType, Manifest.version.modloaderVersion);

            if (Manifest != null)
            {
                int updatesCount = 0;
                Updates = WithDirectory.GetLastUpdates(InstanceId);
                updatesCount = installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

                if (updatesCount == -1)
                {
                    installer.Release();
                    return InstanceInit.GuardError;
                }

                if (updatesCount > 0)
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

                installer.ProcentUpdateEvent += delegate (int totalDataCount, int nowDataCount)
                {
                    ProgressHandler(1, 1, (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100));
                };
            }

            // TODO: есть ли смысл его вызывать?
            List<string> errors = installer.UpdateBaseFiles(Manifest, ref Updates);
            installer.Release();
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
