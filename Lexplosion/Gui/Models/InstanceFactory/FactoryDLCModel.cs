using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    public class FactoryDLCModel : VMBase
    {
        public readonly AddonType Type;

        private ObservableCollection<InstanceAddon> _instanceAddons;
        public ObservableCollection<InstanceAddon> InstalledAddons 
        {
            get => _instanceAddons; set
            {
                _instanceAddons = value;

                if (_instanceAddons.Count == 0)
                    IsEmptyList = true;
                else IsEmptyList = false;

                OnPropertyChanged();
            }
        }

        private bool _isEmptyList;
        public bool IsEmptyList
        {
            get => _isEmptyList; set
            {
                _isEmptyList = value;
                OnPropertyChanged();
            }
        }

        public FactoryDLCModel(List<InstanceAddon> addons, AddonType type)
        {
            InstalledAddons = new ObservableCollection<InstanceAddon>(addons);
            Type = type;
        }

        public void Uninstall(InstanceAddon addon) => InstalledAddons.Remove(addon);
    }
}
