using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.ModalFactory
{
    public sealed class ModalInstanceCreatorFactory : ModalFactoryBase
    {
        private readonly LibraryController _libraryController;
        private readonly InstanceSharesController _shareController;


        public ModalInstanceCreatorFactory(LibraryController controller, InstanceSharesController sharesController)
        {
            _libraryController = controller;
            _shareController = sharesController;
        }


        public override IModalViewModel Create()
        {
            var leftMenuControl = new LeftMenuControl();

            var menuItems = new ModalLeftMenuTabItem[3] {
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "AddCircle",
                        TitleKey = "Create",
                        IsEnable = true,
                        IsSelected = true,
                        Content = new InstanceFactoryViewModel((i) => _libraryController.Add(i), leftMenuControl.CloseCommand)
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "PlaceItem",
                        TitleKey = "Import",
                        IsEnable = true,
                        IsSelected = false,
                        Content = new InstanceImportViewModel((i) => _libraryController.Add(i), _libraryController.Remove)
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "DownloadCloud",
                        TitleKey = "Distributions",
                        IsEnable = true,
                        IsSelected = false,
                        Content = new InstanceDistributionViewModel(_libraryController, _shareController)
                    }
            };

            leftMenuControl.AddTabItems(menuItems, true);
            return leftMenuControl;
        }
    }
}
