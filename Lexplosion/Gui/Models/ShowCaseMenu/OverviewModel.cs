using Lexplosion.Gui.ViewModels;
using Lexplosion.Gui.ViewModels.ShowCaseMenu;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.ShowCaseMenu
{
    public class OverviewModel : VMBase
    {
        #region Properties

        private InstanceData _instanceData;
        public InstanceData InstanceData
        {
            get => _instanceData; set
            {
                _instanceData = value;
                OnPropertyChanged(nameof(InstanceData));
            }
        }

        private bool _isLocalInstance = false;
        public bool IsLocalInstance
        {
            get => _isLocalInstance; set
            {
                _isLocalInstance = value;
                OnPropertyChanged(nameof(IsLocalInstance));
            }
        }

        public GalleryViewModel GalleryVM { get; }


        #endregion Properties


        #region Constructors


        public OverviewModel(InstanceClient instanceClient, ISubmenu submenuViewModel, OverviewViewModel overviewViewModel)
        {
            InstanceData = instanceClient.GetFullInfo();
            overviewViewModel.IsLoadedFailed = InstanceData == null ? true : false;
            IsLocalInstance = (InstanceData != null && InstanceData.TotalDownloads != 0);
            GalleryVM = new GalleryViewModel(InstanceData?.Images ?? new List<byte[]>(), submenuViewModel);
        }


        #endregion Constructors
    }
}

