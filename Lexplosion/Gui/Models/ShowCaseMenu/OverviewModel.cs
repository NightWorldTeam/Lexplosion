using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class OverviewModel : VMBase
    {
        private InstanceData _instanceData;
        public InstanceData InstanceData 
        { 
            get => _instanceData; set 
            {
                _instanceData = value;
                OnPropertyChanged(nameof(InstanceData));
            }
        }

        public GalleryViewModel GalleryVM { get; }

        public OverviewModel(string outsideId, string localId, InstanceSource source)
        {
            if (outsideId == null || outsideId == "") 
            {
                InstanceData = ManageLogic.GetInstanceData(source, localId);
            }
            else InstanceData = ManageLogic.GetInstanceData(source, outsideId);
            GalleryVM = new GalleryViewModel(InstanceData.Images);
        }
    }
}

