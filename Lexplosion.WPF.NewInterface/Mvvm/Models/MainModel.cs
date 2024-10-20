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
        private HashSet<object> ExportingInstances { get; } = new HashSet<object>();

        public IInstanceController CatalogController { get; }
        public IInstanceController LibraryController { get; }
        public InstanceSharesController InstanceSharesController { get; }


        public INotificationService NotificationService { get; } = new NotificationService();

        public MainModel()
        {
            CatalogController = new CatalogController(Export, NotificationService.Notify);
            LibraryController = new LibraryController(Export, NotificationService.Notify);
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
            var exportVM = new InstanceExportViewModel(instanceClient);
            var instanceShare = new InstanceShareViewModel(instanceClient, InstanceSharesController, NotificationService.Notify);
            var activeShares = new ActiveSharesViewModel(InstanceSharesController, NotificationService.Notify);

            var leftmenu = new LeftMenuControl(new ModalLeftMenuTabItem[]
            {
                new ModalLeftMenuTabItem(0, "Export", "Download", exportVM, true, true),
                new ModalLeftMenuTabItem(1, "Share", "Share", instanceShare, true, false),
                new ModalLeftMenuTabItem(2, "ActiveShares", "ActiveShares", activeShares, true, false)
            });

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

            ModalNavigationStore.Instance.Open(leftmenu);
        }
    }
}
