using Lexplosion.Global;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    public class InstanceFactoryModel : VMBase
    {
        #region prop

        public List<string> UnavailableNames { get; } = new List<string>();
        public Dictionary<string, List<string>> ForgeVersions { get; }
        public Dictionary<string, List<string>> FabricVersions { get; }

        private bool _isAvaliableName;
        public bool IsAvaliableName
        {
            get => _isAvaliableName; set
            {
                _isAvaliableName = value;
                OnPropertyChanged(nameof(IsAvaliableName));
            }
        }

        private string _name;
        public string Name
        {
            get => _name; set
            {
                IsAvaliableName = !UnavailableNames.Contains(value);
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _selectedVersion;
        public string SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                OnPropertyChanged(nameof(SelectedVersion));
            }
        }

        private ModloaderType _modloaderType;
        public ModloaderType ModloaderType
        {
            get => _modloaderType; set
            {
                _modloaderType = value;
                OnPropertyChanged(nameof(ModloaderType));
            }
        }

        private string _logoPath = "";
        public string LogoPath
        {
            get => _logoPath; set
            {
                _logoPath = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}