using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.Paginator;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using static Lexplosion.Logic.Network.Web.ModrinthApi;

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
        public static ReadOnlyCollection<string> SortByArgs { get; } = new ReadOnlyCollection<string>(new string[] 
        {
            "Relevancy",
            "Popularity",
            "LatestUpdate",
            "CreationDate",
            "TotalDownloads",
            "A-Z",
        });
        public static ReadOnlyCollection<uint> ShowPerPageList { get; } = new ReadOnlyCollection<uint>(new uint[3]
        {
            10,
            20,
            50
        });


        private readonly BaseInstanceData _instanceData;





        #region Properties


        public List<string> SelectedCategories { get; } = new List<string>(1) { "Adventure and RPG" };
        public List<string> Categories { get; }



        private string _searchFilter = string.Empty;
        public string SearchFilter 
        {
            get => _searchFilter; set 
            {
                _searchFilter = value;
                OnPropertyChanged();
            } 
        }

        private uint _currentPageIndex = 0;
        public uint CurrentPageIndex 
        { 
            get => _currentPageIndex; set 
            {
                _currentPageIndex = value;
                OnPropertyChanged();
            }
        }

        private string _selectedSortByArg = SortByArgs[0];
        public string SelectedSortByArg
        {
            get => _selectedSortByArg; set
            {
                _selectedSortByArg = value;
                OnPropertyChanged();
            }
        }

        private uint _pageSize = ShowPerPageList[0];
        public uint PageSize
        {
            get => _pageSize; set
            {
                _pageSize = value;
                OnPropertyChanged();
            }
        }



        private ObservableCollection<CurseforgeAddon> _addonsList { get; set; } = new ObservableCollection<CurseforgeAddon>();
        public IReadOnlyCollection<CurseforgeAddon> AddonList { get => _addonsList; }


        #endregion Properties


        #region Constructors


        public CurseforgeRepositoryModel(BaseInstanceData baseInstanceData)
        {
            _instanceData = baseInstanceData;
            Categories = new List<string>(
                "Addons|Adventure and RPG|API and Library|Armor, Tools, and Weapons|Cosmetic|Education|Food|Magic|Map and Information|MCreator|Miscellaneous|Redstone|Server Utility|Storage|Technology|Twitch Integration|Utility & QoL|World Gen".Split('|'));
            LoadPageContent();
        }


        #endregion Constructors


        public void OnPageIndexChanged(uint index) 
        {
            CurrentPageIndex = index;
            LoadPageContent();
        }


        private void LoadPageContent()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var addonsList = InstanceAddon.GetAddonsCatalog(
                    _instanceData, (int)PageSize, (int)CurrentPageIndex, AddonType.Mods, CurseforgeApi.GetCategories(CfProjectType.Mods)[0], SearchFilter
                    );

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList = new ObservableCollection<CurseforgeAddon>(addonsList.Select(i => new CurseforgeAddon(i)));
                    OnPropertyChanged(nameof(AddonList));
                });
            });
        }
    }
}
