using Lexplosion.Logic.Objects;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class InstanceModel : VMBase
    {
        private InstanceProperties _properties;

        private string _overviewField;

        #region props

        public InstanceProperties Properties
        {
            get => _properties; set
            {
                _properties = value;
                OnPropertyChanged(nameof(Properties));
            }
        }

        public string OverviewField 
        {
            get => _overviewField; set 
            {
                _overviewField = value;
                OnPropertyChanged(nameof(OverviewField));
            }
        }

        public string OutsideId 
        {
            get => _properties.Id;
        }

        public string LocalId 
        {
            get => _properties.LocalId; set 
            {
                _properties.LocalId = value;
                OnPropertyChanged(nameof(LocalId));
            }
        }

        public InstanceSource Source 
        {
            get => _properties.Type;
        }

        public bool IsInstalled
        {
            get => _properties.IsInstalled; set
            {
                _properties.IsInstalled = value;
                OnPropertyChanged(nameof(IsInstalled));
            }
        }

        public bool IsInstanceAddedToLibrary
        {
            get => _properties.IsInstanceAddedToLibrary; set
            {
                _properties.IsInstanceAddedToLibrary = value;
                OnPropertyChanged(nameof(IsInstanceAddedToLibrary));
            }
        }

        public bool IsDownloadingInstance
        {
            get => _properties.IsDownloadingInstance; set
            {
                _properties.IsDownloadingInstance = value;
                OnPropertyChanged(nameof(IsDownloadingInstance));
            }
        }

        public bool IsNotDownloadingInstance 
        {
            get => !IsDownloadingInstance;
        }
        #endregion

        public InstanceModel(InstanceProperties properties)
        {
            _properties = properties;
            OverviewField = properties.InstanceAssets.description;
        }
    }
}