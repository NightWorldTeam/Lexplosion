using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Services;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models
{
    public sealed class InstanceExportController 
    {
        
    }

    public sealed class ImportController 
    {

    }

    public sealed class MainModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private HashSet<object> ExportingInstances { get; } = new HashSet<object>();

        public IInstanceController CatalogController { get; }
        public IInstanceController LibraryController { get; }
        public InstanceSharesController InstanceSharesController { get; }


        public INotificationService NotificationService { get; } = new NotificationService();

        public MainModel(AppCore appCore)
        {
            _appCore = appCore;
            CatalogController = new CatalogController(appCore, Export, NotificationService.Notify);
            LibraryController = new LibraryController(appCore, Export, NotificationService.Notify);
            InstanceSharesController = new InstanceSharesController();

            OnPropertyChanged(nameof(NotificationService));
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

            var exportVM = new InstanceExportViewModel(instanceClient);
            var instanceShare = new InstanceShareViewModel(instanceClient, InstanceSharesController, leftmenu.NavigateTo, NotificationService.Notify);
            var activeShares = new ActiveSharesViewModel(InstanceSharesController, NotificationService.Notify);

            leftmenu.AddTabItems(new ModalLeftMenuTabItem[]
            {
                new ModalLeftMenuTabItem(0, "Export", "Download", exportVM, true),
                new ModalLeftMenuTabItem(1, "Share", "Share", instanceShare, true),
                new ModalLeftMenuTabItem(2, "ActiveShares", "ActiveShares", activeShares, true)
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
    }
}
