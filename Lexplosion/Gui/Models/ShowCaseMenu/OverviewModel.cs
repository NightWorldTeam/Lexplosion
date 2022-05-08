using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public OverviewModel(string id, InstanceSource source)
        {
            InstanceData = ManageLogic.GetInstanceData(source, id);
            GalleryVM = new GalleryViewModel(InstanceData.Images);
        }
    }
}

