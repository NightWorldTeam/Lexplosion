using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.FIlterPanel
{
    public class CatalogFilterPanel : ObservableObject
    {
        public event Action FilterChanged;


        private static readonly string[] ModrinthSortByParams = [
            "Relevance",
            "DownloadCount",
            "FollowCount",
            "RecentlyPublished",
            "RecentlyUpdated"
        ];

        private static readonly string[] CurseforgeSortByParams = [
            "Relevance",
            "Popularity",
            "LatestUpdate",
            "CreationDate",
            "TotalDownloads",
            "A-Z"
        ];


        private readonly IEnumerable<SortByParamObject> _sortByParams = CurseforgeSortByParams.Select(s => new SortByParamObject(s, s));


        #region Properties


        public ITranslatableObject<InstanceSource> NoneSource { get; } = new InstanceSourceObject("AllSource", InstanceSource.None);
        public MinecraftVersion AllVersion { get; } = new MinecraftVersion("All", MinecraftVersion.VersionType.Release);

        public IEnumerable<SortByParamObject> SortByParams { get => _sortByParams; }

        private SortByParamObject _selectedSortByParam;
        public SortByParamObject SelectedSortByParam 
        {
            get => _selectedSortByParam; set
            {
                _selectedSortByParam = value;
                FilterChanged?.Invoke();
                OnPropertyChanged();
            }
        }

        public IList<CategoryBase> AvailableCategories { get; } = new ObservableCollection<CategoryBase>();
        public ISet<CategoryBase> SelectedCategories { get; } = new HashSet<CategoryBase>();
        public IList<ITranslatableObject<InstanceSource>> Sources { get; } = new ObservableCollection<ITranslatableObject<InstanceSource>>();
        public CollectionViewSource VersionCollectionViewSource { get; } = new();


        private MinecraftVersion _selectedVersion;
        public MinecraftVersion SelectedVersion
        {
            get => _selectedVersion; set
            {
                _selectedVersion = value;
                FilterChanged?.Invoke();
                OnPropertyChanged();
            }
        }


        private ITranslatableObject<InstanceSource> _selectedSource;
        public ITranslatableObject<InstanceSource> SelectedSource
        {
            get => _selectedSource; set
            {
                _selectedSource = value;
                FilterChanged?.Invoke();

                UpdateCategories(value.Value);

                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public CatalogFilterPanel()
        {
            MainViewModel.AllVersionsLoaded += (() => 
            {
                App.Current.Dispatcher.Invoke(() => 
                { 
                    var versions = new ObservableCollection<MinecraftVersion>(MainViewModel.AllGameVersions);
                    versions.Insert(0, AllVersion);
                    SelectedVersion = versions[0];

                    VersionCollectionViewSource.Source = versions;
                    VersionCollectionViewSource.View.Filter = (mv) => ((mv as MinecraftVersion).Type == MinecraftVersion.VersionType.Release);
                });
            });

            UpdateCategories(InstanceSource.Modrinth);

            Sources.Add(new InstanceSourceObject("Modrinth", InstanceSource.Modrinth));
            Sources.Add(new InstanceSourceObject("Curseforge", InstanceSource.Curseforge));
            Sources.Add(new InstanceSourceObject("NightWorld", InstanceSource.Nightworld));

            SelectedSource = Sources[0];
            SelectedSortByParam = SortByParams.First();
        }


        #endregion Constructors


        private void UpdateCategories(InstanceSource instanceSource) 
        {
            AvailableCategories.Clear();
            foreach (var cat in CategoriesManager.GetModpackCategories(EnumManager.InstanceSourceToProjectSource(instanceSource)).Where(mc => mc.Id != "-1"))
            {
                AvailableCategories.Add(cat);
            }
        }


        #region Public Methods


        public void FilterChangedExecuteEvent()
        {
            FilterChanged?.Invoke();
        }


        #endregion Public Methods
    }
}
