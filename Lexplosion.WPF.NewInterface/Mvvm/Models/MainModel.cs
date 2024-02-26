using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using Lexplosion.WPF.NewInterface.Stores;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models
{
    public sealed class InstanceExportController 
    {
        
    }

    public sealed class MainModel : ViewModelBase
    {
        private HashSet<object> ExportingInstances { get; } = new HashSet<object>();


        public IInstanceController CatalogController { get; }
        public IInstanceController LibraryController { get; }

        
        public MainModel()
        {
            CatalogController = new CatalogController(Export);
            LibraryController = new LibraryController(Export);
        }

        /// <summary>
        /// Запускает модальное окно с экспортом сборки.
        /// </summary>
        /// <param name="_instanceClient"></param>
        public void Export(InstanceClient _instanceClient)
        {
            // TODO: Засунить метод Export и HashSet ExportingInstances в отдельный контроллер.
            // Ибо в будущем всё равно делать Раздачу, которая работает по такому-же принципу.
            var exportVM = new InstanceExportViewModel(_instanceClient);

            var leftmenu = new LeftMenuControl(new ModalLeftMenuTabItem[]
            {
                new ModalLeftMenuTabItem(0, "Export", "Download", exportVM, true, true)
            });

            leftmenu.LoaderPlaceholderKey = "ExportProcessActive";

            // Если сборка экспортируется.
            if (ExportingInstances.Contains(_instanceClient))
            {
                leftmenu.IsProcessActive = true;
            }

            // Состояние экспорта изменилось
            exportVM.Model.ExportStatusChanged += (isExporting) =>
            {
                // Если сборка раздаётся, то добавляем её в список
                // Иначе удалем из него.
                if (isExporting)
                {
                    ExportingInstances.Add(_instanceClient);
                }
                else
                {
                    ExportingInstances.Remove(_instanceClient);
                }

                leftmenu.IsProcessActive = isExporting;
            };

            ModalNavigationStore.Instance.Open(leftmenu);
        }
    }
}
