using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Tools;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.Logic.Management.Import.Importers
{
    class ImportExecutor
    {
        private string _filePath;
        private Settings _settings;
        private IImportManager _importManager;
        private ProgressHandlerCallback _progressHandler;
		private readonly DynamicStateHandler<ImportInterruption, InterruptionType> _interruptionHandler;
		private readonly Guid _importId;
		private CancellationToken _cancellationToken;
        private bool _fileAddrIsLocalPath;

        public ImportExecutor(string fileAddr, bool fileAddrIsLocalPath, Settings settings, ProgressHandlerCallback progressHandler, ImportData importData)
        {
            _filePath = fileAddr;
            _settings = settings;
            _progressHandler = progressHandler;
			_interruptionHandler = importData.InterruptionHandler;
			_cancellationToken = importData.CancelToken;
            _fileAddrIsLocalPath = fileAddrIsLocalPath;
			_importId = importData.ImportId;
		}

        private void DefineManagerType()
        {
            try
            {
                if (Path.GetExtension(_filePath) == ".mrpack")
                {
                    _importManager = new ModrinthImportManager(_filePath, _settings, _cancellationToken);
                    return;
                }

                if (Path.GetExtension(_filePath) == ".nwpk")
                {
                    _importManager = new NWPackImportManager(_filePath, _settings, _cancellationToken);
                    return;
                }

                if (Path.GetExtension(_filePath) == ".zip")
                {
                    using (ZipArchive archive = ZipFile.OpenRead(_filePath))
                    {
                        if (archive.GetEntry("modrinth.index.json") != null)
                        {
                            _importManager = new ModrinthImportManager(_filePath, _settings, _cancellationToken);
                            return;
                        }

                        if (archive.GetEntry("instanceInfo.json") != null)
                        {
                            _importManager = new NWPackImportManager(_filePath, _settings, _cancellationToken);
                            return;
                        }

                        if (archive.GetEntry("manifest.json") != null)
                        {
                            _importManager = new CurseforgeImportManager(_filePath, _settings, _cancellationToken);
                            return;
                        }

						_importManager = new SimpleArchiveImportManager(_filePath, _settings, _importId, _cancellationToken, _interruptionHandler);
					}
                }
            }
            catch { }
        }

        public ImportResult Prepeare(out PrepeareResult result)
        {
            //Если мы имеем ссылку на файл, а не локальный путь, то скачиваем этот файл
            if (!_fileAddrIsLocalPath)
            {
                string fileName = "axaxa_ebala";
                string tempDir = WithDirectory.CreateTempDir();

                var taskArgs = new TaskArgs()
                {
                    CancelToken = _cancellationToken,
                    PercentHandler = (int pr) =>
                    {
                        _progressHandler(StageType.Client, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 1,
                            FilesCount = 1,
                            Procents = pr
                        });
                    }
                };

                if (!WithDirectory.DownloadFile(_filePath, fileName, tempDir, taskArgs))
                {
                    result = new PrepeareResult();
                    return ImportResult.DownloadError;
                }

                _filePath = tempDir + fileName;
            }

            DefineManagerType();
            if (_importManager == null)
            {
                result = new PrepeareResult();
                return ImportResult.UnknownFileType;
            }

            return _importManager.Prepeare(_progressHandler, out result);
        }

        public ImportResult Import(string instanceId, out IReadOnlyCollection<string> errors)
        {
            _importManager.SetInstanceId(instanceId);
            return _importManager.Import(_progressHandler, out errors);
        }

    }
}
