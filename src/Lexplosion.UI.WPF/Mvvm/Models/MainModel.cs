using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Core.Services;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Limited;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer;
using System;
using System.Collections.Generic;
using static Lexplosion.Logic.Management.Import.ImportInterruption;

namespace Lexplosion.UI.WPF.Mvvm.Models
{
    public delegate ImportProcess ImportStartFunc(string path, bool isUrl,
        Action<ImportInterruption, InterruptionType> onImportDynamicStateHandlerStateChanged,
        Action<ClientInitResult, ImportProcess, InstanceClient> importresultHandler,
        Action<ImportProcess> onCancelled);

    public sealed class MainModel : ObservableObject
    {
        private readonly AppCore _appCore;
        private readonly List<ImportProcess> _activeImports = [];
        private readonly Dictionary<InstanceClient, InstanceModelBase> _instanceModelByInstanceClient = [];
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;


        public Action _toAuthorization;


        #region Properties


        public InstanceModelBase RunningGame { get; private set; }
        private HashSet<object> ExportingInstances { get; } = new HashSet<object>();

        public IInstanceController CatalogController { get; }
        public ILibraryInstanceController LibraryController { get; }
        public InstanceSharesController InstanceSharesController { get; }


        #endregion Properties


        public MainModel(AppCore appCore, ClientsManager clientsManager)
        {
            _appCore = appCore;
            LibraryController = new LibraryController(appCore, clientsManager, Export, SetRunningGame, GetInstanceModelByInstanceClient, AddInstanceModelByInstanceClient);
            CatalogController = new CatalogController(appCore, Export, SetRunningGame, GetInstanceModelByInstanceClient, AddInstanceModelByInstanceClient);
            InstanceSharesController = new InstanceSharesController();

            OnPropertyChanged(nameof(NotificationService));
        }


        #region Public Methods


        public void SetToAuthorization(Action toAuthorization) 
        {
            _toAuthorization = toAuthorization;
        }

        public ImportProcess StartImport(
            string path, bool isUrl,
            Action<ImportInterruption, InterruptionType> onImportDynamicStateHandlerStateChanged,
            Action<ClientInitResult, ImportProcess, InstanceClient> importresultHandler,
            Action<ImportProcess> onCancelled)
        {

            if (string.IsNullOrEmpty(path))
            {
                _appCore.MessageService.Info("ImportResultWrongUrlOrPath", true);
                return null;
            }

            InstanceClient instanceClient = null;
            ImportProcess importProcess = null;

            // Запускаем импорт
            var dynamicStateHandler = new DynamicStateData<ImportInterruption, InterruptionType>();
            dynamicStateHandler.StateChanged += onImportDynamicStateHandlerStateChanged;

            var importData = new ImportData(dynamicStateHandler.GetHandler, (init) =>
            {
                ShowImportResultNotification(init, importProcess);

                _appCore.UIThread(() =>
                {
                    importProcess.IsImporing = false;
                    importProcess.IsSuccessful = true;

                    OnPropertyChanged(instanceClient, nameof(instanceClient.Name));

                    if (init.State != InstanceInit.Successful)
                    {
                        importProcess.IsSuccessful = false;
                        LibraryController.Remove(instanceClient);
                    }
                });

                importresultHandler?.Invoke(init, importProcess, instanceClient);
            });

            importProcess = new ImportProcess(importData.ImportId, path);
            _activeImports.Add(importProcess);

            importProcess.ImportCancelled += (process) =>
            {
                _activeImports.Remove(process);
                onCancelled?.Invoke(process);
                _appCore.MessageService.Info("ImportCancelledNotification", true);
            };

            if (isUrl)
            {
                var uri = new Uri(path);
                instanceClient = _clientsManager.Import(uri, importData);
            }
            else
            {
                instanceClient = _clientsManager.Import(path, importData);
            }

            importProcess.TargetInstanceClient = instanceClient;
            LibraryController.Add(instanceClient, importData);

            return importProcess;
        }

        public IEnumerable<ImportProcess> GetActiveImports()
        {
            return _activeImports;
        }

        public InstanceModelBase? GetInstanceModelByInstanceClient(InstanceClient instanceClient)
        {
            if (_instanceModelByInstanceClient.TryGetValue(instanceClient, out var instanceModel))
            {
                return instanceModel;
            }

            return null;
        }

        public void AddInstanceModelByInstanceClient(InstanceModelBase instanceModel)
        {
            _instanceModelByInstanceClient.Add(instanceModel.InstanceClient, instanceModel);
        }

        /// <summary>
        /// Запускает модальное окно с экспортом сборки.
        /// </summary>
        /// <param name="instanceClient"></param>
        public void Export(InstanceClient instanceClient)
        {
            // TODO: Засунить метод Export и HashSet ExportingInstances в отдельный контроллер.
            // Ибо в будущем всё равно делать Раздачу, которая работает по такому-же принципу.
            var leftmenu = new LeftMenuControl();

            var exportVM = new InstanceExportViewModel(_appCore, instanceClient);
            var instanceShare = new InstanceShareViewModel(_appCore, instanceClient, InstanceSharesController, leftmenu.NavigateTo);
            var activeShares = new ActiveSharesViewModel(InstanceSharesController);

            leftmenu.AddTabItems(new ModalLeftMenuTabItem[]
            {
                new ModalLeftMenuTabItem(0, "Export", "Download", exportVM, true),
                new ModalLeftMenuTabItem(1, "Share", "Share", new NightWorldLimitedContentLayoutViewModel(instanceShare, _toAuthorization, true), true),
                new ModalLeftMenuTabItem(2, "ActiveShares", "ActiveShares", new NightWorldLimitedContentLayoutViewModel(activeShares, _toAuthorization, true), true)
            }, true);

            leftmenu.LoaderPlaceholderKey = "ExportProcessActive";

            // Если сборка экспортируется.
            if (ExportingInstances.Contains(instanceClient))
            {
                leftmenu.PageLoadingStatusChange(true);
            }

            // Состояние экспорта изменилось
            exportVM.Model.ExportStatusChanged += (isExporting) =>
            {
                // Если сборка раздаётся, то добавляем её в список
                // Иначе удалем из него.
                if (isExporting)
                {
                    ExportingInstances.Add(instanceClient);
                }
                else
                {
                    ExportingInstances.Remove(instanceClient);
                }

                leftmenu.PageLoadingStatusChange(isExporting);
            };

            instanceShare.Model.SharePreparingStarted += (isPreparing) =>
            {
                leftmenu.PageLoadingStatusChange(isPreparing);
            };

            _appCore.ModalNavigationStore.Open(leftmenu);
        }

        public void SetRunningGame(InstanceModelBase instanceModelBase)
        {
            RunningGame = instanceModelBase;
            OnPropertyChanged(nameof(RunningGame));
        }


        #endregion Public Methods


        #region Private Methods


        private void ShowImportResultNotification(ClientInitResult initResult, ImportProcess importFile)
        {
            switch (initResult.State)
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
