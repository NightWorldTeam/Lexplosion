using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.WPF.NewInterface.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Models.AddonsRepositories
{
    public sealed class CurseforgeRepositoryModel : ViewModelBase
    {
        public static IReadOnlyCollection<string> SortByArgs { get; } = new string[] 
        {
            "Relevancy",
            "Popularity",
            "Latest update",
            "Creation Date",
            "Total Downloads",
            "A-Z",
        };

        public static IReadOnlyCollection<int> ShowPerPageList { get; } = new int[3]
        {
            10,
            20,
            50
        };


        private readonly BaseInstanceData _instnaceData;


        #region Properties


        public List<string> SelectedCategories { get; } = new List<string>(1) { "Adventure and RPG" };
        public List<string> Categories { get; }

        private ObservableCollection<InstanceAddon> _addonsList { get; set; } = new ObservableCollection<InstanceAddon>();
        public IReadOnlyCollection<InstanceAddon> AddonList { get => _addonsList; }  


        #endregion Properties


        #region Constructors


        public CurseforgeRepositoryModel(BaseInstanceData baseInstanceData)
        {
            _instnaceData = baseInstanceData;
            Categories = new List<string>(
                "Addons|Adventure and RPG|API and Library|Armor, Tools, and Weapons|Cosmetic|Education|Food|Magic|Map and Information|MCreator|Miscellaneous|Redstone|Server Utility|Storage|Technology|Twitch Integration|Utility & QoL|World Gen".Split('|'));
            LoadAddonsCatalogPage("", 0, null);
        }


        private void LoadAddonsCatalogPage(string searchFilter, int pageIndex, IEnumerable<CategoryBase> categoryBases) 
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var addonsList = InstanceAddon.GetAddonsCatalog(_instnaceData, 10, pageIndex, AddonType.Mods, CurseforgeApi.GetCategories(CfProjectType.Mods)[0], searchFilter);;

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList = new ObservableCollection<InstanceAddon>(addonsList);
                    OnPropertyChanged(nameof(AddonList));
                });
            });
        }


        #endregion Constructors
    }
}
