using System;
using System.Collections.Generic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Installers
{
    class LocalInstallManager : IInstallManager
    {
        private VersionManifest Manifest;
        private LastUpdates Updates;
        private InstanceInstaller installer;

        private string InstanceId;
        private int stagesCount = 0;
        private int updatesCount = 0;

        public event Action<string, int, DownloadFileProgress> FileDownloadEvent
        {
            add
            {
                installer.FileDownloadEvent += value;
            }
            remove
            {
                installer.FileDownloadEvent -= value;
            }
        }

        public LocalInstallManager(string instanceid)
        {
            InstanceId = instanceid;
            installer = new InstanceInstaller(instanceid);
        }

        public InstanceInit Check(out long releaseIndex, string instanceVersion)
        {
            releaseIndex = 0;

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
                updatesCount = installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

                if (updatesCount == -1)
                {
                    return InstanceInit.GuardError;
                }

                if (updatesCount > 0)
                {
                    stagesCount = 1;
                }

                releaseIndex = Manifest.version.releaseIndex;
                return InstanceInit.Successful;
            }
            else
            {
                releaseIndex = Manifest.version.releaseIndex;
                return InstanceInit.Successful;
            }
        }

        public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
        {
            if (stagesCount == 1)
            {
                progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                {
                    StagesCount = 1,
                    Stage = 1,
                    Procents = 0
                });

                if (updatesCount > 1)
                {
                    installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 1,
                            Stage = 1,
                            Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
                            TotalFilesCount = totalDataCount,
                            FilesCount = nowDataCount
                        });
                    };
                }
                else
                {
                    installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
                    {
                        progressHandler(DownloadStageTypes.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 1,
                            Stage = 1,
                            Procents = pr,
                            TotalFilesCount = 1,
                            FilesCount = 0
                        });
                    };
                }
            }

            List<string> errors = installer.UpdateBaseFiles(Manifest, ref Updates, javaPath);
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
