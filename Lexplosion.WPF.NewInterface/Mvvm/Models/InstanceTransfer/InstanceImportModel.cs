using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer
{
    public sealed class InstanceImportModel : ViewModelBase
    {
        private readonly Action<InstanceClient, ImportData?> _addToLibrary;
        private readonly Action<InstanceClient> _removeFromLibrary;
        private readonly AppCore _appCore;
        private IModalViewModel _currentModalViewModelBase;
		private readonly ClientsManager _clientsManager = Runtime.ClientsManager;


		public ObservableCollection<ImportProcess> ImportProcesses { get; } = new();
        public Action<IEnumerable<string>> ImportAction { get; }

        public Queue<InstanceImportFillDataViewModel> FillDataViewModels { get; } = [];

        public IEnumerable<string> AvailableFileExtensions { get; } = ["zip", "nwpk", "mrpack"];

        public string ImportURL { get; set; }


        #region Constructors


        public InstanceImportModel(AppCore appCore, Action<InstanceClient, ImportData?> addToLibrary, Action<InstanceClient> removeFromLibrary)
        {
            _appCore = appCore;
            _addToLibrary = addToLibrary;
            _removeFromLibrary = removeFromLibrary;
            _appCore.ModalNavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            ImportAction = (filePaths) =>
            {
                foreach (var path in filePaths)
                    Import(path);
            };
        }

        private void OnCurrentViewModelChanged()
        {
            _currentModalViewModelBase = _currentModalViewModelBase ?? _appCore.ModalNavigationStore.CurrentViewModel;
        }


        #endregion Constructors


        #region Public Methods 


        /// <summary>
        /// Открывает модальное окно, для выбора файлов с PC.
        /// </summary>
        public void BrowseFiles()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = Constants.ImportFileDialogFilters;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.FileName.EndsWith(Constants.ImportFileExtensionZip) || dialog.FileName.EndsWith(Constants.ImportFileExtensionNWPack) || dialog.FileName.EndsWith(Constants.ImportFileExtensionMRPack))
                    {
                        Import(dialog.FileName);
                    }
                }
            }
        }


        public void Import(string path)
        {
            InstanceClient instanceClient = null;

            // Запускаем импорт
            var dynamicStateHandler = new DynamicStateData<ImportInterruption, InterruptionType>();
            dynamicStateHandler.StateChanged += OnImportDynamicStateHandlerStateChanged;

            ImportProcess importProcess = null;

			var importData = new ImportData(dynamicStateHandler.GetHandler, (init) => ImportResultHandler(init, ref importProcess, instanceClient));

            var importFile = new ImportProcess(importData.ImportId, path, importData.CancelImport);
            importFile.ImportCancelled += OnImportCancelled;
            ImportProcesses.Add(importFile);

            instanceClient = _clientsManager.Import(path, importData);
            importFile.TargetInstanceClient = instanceClient;

            // Добавляем в библиотеку.
            _addToLibrary(instanceClient, importData);
        }

        /// <summary>
        /// Импорт отменен
        /// </summary>
        private void OnImportCancelled(Guid id)
        {
            var importProcess = ImportProcesses.FirstOrDefault(i => i.Id == id);
            CancelImport(importProcess);
            _appCore.MessageService.Info("ImportCancelledNotification", true);
        }

        private void OnImportDynamicStateHandlerStateChanged(ImportInterruption importInterruption, InterruptionType arg2)
        {
            // Если в очереди пусто, окрываем модальное окно.
            // Если в очереди есть элементы, добавляем новый в очередь.
            // Если элемены в очереди закончились, и на момент открытия
            // было открыто модальное окно импорта возвращаем пользователя туда
            // Иначе, закрываем все модальные окна.

            var vm = new InstanceImportFillDataViewModel(_appCore, (baseInstanceData) =>
            {
                importInterruption.BaseData = baseInstanceData;

                if (FillDataViewModels.Count > 0)
                {
                    var vm = FillDataViewModels.Dequeue();
                    _appCore.ModalNavigationStore.Open(vm);
                    return;
                }
                _appCore.ModalNavigationStore.Open(_currentModalViewModelBase);
            }, ImportProcesses.FirstOrDefault(i => i.Id == importInterruption.ImportId).Cancel);

            if (FillDataViewModels.Count == 0)
            {
                _appCore.ModalNavigationStore.Open(vm);
            }
            else
            {
                FillDataViewModels.Enqueue(vm);
            }
        }

        public void CancelImport(ImportProcess importProcess)
        {
            if (!importProcess.IsImporing)
                return;

            importProcess.Cancel();

            var index = ImportProcesses.IndexOf(importProcess);

            if (index == -1)
                return;

            ImportProcesses.RemoveAt(index);
            _removeFromLibrary?.Invoke(importProcess.TargetInstanceClient);
        }


        public void ImportByUrl() 
        {
            if (string.IsNullOrEmpty(ImportURL))
            {
                _appCore.MessageService.Info("ImportCancelledNotification", true);
                return;
            }

            
            InstanceClient instanceClient = null;

            // Запускаем импорт
            var dynamicStateHandler = new DynamicStateData<ImportInterruption, InterruptionType>();
            dynamicStateHandler.StateChanged += OnImportDynamicStateHandlerStateChanged;

            var importProcessLazy = new Lazy<ImportData>();

            ImportProcess importProcess = null;
            var importData = new ImportData(dynamicStateHandler.GetHandler, (init) => ImportResultHandler(init, ref importProcess, instanceClient));

            var uri = new Uri(ImportURL);
            importProcess = new ImportProcess(importData.ImportId, uri, importData.CancelImport);
            importProcess.ImportCancelled += OnImportCancelled;
            ImportProcesses.Add(importProcess);
            
            instanceClient = _clientsManager.Import(uri, importData);

            importProcess.TargetInstanceClient = instanceClient;

            // Добавляем в библиотеку.
            _addToLibrary(instanceClient, importData);
        }


        #endregion Public Methods


        #region Private Methods


        private void ImportResultHandler(ClientInitResult clientInitResult, ref ImportProcess importFileRef, InstanceClient instanceClient)
        {
            var initResult = clientInitResult.State;
            var importFile = importFileRef;
            _appCore.UIThread(() =>
            {
                importFile.IsImporing = false;
                importFile.IsSuccessful = true;

                // TODO: Send Notification
                OnPropertyChanged(nameof(instanceClient.Name));

                if (initResult != InstanceInit.Successful)
                {
                    importFile.IsSuccessful = false;
                    _removeFromLibrary(instanceClient);
                }
            });

            switch (initResult)
            {
                case InstanceInit.Successful:
                    _appCore.MessageService.Success("ImportResultSuccessful", true, importFile.Name);
                    break;
                case InstanceInit.ZipFileOpenError:
                    _appCore.MessageService.Error("ImportResultZipFileOpenError", true);
                    break;
                case InstanceInit.GameVersionError:
                    _appCore.MessageService.Error("ImportResultGameVersionError", true);
                    break;
                case InstanceInit.ManifestError:
                    _appCore.MessageService.Error("ImportResultManifestError", true);
                    break;
                case InstanceInit.JavaDownloadError:
                    _appCore.MessageService.Error("ImportResultJavaDownloadError", true);
                    break;
                case InstanceInit.IsOfflineMode:
                    _appCore.MessageService.Error("ImportResultIsOfflineMode", true);
                    break;
                case InstanceInit.MoveFilesError:
                    _appCore.MessageService.Error("ImportResultMoveFilesError", true);
                    break;
                case InstanceInit.DownloadFilesError:
                    _appCore.MessageService.Error("ImportResultDownloadFilesError", true);
                    break;
                case InstanceInit.DirectoryCreateError:
                    _appCore.MessageService.Error("ImportResultDirectoryCreateError", true);
                    break;
                case InstanceInit.WrongClientFileUrl:
                    _appCore.MessageService.Error("ImportResultWrongUrl", true);
                    break;
                case InstanceInit.UnknownClientFileType:
                    _appCore.MessageService.Error("ImportResultUnknownFileType", true);
                    break;
                case InstanceInit.IsCancelled:
                    _appCore.MessageService.Error("ImportCancelledNotification", true);
                    break;
                default:
                    _appCore.MessageService.Error("ImportResultUnknownError", true);
                    break;
            }
        }


        #endregion Private Methods
    }
}
