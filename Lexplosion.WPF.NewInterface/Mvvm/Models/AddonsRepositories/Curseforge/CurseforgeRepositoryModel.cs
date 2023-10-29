using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public sealed class CurseforgeAddon : ObservableObject
    {
        private readonly InstanceAddon _instanceAddon;

        private ObservableCollection<FrameworkElementModel> _buttons = new ObservableCollection<FrameworkElementModel>();
        public IReadOnlyList<FrameworkElementModel> Buttons { get => _buttons; }


        #region Properties


        public byte[] Logo { get => _instanceAddon.Logo; }
        public string Name { get => _instanceAddon.Name; }
        public string Author { get => _instanceAddon.Author; }
        public string Description { get => _instanceAddon.Description; }
        public int DownloadCount { get => _instanceAddon.DownloadCount; }
        public string LatestUpdated { get => _instanceAddon.LastUpdated; }
        public string Version { get => _instanceAddon.Version; }



        #endregion Properties

        public CurseforgeAddon(InstanceAddon instanceAddon)
        {
            _instanceAddon = instanceAddon;
            _instanceAddon.LoadLoaded += () =>
            {
                OnPropertyChanged(nameof(Logo));
            };
            LoadButtons();
        }

        // Install, Remove, Update

        public void LoadButtons()
        {
            if (_instanceAddon.IsUrlExist)
            {
                _buttons.Add(new FrameworkElementModel("VisitCurseforge", () =>
                {
                    try { Process.Start(_instanceAddon.WebsiteUrl); }
                    catch
                    { // todo: прибраться и уведомления выводить
                    }
                }, "Curseforge", width: 24));
            }

            if (_instanceAddon.UpdateAvailable)
            {
                _buttons.Add(new FrameworkElementModel("Update", () => { _instanceAddon.Update(); }, "Update", height: 18));
            }

            if (_instanceAddon.IsInstalled)
            {
                _buttons.Add(new FrameworkElementModel("Delete", _instanceAddon.Delete, "Delete", height: 20));
            }
        }
    }

    public sealed class CurseforgeRepositoryModel : ViewModelBase
    {
        public static IReadOnlyCollection<string> SortByArgs { get; } = new string[]
        {
            "Relevancy",
            "Popularity",
            "LatestUpdate",
            "CreationDate",
            "TotalDownloads",
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

        private ObservableCollection<CurseforgeAddon> _addonsList { get; set; } = new ObservableCollection<CurseforgeAddon>();
        public IReadOnlyCollection<CurseforgeAddon> AddonList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        public CurseforgeRepositoryModel(BaseInstanceData baseInstanceData)
        {
            _instnaceData = baseInstanceData;
            Categories = new List<string>(
                "Addons|Adventure and RPG|API and Library|Armor, Tools, and Weapons|Cosmetic|Education|Food|Magic|Map and Information|MCreator|Miscellaneous|Redstone|Server Utility|Storage|Technology|Twitch Integration|Utility & QoL|World Gen".Split('|'));
            LoadAddonsCatalogPage("", 0, null);
        }


        #endregion Constructors
        
        
        private void LoadAddonsCatalogPage(string searchFilter, int pageIndex, IEnumerable<CategoryBase> categoryBases)
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var addonsList = InstanceAddon.GetAddonsCatalog(_instnaceData, 10, pageIndex, AddonType.Mods, CurseforgeApi.GetCategories(CfProjectType.Mods)[0], searchFilter); ;

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList = new ObservableCollection<CurseforgeAddon>(addonsList.Select(ia => new CurseforgeAddon(ia)));
                    OnPropertyChanged(nameof(AddonList));
                });
            });
        }
    }
}
