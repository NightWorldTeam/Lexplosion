﻿using Lexplosion.Logic.Management.Instances;
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

                    if (arg.Value2 == DownloadAddonRes.Successful)
                    {
                        InstalledAddons.Add(instanceAddon);
                    }
                    InProgressAddons.Remove(downloableAddonFile);
                });
            };

            InProgressAddons.Add(downloableAddonFile);

            Runtime.TaskRun(() =>
            {
                var acceptableModloaders = SelectedModloaders
                    .Where(i => (int)i.EnumValue != (int)_instanceModelBase.BaseData.Modloader)
                    .Select(i => i.EnumValue);

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
            _appCore.ModalNavigationStore.Open(new SelectAddonVersionViewModel(instanceAddon, InstallAddon));
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

        public void OpenWebsite() 
        {
            _instanceModelBase.GoToWebsite();
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