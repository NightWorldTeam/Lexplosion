using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Effects;

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
        private readonly AddonType _addonType;

        #region Properties


        public List<string> SelectedCategories { get; } = new List<string>();//(1) { "Adventure and RPG" };
        public List<string> Categories { get; }



        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                Runtime.DebugWrite(_searchFilter);
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
                LoadPageContent();
                OnPropertyChanged();
            }
        }

        private uint _pageSize = ShowPerPageList[0];
        public uint PageSize
        {
            get => _pageSize; set
            {
                _pageSize = value;
                LoadPageContent();
                OnPropertyChanged();
            }
        }



        private ObservableCollection<CurseforgeAddon> _addonsList { get; set; } = new ObservableCollection<CurseforgeAddon>();
        public IReadOnlyCollection<CurseforgeAddon> AddonList { get => _addonsList; }


        public bool IsContentLoading { get; private set; }


        #endregion Properties


        #region Constructors


        public CurseforgeRepositoryModel(BaseInstanceData baseInstanceData, AddonType addonType)
        {
            _instanceData = baseInstanceData;
            LoadPageContent();
            _addonType = addonType;
        }


        #endregion Constructors


        public void Search()
        {
            LoadPageContent();
        }


        public void OnPageIndexChanged(uint index)
        {
            CurrentPageIndex = index;
            LoadPageContent();
        }


        private void LoadPageContent()
        {
            IsContentLoading = true;
            OnPropertyChanged(nameof(IsContentLoading));
            Lexplosion.Runtime.TaskRun(() =>
            {
                var searchParams = new CurseforgeSearchParams(SearchFilter, string.Empty, [CurseforgeApi.GetCategories(_addonType.ToCfProjectType())[0]], (int)PageSize, (int)CurrentPageIndex, CfSortField.Featured);

                var addonsList = AddonsManager.GetManager(_instanceData).GetAddonsCatalog(ProjectSource.Curseforge, _addonType, searchParams);

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList = new ObservableCollection<CurseforgeAddon>(addonsList.Select(i => new CurseforgeAddon(i)));
                    OnPropertyChanged(nameof(AddonList));
                    IsContentLoading = false;
                    OnPropertyChanged(nameof(IsContentLoading));
                });
            });
        }

        public void InstallAddon(InstanceAddon addon)
        {
            var stateData = new DynamicStateData<SetValues<InstanceAddon, DownloadAddonRes>, InstanceAddon.InstallAddonState>();
            stateData.StateChanged += OnInstanceAddonInstallingStateChanged;

            addon.InstallLatestVersion(stateData.GetHandler);
        }

        private void OnInstanceAddonInstallingStateChanged(SetValues<InstanceAddon, DownloadAddonRes> arg, InstanceAddon.InstallAddonState state)
        {
            switch (state)
            {
                case InstanceAddon.InstallAddonState.StartDownload:

                    break;
                case InstanceAddon.InstallAddonState.EndDownload:

                    break;
            }
        }
    }
}
