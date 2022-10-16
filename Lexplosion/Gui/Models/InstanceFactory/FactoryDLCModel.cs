using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Logic.Management.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

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

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmptyList));
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
            IsEmptyList = addons.Count == 0;
        }

        public FactoryDLCModel(List<InstanceAddon> addons, CfProjectType type, string emptyListMessage) : this(addons, type)
        {
            EmptyListMessage = emptyListMessage;
            IsEmptyList = addons.Count == 0;
        }

        public void Uninstall(InstanceAddon addon)
        {
            InstalledAddons.Remove(addon);
            addon.Delete();
            IsEmptyList = InstalledAddons.Count == 0;
        }
    }
}
