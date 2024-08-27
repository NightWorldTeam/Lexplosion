using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Installers
{
    abstract class ArchiveImportManager<TManifest> : IImportManager
    {
        private CancellationToken _cancelToken;
        private Settings _settings;

        /// <summary>
        /// Конструктор, автоматом определяющий делегат получения файла модпака.
        /// </summary>
        /// <param name="fileAddres">Путь до файла модпака.</param>
        /// <param name="isLocalPath">true если fileAddres содержит путь до локлаьного файла, false если fileAddres содержит url для скачивнаия файла</param>
        protected ArchiveImportManager(string fileAddres, bool isLocalPath, IArchivedInstanceInstaller<TManifest> installer, Settings globalSettings, CancellationToken cancelToken)
        {
            if (isLocalPath)
            {
                instanceFileGetter = (string tempDir, Func<string, TaskArgs> taskArgsGetter) => (true, fileAddres, Path.GetFileName(fileAddres));
            }
            else
            {
                instanceFileGetter = (string tempDir, Func<string, TaskArgs> taskArgsGetter) =>
                {
                    string fileName = "axaxa_ebala";
                    bool res = WithDirectory.DownloadFile(fileAddres, fileName, tempDir, taskArgsGetter(fileName));
                    return (res, tempDir + fileName, fileName);
                };
            }

            _cancelToken = cancelToken;
            _settings = globalSettings;
            this.installer = installer;
        }

        protected ArchiveImportManager(InstanceFileGetter instanceFileGetter, IArchivedInstanceInstaller<TManifest> installer, Settings globalSettings, CancellationToken cancelToken)
        {
            this.instanceFileGetter = instanceFileGetter;
            _cancelToken = cancelToken;
            _settings = globalSettings;
            this.installer = installer;
        }

        protected IArchivedInstanceInstaller<TManifest> installer;

        protected InstanceFileGetter instanceFileGetter { get; set; }
        protected VersionManifest versionManifest;
        protected InstanceContent instanceContent;
        protected TManifest manifest;

        protected static string GenerateTempId()
        {
            var rnd = new Random();
            string id = rnd.GenerateString(10);
            while (Directory.Exists(WithDirectory.GetInstancePath(id)))
            {
                id = rnd.GenerateString(10);
            }

            return id;
        }

        protected abstract bool ManifestIsValid(TManifest manifest);

        /// <summary>
        /// Должен определять тип майкрафта
        /// </summary>
        /// <param name="clienType">Тип клиента</param>
        /// <param name="modloaderVersion">Версия модлоадера (если его нет, то должна возвращаться пустая строка)</param>
        protected abstract void DetermineGameType(TManifest manifest, out ClientType clienType, out string modloaderVersion);

        protected abstract string DetermineGameVersion(TManifest manifest);

        protected abstract string DetermineInstanceName(TManifest manifest);

        public virtual InstanceInit Prepeare(ProgressHandlerCallback progressHandler, out PrepeareResult result)
        {
            result = new PrepeareResult();

            instanceContent = new InstanceContent();
            manifest = installer.Extraction(instanceFileGetter, ref instanceContent, _cancelToken);

            if (_cancelToken.IsCancellationRequested)
            {
                return InstanceInit.IsCancelled;
            }

            if (!ManifestIsValid(manifest))
            {
                Runtime.DebugWrite("Manifest is invalid");
                return InstanceInit.ManifestError;
            }

            progressHandler(StageType.Client, new ProgressHandlerArguments()
            {
                StagesCount = 3,
                Stage = 2,
                Procents = 0
            });

            DetermineGameType(manifest, out ClientType gameType, out string modLoaderVersion);
            Runtime.DebugWrite("modLoaderVersion " + modLoaderVersion);

            versionManifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    GameVersionInfo = new MinecraftVersion(DetermineGameVersion(manifest)),
                    ModloaderVersion = modLoaderVersion,
                    ModloaderType = gameType,
                    IsNightWorldClient = _settings.NwClientByDefault == true
                }
            };

            result.Name = DetermineInstanceName(manifest);

            return InstanceInit.Successful;
        }

        public virtual InstanceInit Import(ProgressHandlerCallback progressHandler, string instanceId, out IReadOnlyCollection<string> errors)
        {
            errors = null;
            if (versionManifest == null || instanceContent == null)
            {
                return InstanceInit.UnknownError;
            }

            errors = installer.Install(manifest, instanceContent, _cancelToken);
            if (errors.Count > 0)
            {
                return InstanceInit.DownloadFilesError;
            }

            DataFilesManager.SaveManifest(instanceId, versionManifest);
            return InstanceInit.Successful;
        }

        public void SetInstanceId(string id)
        {
            installer.SetInstanceId(id);
        }
    }
}
