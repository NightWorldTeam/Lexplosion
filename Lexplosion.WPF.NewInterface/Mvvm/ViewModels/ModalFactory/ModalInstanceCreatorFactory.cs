using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Limited;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory
{
    public sealed class ModalInstanceCreatorFactory : ModalFactoryBase
    {
        private readonly LibraryController _libraryController;
        private readonly InstanceSharesController _shareController;
        private readonly AppCore _appCore;
        private readonly ImportStartFunc _importStart;
        private readonly Func<IEnumerable<ImportProcess>> _getActiveImports;

        public ModalInstanceCreatorFactory(AppCore appCore, ImportStartFunc importStart, Func<IEnumerable<ImportProcess>> getActiveImports, LibraryController controller, InstanceSharesController sharesController)
        {
            _appCore = appCore;
            _importStart = importStart;
            _getActiveImports = getActiveImports;
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
                Content = !hasMinecraftVersions ? null : new InstanceFactoryViewModel((i) => _libraryController.Add(i), leftMenuControl.CloseCommand, 
                    _libraryController.InstancesGroups, _libraryController.SelectedGroup)
            });

            menuItems.Add(new ModalLeftMenuTabItem()
            {
                IconKey = "PlaceItem",
                TitleKey = "Import",
                IsEnable = true,
                IsSelected = !hasMinecraftVersions,
                Content = new InstanceImportViewModel(
                    _appCore, _importStart, _getActiveImports,
                    (instanceClient, importData) => _libraryController.Add(instanceClient, importData),
                    _libraryController.Remove)
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
