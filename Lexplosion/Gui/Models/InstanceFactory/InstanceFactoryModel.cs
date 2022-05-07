using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceFactory
{
    public class InstanceFactoryModel : VMBase
    {
        private string _name;
        private string _selectedVersion;
        private ModloaderType _modloaderType;
        
        #region prop

        public  List<string> UnavailableNames { get; }
        public Dictionary<string, List<string>> ForgeVersions { get; }
        public Dictionary<string, List<string>> FabricVersions { get; }

        public string Name
        {
            get => _name; set
            {
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
            // Get UnavaliableNames
            //UnavailableNames = new List<string>();
            //foreach (var instance in UserData.Instances.Record.Keys)
            //    UnavailableNames.Add(UserData.Instances.Record[instance].Name);
        }
    }
}
