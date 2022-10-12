using Lexplosion.Gui.ViewModels;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;

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

        public OverviewModel(InstanceClient instanceClient, ISubmenu submenuViewModel, OverviewViewModel overviewViewModel)
        {
            InstanceData = instanceClient.GetFullInfo();
            overviewViewModel.IsLoadedFailed = InstanceData == null ? true : false;
            IsLocalInstance = (InstanceData != null && InstanceData.TotalDownloads != 0);
            GalleryVM = new GalleryViewModel(InstanceData?.Images ?? new List<byte[]>(), submenuViewModel);
        }
    }
}

