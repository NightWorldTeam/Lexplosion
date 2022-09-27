using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    public class FactoryDLCModel : VMBase
    {
        public CfProjectType Type { get; }

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

        private string _emptyListMessage;
        public string EmptyListMessage 
        {
            get => _emptyListMessage; set 
            {
                _emptyListMessage = value;
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

        public FactoryDLCModel(List<InstanceAddon> addons, CfProjectType type)
        {
            InstalledAddons = new ObservableCollection<InstanceAddon>(addons);
            Type = type;
        }

        public FactoryDLCModel(List<InstanceAddon> addons, CfProjectType type, string emptyListMessage) : this(addons, type)
        {
            EmptyListMessage = emptyListMessage;
        }

        public void Uninstall(InstanceAddon addon) 
        {
            InstalledAddons.Remove(addon);
        }
    }
}
