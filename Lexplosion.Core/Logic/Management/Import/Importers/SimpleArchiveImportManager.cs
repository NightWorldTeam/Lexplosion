using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Threading;
using Lexplosion.Tools;
using Lexplosion.Logic.Management.Instances;
using System.Security.Cryptography;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.CommonClientData;
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

		private VersionManifest _versionManifest;
		private string _localId;
		private string _unzipPath;

		public SimpleArchiveImportManager(string fileAddres, Settings settings, Guid importId, CancellationToken cancelToken, DynamicStateHandler<ImportInterruption, InterruptionType> interruptionHandler)
		{
			_fileAddres = fileAddres;
			_settings = settings;
			_importId = importId;
			_cancelToken = cancelToken;
			_interruptionHandler = interruptionHandler;
		}

		public int CompletedStagesCount { private get; set; }

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

		public ImportResult Prepeare(ProgressHandlerCallback progressHandler, out PrepeareResult result)
		{
			result = new PrepeareResult();

			progressHandler(StageType.Client, new ProgressHandlerArguments()
			{
				StagesCount = CompletedStagesCount + 2,
				Stage = CompletedStagesCount + 1,
				Procents = 0
			});

			_unzipPath = WithDirectory.CreateTempDir() + "import/";
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
				return ImportResult.DirectoryCreateError;
			}

			try
			{
				ZipFile.ExtractToDirectory(_fileAddres, activeUnzipPath);

				// во избежание приколов удаляеем файл manifest.json, если он есть
				string manifestFile = activeUnzipPath + "/manifest.json";
				if (File.Exists(manifestFile)) File.Delete(manifestFile);
			}
			catch (Exception ex)
			{
				Runtime.DebugWrite("Exception " + ex);
				return ImportResult.ZipFileError;
			}

			//Вызываем эвент, говорящий что нам нужны данные для сборки
			var interruption = new ImportInterruption(_importId);
			_interruptionHandler.ChangeState(interruption, InterruptionType.BasicDataRequired);

			BaseInstanceData data = interruption.BaseData;
			if (_cancelToken.IsCancellationRequested) return ImportResult.Canceled;

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

			return ImportResult.Successful;
		}

		public void SetInstanceId(string id)
		{
			_localId = id;
		}
	}
}
