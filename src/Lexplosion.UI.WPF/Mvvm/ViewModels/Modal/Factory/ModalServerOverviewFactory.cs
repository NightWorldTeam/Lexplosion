using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.Factory
{
    public sealed class ModalServerOverviewFactory : ModalAbstractFactory
    {
        private readonly Action<InstanceClient> _addToLibrary;

        public ModalServerOverviewFactory(Action<InstanceClient> addToLibrary)
        {
            _addToLibrary = addToLibrary;
        }

        public override IModalViewModel Create()
        {
            return new LeftMenuControl(
                new ModalLeftMenuTabItem[3]
                {
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "AddCircle",
                        TitleKey = "Create",
                        IsEnable = true,
                        IsSelected = true,
                        Content = new InstanceFactoryViewModel(_addToLibrary)
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "PlaceItem",
                        TitleKey = "Import",
                        IsEnable = true,
                        IsSelected = false
                    },
                    new ModalLeftMenuTabItem()
                    {
                        IconKey = "DownloadCloud",
                        TitleKey = "Distributions",
                        IsEnable = true,
                        IsSelected = false
                    }
                }
                );
        }
    }
}
