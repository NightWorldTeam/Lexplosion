using Lexplosion.Global;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    public class InstanceFactoryModel : VMBase
    {
        private const string _defaultInstanceName = "Custom Instance";
        private string _name;
        private string _selectedVersion;
        private ModloaderType _modloaderType;

        private bool _isAvaliableName;
        #region prop

        public List<string> UnavailableNames { get; } = new List<string>();
        public Dictionary<string, List<string>> ForgeVersions { get; }
        public Dictionary<string, List<string>> FabricVersions { get; }

        public bool IsAvaliableName 
        {
            get => _isAvaliableName; set
            {
                _isAvaliableName = value;
                OnPropertyChanged(nameof(IsAvaliableName));
            }
        }

        public string Name
        {
            get => _name; set
            {
                IsAvaliableName = !UnavailableNames.Contains(value);
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                OnPropertyChanged(nameof(SelectedVersion));
            }
        }

        public ModloaderType ModloaderType
        {
            get => _modloaderType; set
            {

                _modloaderType = value;
                OnPropertyChanged(nameof(ModloaderType));
            }
        }
        #endregion

        public InstanceFactoryModel()
        {
            Name = _defaultInstanceName;
        }
    }
}
