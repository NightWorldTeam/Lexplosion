using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.Logic.Management.Import.Importers
{
    internal class SimpleArchiveImportManager : IImportManager
    {
        private readonly string _fileAddres;
        private readonly Settings _settings;
        private readonly Guid _importId;
        private readonly CancellationToken _cancelToken;
        private readonly DynamicStateHandler<ImportInterruption, InterruptionType> _interruptionHandler;
        private readonly WithDirectory _withDirectory;
        private readonly DataFilesManager _dataFilesManager;

        private VersionManifest _versionManifest;
        private string _localId;
        private string _unzipPath;

        public SimpleArchiveImportManager(string fileAddres, Settings settings, IFileServicesContainer services, Guid importId, CancellationToken cancelToken, DynamicStateHandler<ImportInterruption, InterruptionType> interruptionHandler)
        {
            _fileAddres = fileAddres;
            _settings = settings;
            _importId = importId;
            _cancelToken = cancelToken;
            _interruptionHandler = interruptionHandler;
            _withDirectory = services.DirectoryService;
            _dataFilesManager = services.DataFilesService;
        }

        public InstanceInit Import(ProgressHandler progressHandler, out IReadOnlyCollection<string> errors)
        {
            errors = new List<string>();
            InstanceInit result = _withDirectory.MoveUnpackedInstance(_localId, _unzipPath);
            if (result != InstanceInit.Successful)
            {
                try
                {
                    string dir = _withDirectory.GetInstancePath(_localId);
                    if (Directory.Exists(dir)) Directory.Delete(dir, true);
                }
                catch { }

                return result;
            }

            _dataFilesManager.SaveManifest(_localId, _versionManifest);

            return InstanceInit.Successful;
        }

        public InstanceInit Prepeare(ProgressHandler progressHandler, out PrepeareResult result)
        {
            result = new PrepeareResult();

            progressHandler(StateType.DownloadClient, new ProgressHandlerArguments()
            {
                StagesCount = 2,
                Stage = 1,
                Procents = 0
            });

            _unzipPath = _withDirectory.CreateTempDir() + "import/";
            string activeUnzipPath = _unzipPath + "files";

            try
            {
                if (!Directory.Exists(activeUnzipPath))
                {
                    Directory.CreateDirectory(activeUnzipPath);
                }
                else
                {
                    Directory.Delete(activeUnzipPath, true);
                }
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return InstanceInit.DirectoryCreateError;
            }

            try
            {
                ZipFile.ExtractToDirectory(_fileAddres, activeUnzipPath);

                // во избежание приколов удаляеем файл manifest.json, если он есть
                string manifestFile = activeUnzipPath + "/" + DataFilesManager.MANIFEST_FILE;
                if (File.Exists(manifestFile)) File.Delete(manifestFile);
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
                return InstanceInit.ZipFileOpenError;
            }

            //Вызываем эвент, говорящий что нам нужны данные для сборки
            var interruption = new ImportInterruption(_importId);
            _interruptionHandler.ChangeState(interruption, InterruptionType.BasicDataRequired);

            BaseInstanceData data = interruption.BaseData;
            if (_cancelToken.IsCancellationRequested) return InstanceInit.IsCancelled;

            result.Name = data.Name ?? "Pochemy";
            result.Author = data.Author;
            result.Description = data.Description;
            result.Summary = data.Summary;
            result.GameVersionInfo = data.GameVersion;

            _versionManifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    GameVersionInfo = data.GameVersion,
                    ModloaderVersion = data.ModloaderVersion,
                    ModloaderType = data.Modloader,
                    IsNightWorldClient = _settings.NwClientByDefault == true
                }
            };

            return InstanceInit.Successful;
        }

        public void SetInstanceId(string id)
        {
            _localId = id;
        }
    }
}
