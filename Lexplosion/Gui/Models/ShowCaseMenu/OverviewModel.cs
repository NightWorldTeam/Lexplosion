using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class OverviewModel : VMBase
    {
        private InstanceData _instanceData;
        private bool _isLocalInstance = false;

        #region props
        public InstanceData InstanceData
        {
            get => _instanceData; set
            {
                _instanceData = value;
                OnPropertyChanged(nameof(InstanceData));
            }
        }

        public bool IsLocalInstance 
        {
            get => _isLocalInstance; set
            {
                _isLocalInstance = value;
                OnPropertyChanged(nameof(IsLocalInstance));
            }
        }
        #endregion

        public GalleryViewModel GalleryVM { get; }

        public OverviewModel(InstanceClient instanceClient)
        {
            InstanceData = instanceClient.GetFullInfo();
            GalleryVM = new GalleryViewModel(InstanceData.Images);
        }
    }
}

