using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.Logic.Management.Import.Importers
{
    class ImportExecutor
    {
        private string _filePath;
        private Settings _settings;
        private readonly IPlatfromServiceContainer _services;
        private readonly WithDirectory _withDirectory;
        private IImportManager _importManager;
        private ProgressHandler _progressHandler;
        private readonly DynamicStateHandler<ImportInterruption, InterruptionType> _interruptionHandler;
        private readonly Guid _importId;
        private CancellationToken _cancellationToken;
        private bool _fileAddrIsLocalPath;

        public ImportExecutor(string fileAddr, bool fileAddrIsLocalPath, Settings settings, IPlatfromServiceContainer services, ProgressHandler progressHandler, ImportData importData)
        {
            _filePath = fileAddr;
            _settings = settings;
            _services = services;
            _progressHandler = progressHandler;
            _interruptionHandler = importData.InterruptionHandler;
            _cancellationToken = importData.CancelToken;
            _fileAddrIsLocalPath = fileAddrIsLocalPath;
            _importId = importData.ImportId;
            _withDirectory = services.DirectoryService;
        }

        private void DefineManagerType()
        {
            try
            {
                if (Path.GetExtension(_filePath) == ".mrpack")
                {
                    Runtime.DebugWrite(".mrpack pack");
                    _importManager = new ModrinthImportManager(_filePath, _settings, _services, _cancellationToken);
                    return;
                }

                if (Path.GetExtension(_filePath) == ".nwpk")
                {
                    Runtime.DebugWrite(".nwpk pack");
                    _importManager = new NWPackImportManager(_filePath, _settings, _services, _cancellationToken);
                    return;
                }
            }
            catch { }

            try
            {
                Runtime.DebugWrite("trying open as an archive");

                using (ZipArchive archive = ZipFile.OpenRead(_filePath))
                {
                    if (archive.GetEntry("modrinth.index.json") != null)
                    {
                        Runtime.DebugWrite("is modrinth");
                        _importManager = new ModrinthImportManager(_filePath, _settings, _services, _cancellationToken);
                        return;
                    }

                    if (archive.GetEntry("instanceInfo.json") != null)
                    {
                        Runtime.DebugWrite("is nightworld");
                        _importManager = new NWPackImportManager(_filePath, _settings, _services, _cancellationToken);
                        return;
                    }

                    if (archive.GetEntry("manifest.json") != null)
                    {
                        Runtime.DebugWrite("is curseforge");
                        _importManager = new CurseforgeImportManager(_filePath, _settings, _services, _cancellationToken);
                        return;
                    }

                    _importManager = new SimpleArchiveImportManager(_filePath, _settings, _services, _importId, _cancellationToken, _interruptionHandler);
                }
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("file open error " + ex);
            }
        }

        public InstanceInit Prepeare(out PrepeareResult result)
        {
            ProgressHandler progressHandler = _progressHandler;

            //Если мы имеем ссылку на файл, а не локальный путь, то скачиваем этот файл
            if (!_fileAddrIsLocalPath)
            {
                string fileName = "axaxa_ebala";
                string tempDir = _withDirectory.CreateTempDir();

                var taskArgs = new TaskArgs()
                {
                    CancelToken = _cancellationToken,
                    PercentHandler = (int pr) =>
                    {
                        _progressHandler(StateType.DownloadClient, new ProgressHandlerArguments()
                        {
                            StagesCount = 3,
                            Stage = 1,
                            FilesCount = 0,
                            Procents = pr,
                            TotalFilesCount = 1,
                        });
                    }
                };

                if (!_withDirectory.DownloadFile(_filePath, fileName, tempDir, taskArgs))
                {
                    result = new PrepeareResult();
                    return InstanceInit.DownloadFilesError;
                }

                _filePath = tempDir + fileName;

                progressHandler = (StateType stageType, ProgressHandlerArguments data) =>
                {
                    // в калбеке обработки прогресса прибавляем в количества стадий и в номер стадии по еденице, потому что одна стадия у нас уже была (скачивание файла по url)
                    _progressHandler(stageType, new ProgressHandlerArguments()
                    {
                        FilesCount = data.FilesCount,
                        TotalFilesCount = data.TotalFilesCount,
                        Stage = data.Stage + 1,
                        StagesCount = data.StagesCount + 1,
                    });
                };
            }

            DefineManagerType();
            if (_importManager == null)
            {
                result = new PrepeareResult();
                return InstanceInit.UnknownClientFileType;
            }

            return _importManager.Prepeare(progressHandler, out result);
        }

        public InstanceInit Import(string instanceId, out IReadOnlyCollection<string> errors)
        {
            _importManager.SetInstanceId(instanceId);
            return _importManager.Import(_progressHandler, out errors);
        }

    }
}
