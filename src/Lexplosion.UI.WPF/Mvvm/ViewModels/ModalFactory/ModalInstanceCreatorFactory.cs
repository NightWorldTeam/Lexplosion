using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Mvvm.Models;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Limited;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal.InstanceTransfer;
using System;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.ModalFactory
{
    public sealed class ModalInstanceCreatorFactory : ModalFactoryBase
    {
        private readonly LibraryController _libraryController;
        private readonly InstanceSharesController _shareController;
        private readonly Action _toAuthorization;
        private readonly AppCore _appCore;
        private readonly ImportStartFunc _importStart;
        private readonly Func<IEnumerable<ImportProcess>> _getActiveImports;

        public ModalInstanceCreatorFactory(AppCore appCore, ImportStartFunc importStart, Func<IEnumerable<ImportProcess>> getActiveImports, LibraryController controller, InstanceSharesController sharesController, Action toAuthorization)
        {
            _appCore = appCore;
            _importStart = importStart;
            _getActiveImports = getActiveImports;
            _libraryController = controller;
            _shareController = sharesController;
            _toAuthorization = toAuthorization;
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
                Content = new NightWorldLimitedContentLayoutViewModel(new InstanceDistributionViewModel(_libraryController, _shareController), _toAuthorization, true)
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
