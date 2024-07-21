using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lexplosion.Logic.Network.Web;
using System;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.Tools;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public sealed class CategoryGroup
    {
        public string Header { get; }
        public string IconData { get; }
        public IReadOnlyCollection<CategoryWrapper> Categories { get; }

        public CategoryGroup(string header, IEnumerable<CategoryWrapper> categories, string iconData = null)
        {
            Header = header;
            Categories = categories.ToArray<CategoryWrapper>();
            IconData = iconData;
        }
    }

    public abstract class AddonsRepositoryModelBase : ViewModelBase
    {
        protected static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            Name = "All",
            ClassId = "",
            ParentCategoryId = ""
        };
        protected BaseInstanceData _instanceData;
        protected readonly ProjectSource _projectSource;
        protected readonly AddonType _addonType;


        protected bool _isClearFilters = false;


        #region Properties


        public abstract ReadOnlyCollection<uint> PageSizes { get; }


        protected readonly ObservableCollection<CategoryWrapper> _categories = new();
        protected readonly ObservableCollection<IProjectCategory> _selectedCategories = new();
        protected ObservableCollection<InstanceAddon> _addonsList = new();
        protected ObservableCollection<CategoryGroup> _categoriesGroups = new();

        public IEnumerable<CategoryWrapper> Categories { get => _categories; }
        public IEnumerable<IProjectCategory> SelectedCategories { get => _selectedCategories; }
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }
        public IEnumerable<CategoryGroup> CategoriesGroups { get => _categoriesGroups; }

        public IEnumerable<SortByParamObject> SortByParams { get; protected set; }

        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                OnSearchFilterChanged();
            }
        }

        private byte _selectedSortByIndex = 0;
        public byte SelectedSortByIndex
        {
            get => _selectedSortByIndex; set
            {
                _selectedSortByIndex = value;
                OnSortByChanged();
            }
        }

        private uint _pageSize = 10;
        public uint PageSize
        {
            get => _pageSize; set
            {
                _pageSize = value;
                OnPageSizeChanged();
            }
        }

        private uint _currentPageIndex;
        public uint CurrentPageIndex
        {
            get => _currentPageIndex; set
            {
                _currentPageIndex = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        protected AddonsRepositoryModelBase(ProjectSource projectSource, BaseInstanceData instanceData, AddonType addonType)
        {
            _projectSource = projectSource;
            _instanceData = instanceData;
            _addonType = addonType;
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected abstract ISearchParams BuildSearchParams();
        protected abstract List<IProjectCategory> GetCategories();


        protected void LoadContent()
        {
            Runtime.TaskRun(() =>
            {
                var addons = InstanceAddon.GetAddonsCatalog(_projectSource, _instanceData, _addonType, BuildSearchParams());

                App.Current.Dispatcher.Invoke(() =>
                {
                    _addonsList.Clear();
                    foreach (var i in addons)
                    {
                        _addonsList.Add(i);
                    }
                });
            });
        }


        public void ClearFilters()
        {
            /*            _isClearFilters = true;
                        foreach (var category in Categories)
                        {
                            category.IsSelected = false;
                        }
                        _isClearFilters = false;
                        LoadPage();*/
        }

        public void InstallAddon(InstanceAddon instanceAddon)
        {
            //instanceAddon.InstallLatestVersion();
        }


        #endregion Public & Protected Methods 


        #region Private Methods


        protected virtual void OnSelectedCategoryChanged(IProjectCategory category, bool isSelected)
        {
            if (isSelected)
            {
                _selectedCategories.Add(category);
            }
            else
            {
                _selectedCategories.Remove(category);
            }
        }

        private void OnSortByChanged()
        {
            OnPropertyChanged(nameof(SelectedSortByIndex));
            LoadContent();
        }

        private void OnSearchFilterChanged()
        {
            OnPropertyChanged(nameof(SearchFilter));
            LoadContent();
        }

        private void OnCurrentPageIndexChanged()
        {
            OnPropertyChanged(nameof(CurrentPageIndex));
            LoadContent();
        }

        private void OnPageSizeChanged()
        {
            OnPropertyChanged(nameof(PageSize));
            LoadContent();
        }


        #endregion Private Methods
    }

    public sealed class AddonsRepositoryModel : AddonsRepositoryModelBase
    {
        private ICollection<IProjectCategory> _latestApplyCategories = new List<IProjectCategory>();
        private readonly Dictionary<string, List<CategoryWrapper>> _categoriesGroupsByName = new();

        private bool _hasConfirmCategories;
        public bool HasConfirmCategories
        {
            get => _hasConfirmCategories; set
            {
                _hasConfirmCategories = value;
                OnPropertyChanged();
            }
        }

        public override ReadOnlyCollection<uint> PageSizes { get; }


        #region Constructors


        public AddonsRepositoryModel(ProjectSource projectSource, BaseInstanceData instanceData, AddonType addonType)
            : base(projectSource, instanceData, addonType)
        {
            PageSizes = projectSource switch
            {
                ProjectSource.Modrinth => new([6, 10, 16, 20, 50, 100]),
                ProjectSource.Curseforge => new([10, 20, 50]),
                _ => new([10]),
            };
            PageSize = PageSizes[0];



            SortByParams = GetSortByParams();

            PrepareCategories();
            LoadContent();
        }


        #endregion Constructors


        #region Public & Protected Methods

        public void InstallAddon(InstanceAddon instanceAddon) 
        {
            var stateData = new DynamicStateData<SetValues<InstanceAddon, DownloadAddonRes>, InstanceAddon.InstallAddonState>();

            stateData.StateChanged += (arg, state) =>
            {
                if (arg.Value2 != DownloadAddonRes.Successful) 
                {
                    var s = 0;
                }
            };

            Runtime.TaskRun(() => 
            { 
                instanceAddon.InstallLatestVersion(stateData.GetHandler);
            });
        }


        protected override void OnSelectedCategoryChanged(IProjectCategory category, bool isSelected)
        {
            if (!HasConfirmCategories)
            {
                HasConfirmCategories = true;
                _latestApplyCategories.Clear();
                foreach (var i in _selectedCategories)
                    _latestApplyCategories.Add(i);
            }

            base.OnSelectedCategoryChanged(category, isSelected);

            bool isOld = _latestApplyCategories.Count == SelectedCategories.Count();

            if (isOld)
            {
                foreach (var i in _latestApplyCategories)
                {
                    if (!SelectedCategories.Contains(i))
                    {
                        isOld = false;
                        break;
                    }
                }

                if (HasConfirmCategories && isOld)
                {
                    HasConfirmCategories = false;
                }
            }
        }


        public void ApplyCategories()
        {
            HasConfirmCategories = false;
            LoadContent();
        }

        protected override ISearchParams BuildSearchParams()
        {
            if (_projectSource == ProjectSource.Curseforge)
            {
                return new CurseforgeSearchParams(SearchFilter, _instanceData.GameVersion.ToString(),
                    SelectedCategories, (int)PageSize, (int)CurrentPageIndex, (CfSortField)SelectedSortByIndex,
                    new List<Modloader> { _instanceData.Modloader.ToModloader() });
            }
            else
            {
                return new ModrinthSearchParams(SearchFilter, _instanceData.GameVersion.ToString(),
                    SelectedCategories, (int)PageSize, (int)CurrentPageIndex, (ModrinthSortField)SelectedSortByIndex,
                    new List<Modloader> { _instanceData.Modloader.ToModloader() });
            }
        }

        protected override List<IProjectCategory> GetCategories()
        {
            if (_projectSource == ProjectSource.Modrinth)
                return ModrinthApi.GetCategories().ToList<IProjectCategory>();

            if (_projectSource == ProjectSource.Curseforge)
                return CurseforgeApi.GetCategories(_addonType.ToCfProjectType()).ToList<IProjectCategory>();

            return [];
        }


        #endregion Public & Protected


        #region Private Methods


        private IEnumerable<SortByParamObject> GetSortByParams() 
        {
            Type sortByParamsType = _projectSource switch
            {
                ProjectSource.Curseforge => typeof(CfSortField),
                ProjectSource.Modrinth => typeof(ModrinthSortField),
                _ => null,
            };

            // If Exception when ProjectSource has new Value)
            var enumValues = Enum.GetValues(sortByParamsType);
            var sortByParams = new SortByParamObject[enumValues.Length];
            var i = 0;
            
            foreach (var index in enumValues)
            {
                var name = Enum.GetName(sortByParamsType, index);
                sortByParams[i] = new(name, (int)index);
                i++;
            }

            return sortByParams;
        }

        private void PrepareCategories()
        {
            Runtime.TaskRun(() =>
            {
                var categories = GetCategories();
                Console.WriteLine(categories.Count);

                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var category in categories)
                    {
                        //Console.WriteLine($"{category.Id} {category.Name} {category.ClassId} {category.ParentCategoryId}");

                        if (category.Id == "-1")
                            continue;

                        if (_projectSource == ProjectSource.Modrinth)
                        {
                            if (!_addonType.ToString().ToLower().Contains(category.ClassId.ToLower()))
                                continue;
                        }


                        if (!_categoriesGroupsByName.ContainsKey(category.ParentCategoryId))
                        {
                            _categoriesGroupsByName.Add(category.ParentCategoryId, new List<CategoryWrapper>());
                        }

                        var categoryWrapper = new CategoryWrapper(category);
                        categoryWrapper.SelectedEvent += OnSelectedCategoryChanged;
                        _categories.Add(categoryWrapper);
                        _categoriesGroupsByName[category.ParentCategoryId].Add(categoryWrapper);
                    }

                    foreach (var item in _categoriesGroupsByName)
                    {
                        _categoriesGroups.Add(new CategoryGroup(GetCategoryGroupHeader(item.Key, categories), item.Value));
                    }
                });
            });
        }

        private string GetCategoryGroupHeader(string header, IEnumerable<IProjectCategory> categories)
        {
            if (_projectSource == ProjectSource.Modrinth)
            {
                return header;
            }
            if (int.TryParse(header, out var newHeader))
            {
                if (header == "6")
                    return "All";
                /*              Id Name ClassId ParentCategoryId}
                                426 Addons 6 6
                                427 Thermal Expansion 6 426*/
                // not working
                var g = categories.FirstOrDefault(c => c.Id == header && c.ParentCategoryId == "6");
                return g == null ? string.Empty : g.Name;
            }
            return header;
        }

        internal void SelectCategory(IProjectCategory category)
        {
            foreach (var i in Categories) 
            {
                if (i.GetHashCode() == category.GetHashCode())
                    i.IsSelected = true;
                Console.WriteLine($"{i.Name} {i.GetHashCode()} == {i.GetHashCode()} {i.Name}");
            }
        }


        #endregion Private Methods
    }
}
/*-1 All 6 6
435 Server Utility 6 6
4671 Twitch Integration 6 6
424 Cosmetic 6 6
421 API and Library 6 6
422 Adventure and RPG 6 6
419 Magic 6 6
5314 KubeJS 6 426
6821 Bug Fixes 6 6
434 Armor, Tools, and Weapons 6 6
429 Industrial Craft 6 426
432 Buildcraft 6 426
408 Ores and Resources 6 406
411 Mobs 6 406
420 Storage 6 6
6484 Create 6 426
436 Food 6 6
407 Biomes 6 406
415 Energy, Fluid, and Item Transport 6 412
430 Thaumcraft 6 426
416 Farming 6 412
4843 Automation 6 412
406 World Gen 6 6
410 Dimensions 6 406
4773 CraftTweaker 6 426
417 Energy 6 412
409 Structures 6 406
433 Forestry 6 426
423 Map and Information 6 6
418 Genetics 6 412
Id Name ClassId ParentCategoryId}
426 Addons 6 6
427 Thermal Expansion 6 426
6814 Performance 6 6
5299 Education 6 6
412 Technology 6 6
413 Processing 6 412
4485 Blood Magic 6 426
6145 Skyblock 6 426
428 Tinker's Construct 6 426
4545 Applied Energistics 2 6 426
414 Player Transport 6 412
5232 Galacticraft 6 426
425 Miscellaneous 6 6
6954 Integrated Dynamics 6 426
4558 Redstone 6 6
5191 Utility & QoL 6 6
4906 MCreator 6 6*/