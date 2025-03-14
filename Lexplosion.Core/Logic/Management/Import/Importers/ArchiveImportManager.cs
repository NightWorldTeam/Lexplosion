using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Import.Importers
{
    abstract class ArchiveImportManager<TManifest> : IImportManager
    {
        private CancellationToken _cancelToken;
        private Settings _settings;

        protected string instanceId;
        protected string fileAddres;

        /// <summary>
        /// Конструктор, автоматом определяющий делегат получения файла модпака.
        /// </summary>
        /// <param name="fileAddres">Путь до файла модпака.</param>
        protected ArchiveImportManager(string fileAddres, IArchivedInstanceInstaller<TManifest> installer, Settings globalSettings, CancellationToken cancelToken)
        {
            this.fileAddres = fileAddres;
            _cancelToken = cancelToken;
            _settings = globalSettings;
            this.installer = installer;
        }

        protected IArchivedInstanceInstaller<TManifest> installer;

        protected VersionManifest versionManifest;
        protected TManifest manifest;

        public int CompletedStagesCount { private get; set; }

        protected abstract bool ManifestIsValid(TManifest manifest);

        /// <summary>
        /// Должен определять тип майкрафта
        /// </summary>
        /// <param name="clienType">Тип клиента</param>
        /// <param name="modloaderVersion">Версия модлоадера (если его нет, то должна возвращаться пустая строка)</param>
        protected abstract void DetermineGameType(TManifest manifest, out ClientType clienType, out string modloaderVersion);

        protected abstract string DetermineGameVersion(TManifest manifest);

        protected abstract string DetermineInstanceName(TManifest manifest);

        protected abstract string DetermineSummary(TManifest manifest);

        protected abstract string DetermineAthor(TManifest manifest);

        public virtual ImportResult Prepeare(ProgressHandlerCallback progressHandler, out PrepeareResult result)
        {
            result = new PrepeareResult();

            progressHandler(StageType.Client, new ProgressHandlerArguments()
            {
                StagesCount = CompletedStagesCount + 2,
                Stage = CompletedStagesCount + 1,
                Procents = 0
            });

            installer.MainFileDownload += (int pr) =>
            {
                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    StagesCount = CompletedStagesCount + 2,
                    Stage = CompletedStagesCount + 1,
                    Procents = pr
                });
            };

            InstanceFileGetter instanceFileGetter = (string tempDir, Func<string, TaskArgs> taskArgsGetter) => (true, fileAddres, Path.GetFileName(fileAddres));

            manifest = installer.Extraction(instanceFileGetter, _cancelToken);

            if (_cancelToken.IsCancellationRequested)
            {
                return ImportResult.Canceled;
            }

            if (!ManifestIsValid(manifest))
            {
                Runtime.DebugWrite("Manifest is invalid");
                return ImportResult.ManifestError;
            }

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
            result.Summary = DetermineSummary(manifest);
            result.Author = DetermineAthor(manifest);

            return ImportResult.Successful;
        }

        public virtual ImportResult Import(ProgressHandlerCallback progressHandler, out IReadOnlyCollection<string> errors)
        {
            errors = null;
            if (versionManifest == null)
            {
                return ImportResult.UnknownError;
            }

            installer.AddonsDownload += (int totalDataCount, int nowDataCount) =>
            {
                progressHandler(StageType.Client, new ProgressHandlerArguments()
                {
                    TotalFilesCount = totalDataCount,
                    FilesCount = nowDataCount,
                    Procents = (((nowDataCount != 0 ? nowDataCount : 1) * 100) / totalDataCount),
                    Stage = CompletedStagesCount + 2,
                    StagesCount = CompletedStagesCount + 2
                });
            };

            var instanceContent = new InstanceContent();
            bool result = installer.HandleExtractedFiles(ref instanceContent, _cancelToken);
            if (!result || instanceContent == null)
            {
                return ImportResult.MovingFilesError;
            }

            errors = installer.Install(manifest, instanceContent, _cancelToken);
            if (errors.Count > 0)
            {
                return ImportResult.DownloadError;
            }

            DataFilesManager.SaveManifest(instanceId, versionManifest);
            return ImportResult.Successful;
        }

        public void SetInstanceId(string id)
        {
            instanceId = id;
            installer.SetInstanceId(id);
        }
    }
}
