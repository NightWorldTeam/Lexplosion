using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.Logic.Management.Accounts;
using System.Collections.Generic;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Limited;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory
{
    public sealed class ModalInstanceCreatorFactory : ModalFactoryBase
    {
        private readonly LibraryController _libraryController;
        private readonly InstanceSharesController _shareController;
        private readonly AppCore _appCore;


        public ModalInstanceCreatorFactory(AppCore appCore, LibraryController controller, InstanceSharesController sharesController)
        {
            _appCore = appCore;
            _libraryController = controller;
            _shareController = sharesController;
        }


        public override IModalViewModel Create()
        {
            var leftMenuControl = new LeftMenuControl();

            var menuItems = new List<ModalLeftMenuTabItem>();

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "AddCircle",
                TitleKey = "Create",
                IsEnable = true,
                IsSelected = true,
                Content = new InstanceFactoryViewModel((i) => _libraryController.Add(i), leftMenuControl.CloseCommand)
            });

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "PlaceItem",
                TitleKey = "Import",
                IsEnable = true,
                IsSelected = false,
                Content = new InstanceImportViewModel(_appCore, (i) => _libraryController.Add(i), _libraryController.Remove)
            });

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "DownloadCloud",
                TitleKey = "Distributions",
                IsEnable = true,
                IsSelected = false,
                Content = new NightWorldLimitedContentLayoutViewModel(new InstanceDistributionViewModel(_libraryController, _shareController), true)
            });

            leftMenuControl.AddTabItems(menuItems, true);
            return leftMenuControl;
        }
    }
}
