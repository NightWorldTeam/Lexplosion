using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lexplosion.Logic.Network.Web;
using System;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories.Groups;
using Lexplosion.WPF.NewInterface.Core.GameExtensions;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories
{
    public sealed class LexplosionAddonsRepositoryModel : AddonsRepositoryModelBase
    {
        private readonly AppCore _appCore;
        private readonly ICollection<IProjectCategory> _latestApplyCategories = new List<IProjectCategory>();
        private readonly ICollection<object> _latestApplyFilterChanges = new List<object>();
        private readonly Dictionary<string, List<CategoryWrapper>> _categoriesGroupsByName = new();
        private readonly Action _launchInstanceAction;
        private readonly InstanceModelBase _instanceModelBase;

        public ObservableCollection<InstanceAddon> InstalledAddons { get; set; } = [];
        public ObservableCollection<DownloableAddonFile> InProgressAddons { get; set; } = [];


        public bool IsAddonTypeMaps { get; set; }
        public bool IsAddonTypeMods { get; set; }
        public bool IsAddonTypeResourcepacks { get; set; }
        public bool IsAddonTypeShaders { get; set; }


        private bool _hasUnconfirmChanges;
        public bool HasUnconfirmChanges
        {
            get => _hasUnconfirmChanges; set
            {
                _hasUnconfirmChanges = value;
                OnPropertyChanged();
            }
        }

        public override ReadOnlyCollection<int> PageSizes { get; }


        public bool IsGameLoaded { get; set; }


        #region Constructors


        public LexplosionAddonsRepositoryModel(AppCore appCore, ProjectSource projectSource, BaseInstanceData instanceData, AddonType addonType, InstanceModelBase instanceModelBase, bool isDefaultSelected = false)
            : base(projectSource, instanceData, addonType, isDefaultSelected)
        {
            _appCore = appCore;
            _instanceModelBase = instanceModelBase;
            PageSizes = projectSource switch
            {
                ProjectSource.Modrinth => new([5, 10, 15, 20, 50, 100]),
                ProjectSource.Curseforge => new([10, 20, 50]),
                _ => new([10]),
            };

            PageSize = 10;

            IsAddonTypeMods = addonType == AddonType.Mods;
            IsAddonTypeMaps = addonType == AddonType.Maps;
            IsAddonTypeShaders = addonType == AddonType.Shaders;
            IsAddonTypeResourcepacks = addonType == AddonType.Resourcepacks;

            SortByParams = GetSortByParams();

            PrepareCategories();

            instanceModelBase.GameLaunchCompleted += (o) =>
                {
                    IsGameLoaded = true;
                    OnPropertyChanged(nameof(IsGameLoaded));
                };
            _launchInstanceAction = instanceModelBase.Run;

            Runtime.TaskRun(() =>
            {
                var installedAddons = AddonsManager.GetManager(instanceData).GetInstalledAddons(addonType);
                App.Current.Dispatcher.Invoke(() =>
                {
                    InstalledAddons = new(installedAddons);
                    OnPropertyChanged(nameof(InstalledAddons));
                });
            });
            HasUnconfirmChanges = false;
        }


        #endregion Constructors


        #region Public & Protected Methods

        public void InstallAddon(InstanceAddon instanceAddon, object addonVersion = null)
        {
            var stateData = new DynamicStateData<SetValues<InstanceAddon, DownloadAddonRes>, InstanceAddon.InstallAddonState>();
            var downloableAddonFile = new DownloableAddonFile(instanceAddon);
            stateData.StateChanged += (arg, state) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (state == InstanceAddon.InstallAddonState.StartDownload)
                    {
                        InProgressAddons.Remove(new DownloableAddonFile(arg.Value1));
                    }
                    else if (state == InstanceAddon.InstallAddonState.EndDownload)
                    {
                        if (arg.Value2 == DownloadAddonRes.Successful)
                        {
                            InstalledAddons.Add(instanceAddon);
                        }
                        InProgressAddons.Remove(new DownloableAddonFile(arg.Value1));
                    }
                });
            };

            InProgressAddons.Add(downloableAddonFile);

            var acceptableModloaders = SelectedModloaders
                .Where(i => (int)i.EnumValue != (int)_instanceModelBase.BaseData.Modloader)
                .Select(i => i.EnumValue);

            Runtime.TaskRun(() =>
            {

                if (addonVersion == null)
                {
                    instanceAddon.InstallLatestVersion(stateData.GetHandler, acceptableModloaders: acceptableModloaders);
                }
                else
                {
                    instanceAddon.InstallSpecificVersion(stateData.GetHandler, acceptableModloaders: acceptableModloaders, versionInfo: addonVersion);
                }
            });
        }


        public void InstallAddonCurrentVersion(InstanceAddon instanceAddon)
        {
            var acceptableModloaders = SelectedModloaders
                .Where(i => (int)i.EnumValue != (int)_instanceModelBase.BaseData.Modloader)
                .Select(i => i.EnumValue);

            _appCore.ModalNavigationStore.Open(new SelectAddonVersionViewModel(instanceAddon, InstallAddon, acceptableModloaders));
        }


        protected override void OnSelectedCategoryChanged(IProjectCategory category, bool isSelected)
        {
            if (!HasUnconfirmChanges)
            {
                HasUnconfirmChanges = true;
                _latestApplyFilterChanges.Clear();
                foreach (var i in _selectedCategories)
                    _latestApplyFilterChanges.Add(i);
            }

            base.OnSelectedCategoryChanged(category, isSelected);

            bool isOld = _latestApplyFilterChanges.Count == SelectedCategories.Count() + SelectedModloaders.Count();

            if (isOld)
            {
                foreach (var i in _latestApplyFilterChanges)
                {
                    if (i is IProjectCategory && !SelectedCategories.Contains(i as IProjectCategory))
                    {
                        isOld = false;
                        break;
                    }
                }

                if (HasUnconfirmChanges && isOld)
                {
                    HasUnconfirmChanges = false;
                }
            }
        }


        protected override void OnModloaderSelectedChanged(Core.Objects.Modloader modloader, bool isSelected)
        {
            if (!HasUnconfirmChanges)
            {
                HasUnconfirmChanges = true;
                _latestApplyFilterChanges.Clear();
                foreach (var i in _selectedCategories)
                    _latestApplyFilterChanges.Add(i);
            }

            base.OnModloaderSelectedChanged(modloader, isSelected);

            bool isOld = _latestApplyFilterChanges.Count == SelectedCategories.Count() + SelectedModloaders.Count();

            if (isOld)
            {
                foreach (var i in _latestApplyFilterChanges)
                {
                    if (i is Modloader && !SelectedModloaders.Contains(i as Core.Objects.Modloader))
                    {
                        isOld = false;
                        break;
                    }
                }

                if (HasUnconfirmChanges && isOld)
                {
                    HasUnconfirmChanges = false;
                }
            }
        }

        public void ApplyCategories()
        {
            HasUnconfirmChanges = false;
            LoadContent();
        }

        protected override ISearchParams BuildSearchParams()
        {
            if (_projectSource == ProjectSource.Curseforge)
            {
                return new CurseforgeSearchParams(SearchFilter, _instanceData.GameVersion.ToString(),
                    SelectedCategories, (int)PageSize, (int)CurrentPageIndex, (CfSortField)SelectedSortByIndex,
                    SelectedModloaders.Select(m => m.EnumValue).ToList());
            }
            else
            {
                return new ModrinthSearchParams(SearchFilter, _instanceData.GameVersion.ToString(),
                    SelectedCategories, (int)PageSize, (int)CurrentPageIndex, (ModrinthSortField)SelectedSortByIndex,
                    SelectedModloaders.Select(m => m.EnumValue).ToList());
            }
        }

        protected override List<IProjectCategory> GetCategories()
        {
            return CategoriesManager.GetAddonsCategories(_projectSource, _addonType).ToList<IProjectCategory>() ?? [];
        }


        public void LaunchInstance()
        {
            _launchInstanceAction?.Invoke();
        }


        public void StopInstanceProcess()
        {

        }

        public void OpenWebsite(InstanceAddon addon)
        {
            try
            {
                System.Diagnostics.Process.Start(addon.WebsiteUrl);
            }
            catch
            {

            }
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
                var name = $"{_projectSource.ToString()}{Enum.GetName(sortByParamsType, index)}";
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

                // curseforge maps or shaders
                if (header == "12" || header == "17" || header == "6552")
                    return "categories";
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
            }
        }

        internal void RemoveAddon(InstanceAddon addon)
        {
            addon.Delete();
        }


        public void DisableAddon(InstanceAddon addon)
        {
            addon.IsEnable = false;
        }

        public void EnableAddon(InstanceAddon addon)
        {
            addon.IsEnable = true;
        }


        #endregion Private Methods
    }
}