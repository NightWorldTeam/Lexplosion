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

            var hasMinecraftVersions = !(MainViewModel.AllGameVersions == null || MainViewModel.AllGameVersions.Length == 0);

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "AddCircle",
                TitleKey = "Create",
                IsEnable = hasMinecraftVersions,
                IsSelected = hasMinecraftVersions,
                Content = !hasMinecraftVersions ? null : new InstanceFactoryViewModel((i) => _libraryController.Add(i), leftMenuControl.CloseCommand)
            });

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "PlaceItem",
                TitleKey = "Import",
                IsEnable = true,
                IsSelected = !hasMinecraftVersions,
                Content = new InstanceImportViewModel(_appCore, (i) => _libraryController.Add(i), _libraryController.Remove)
            });

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "DownloadCloud",
                TitleKey = "Distributions",
                IsEnable = hasMinecraftVersions,
                IsSelected = false,
                Content = new NightWorldLimitedContentLayoutViewModel(new InstanceDistributionViewModel(_libraryController, _shareController), true)
            });

            if (hasMinecraftVersions)
            {
                leftMenuControl.AddTabItems(menuItems, hasMinecraftVersions);
            }
            else 
            {
                leftMenuControl.AddTabItems(menuItems, selectedPageType: typeof(InstanceImportViewModel));
            }
            return leftMenuControl;
        }
    }
}
