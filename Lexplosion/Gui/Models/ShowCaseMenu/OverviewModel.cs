using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
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

        public OverviewModel(string outsideId, string localId, InstanceSource source)
        {
            switch (source) 
            {
                case InstanceSource.Nightworld:
                    InstanceData = ManageLogic.GetInstanceData(source, outsideId);
                    break;
                case InstanceSource.Curseforge:
                    InstanceData = ManageLogic.GetInstanceData(source, outsideId);
                    break;
                case InstanceSource.Local:
                    IsLocalInstance = true;
                    InstanceData = ManageLogic.GetInstanceData(source, localId);
                    break;
            }
            GalleryVM = new GalleryViewModel(InstanceData.Images);
        }
    }
}

