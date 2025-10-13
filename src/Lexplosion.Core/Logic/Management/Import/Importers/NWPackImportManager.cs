using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Objects.CommonClientData;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Lexplosion.Logic.Management.Import.Importers
{
    class NWPackImportManager : IImportManager
    {
        private string _fileAddres;
        private Settings _settings;
        private readonly INightWorldFileServicesContainer _services;
        private readonly WithDirectory _withDirectory;
        private readonly DataFilesManager _dataFilesManager;
        private CancellationToken _cancellationToken;

        private VersionManifest _versionManifest;
        private string _localId;
        private string _unzipPath;

        public NWPackImportManager(string fileAddres, Settings settings, INightWorldFileServicesContainer services, CancellationToken cancelToken)
        {
            _fileAddres = fileAddres;
            _settings = settings;
            _services = services;
            _cancellationToken = cancelToken;
            _withDirectory = services.DirectoryService;
            _dataFilesManager = services.DataFilesService;
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

            InstanceInit res = _withDirectory.UnzipInstance(_fileAddres, out _unzipPath);
            var parameters = _dataFilesManager.GetFile<ArchivedClientData>($"{_unzipPath}instanceInfo.json");

            if (res != InstanceInit.Successful) return res;

            if (parameters?.GameVersionInfo?.IsNan != false)
            {
                Runtime.DebugWrite("GameVersionError");
                return InstanceInit.GameVersionError;
            }

            _versionManifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    GameVersionInfo = parameters.GameVersionInfo,
                    ModloaderVersion = parameters.ModloaderVersion,
                    ModloaderType = parameters.ModloaderType,
                    IsNightWorldClient = _settings.NwClientByDefault == true
                }
            };

            if (parameters.AdditionalInstallerType != null && !string.IsNullOrWhiteSpace(parameters.AdditionalInstallerVersion))
            {
                _versionManifest.version.AdditionalInstaller = new AdditionalInstaller()
                {
                    type = parameters.AdditionalInstallerType ?? AdditionalInstallerType.Optifine,
                    installerVersion = parameters.AdditionalInstallerVersion
                };
            }

            result.Name = parameters.Name;
            result.Description = parameters.Description;
            result.Author = parameters.Author;
            result.Summary = parameters.Summary;
            result.GameVersionInfo = parameters.GameVersionInfo;
            result.LogoPath = _unzipPath + parameters.LogoFileName;

            return InstanceInit.Successful;
        }

        public InstanceInit Import(ProgressHandler progressHandler, out IReadOnlyCollection<string> errors)
        {
            progressHandler(StateType.DownloadClient, new ProgressHandlerArguments()
            {
                StagesCount = 2,
                Stage = 2,
                Procents = 0
            });

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

        public void SetInstanceId(string id)
        {
            _localId = id;
        }
    }
}
