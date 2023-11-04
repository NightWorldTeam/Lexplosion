using System;
using System.Collections.Generic;
using System.Threading;
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
        private CancellationToken _cancelToken;

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

        public event Action DownloadStarted;

        public LocalInstallManager(string instanceid, CancellationToken cancelToken)
        {
            InstanceId = instanceid;
            _cancelToken = cancelToken;
            installer = new InstanceInstaller(instanceid);
        }

        public InstanceInit Check(out string javaVersionName, string instanceVersion)
        {
            javaVersionName = "";

            //модпак локальный. получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
            Manifest = DataFilesManager.GetManifest(InstanceId, false);

            if (Manifest == null || Manifest.version == null || Manifest.version.GameVersion == null)
            {
                return InstanceInit.ManifestError;
            }

            var gameVersion = Manifest.version.GameVersion;
            var modloaderType = Manifest.version.ModloaderType;
            var modloaderVersion = Manifest.version.ModloaderVersion;
            var optifineVersion = Manifest.version.AdditionalInstaller?.installerVersion;
            var isNwClient = Manifest.version.IsNightWorldClient;

            Manifest = ToServer.GetVersionManifest(gameVersion, modloaderType, isNwClient, modloaderVersion, optifineVersion);

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
                    DownloadStarted?.Invoke();
                }

                javaVersionName = Manifest.version.JavaVersionName;
                return InstanceInit.Successful;
            }
            else
            {
                return InstanceInit.ManifestError;
            }
        }

        public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
        {
            if (stagesCount == 1)
            {
                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    StagesCount = 1,
                    Stage = 1,
                    Procents = 0
                });

                if (updatesCount > 1)
                {
                    installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
                    {
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
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
                        progressHandler(StageType.Client, new ProgressHandlerArguments()
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

            if (_cancelToken.IsCancellationRequested)
            {
                return new InitData
                {
                    InitResult = InstanceInit.IsCancelled
                };
            }

            List<string> errors = installer.UpdateBaseFiles(Manifest, ref Updates, javaPath, _cancelToken);
            DataFilesManager.SaveManifest(InstanceId, Manifest);

            if (_cancelToken.IsCancellationRequested)
            {
                return new InitData
                {
                    InitResult = InstanceInit.IsCancelled
                };
            }

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
                Libraries = Manifest.libraries,
                UpdatesAvailable = (result != InstanceInit.Successful)
            };
        }
    }
}
