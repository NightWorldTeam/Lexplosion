using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    public class InstanceFactoryModel : VMBase
    {
        #region Properities


        public List<string> UnavailableNames { get; } = new List<string>();
        public Dictionary<string, List<string>> ForgeVersions { get; }
        public Dictionary<string, List<string>> FabricVersions { get; }

        public Dictionary<string, List<string>> OptifineVersios { get; }

        private bool _isAvaliableName;
        public bool IsAvaliableName
        {
            get => _isAvaliableName; set
            {
                _isAvaliableName = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name; set
            {
                IsAvaliableName = !UnavailableNames.Contains(value);
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _selectedVersion;
        public string SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                OnPropertyChanged();
            }
        }

        private ModloaderType _modloaderType;
        public ModloaderType ModloaderType
        {
            get => _modloaderType; set
            {
                _modloaderType = value;
                OnPropertyChanged();
            }
        }

        private string _logoPath = default;
        public string LogoPath
        {
            get => _logoPath; set
            {
                _logoPath = value;
                OnPropertyChanged();
            }
        }

        #endregion Properities
    }
}