using System.Collections.Generic;
using System.IO;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Import.Importers
{
	class NWPackImportManager : IImportManager
	{
		private string _fileAddres;
		private Settings _settings;
		private CancellationToken _cancellationToken;

		private VersionManifest _versionManifest;
		private string _localId;
		private string _unzipPath;

		public int CompletedStagesCount { private get; set; }

		public NWPackImportManager(string fileAddres, Settings settings, CancellationToken cancelToken)
		{
			_fileAddres = fileAddres;
			_settings = settings;
			_cancellationToken = cancelToken;
		}

		public ImportResult Prepeare(ProgressHandlerCallback progressHandler, out PrepeareResult result)
		{
			result = new PrepeareResult();

			progressHandler(StageType.Client, new ProgressHandlerArguments()
			{
				StagesCount = CompletedStagesCount + 2,
				Stage = CompletedStagesCount + 1,
				Procents = 0
			});

			ImportResult res = WithDirectory.UnzipInstance(_fileAddres, out ArchivedClientData parameters, out _unzipPath);
			if (res != ImportResult.Successful) return res;

			if (parameters?.GameVersionInfo?.IsNan != false)
			{
				Runtime.DebugWrite("GameVersionError");
				return ImportResult.GameVersionError;
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

			return ImportResult.Successful;
		}

		public ImportResult Import(ProgressHandlerCallback progressHandler, out IReadOnlyCollection<string> errors)
		{
			errors = new List<string>();
			ImportResult result = WithDirectory.MoveUnpackedInstance(_localId, _unzipPath);
			if (result != ImportResult.Successful)
			{
				try
				{
					string dir = WithDirectory.GetInstancePath(_localId);
					if (Directory.Exists(dir)) Directory.Delete(dir, true);
				}
				catch { }

				return result;
			}

			DataFilesManager.SaveManifest(_localId, _versionManifest);

			return ImportResult.Successful;
		}

		public void SetInstanceId(string id)
		{
			_localId = id;
		}
	}
}
