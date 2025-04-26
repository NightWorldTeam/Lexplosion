using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.FIlterPanel
{
    public class CatalogFilterPanel : ObservableObject
    {
        public event Action FilterChanged;


        #region Properties


        public ITranslatableObject<InstanceSource> NoneSource { get; } = new InstanceSourceObject("AllSource", InstanceSource.None);
        public MinecraftVersion AllVersion { get; } = new MinecraftVersion("All", MinecraftVersion.VersionType.Release);

        private IList<SortByParamObject> _sortByParams = new ObservableCollection<SortByParamObject>();
        public IList<SortByParamObject> SortByParams 
        { 
            get => _sortByParams; set
            {
                _sortByParams = value;
                SelectedSortByParam = value.FirstOrDefault();
                OnPropertyChanged();
            }
        
        }

        private SortByParamObject _selectedSortByParam;
        public SortByParamObject SelectedSortByParam 
        {
            get => _selectedSortByParam; set
            {
                _selectedSortByParam = value;
                FilterChangedExecuteEvent();
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
                FilterChangedExecuteEvent();
                OnPropertyChanged();
            }
        }


        private ITranslatableObject<InstanceSource> _selectedSource;
        public ITranslatableObject<InstanceSource> SelectedSource
        {
            get => _selectedSource; set
            {
                _selectedSource = value;
                
                UpdateCategories(value.Value);

                switch (value.Value) 
                {
                    case InstanceSource.Modrinth:
                        {
                            _sortByParams.Clear();
                            foreach (var index in Enum.GetValues(typeof(ModrinthSortField)))
                            {
                                var name = Enum.GetName(typeof(ModrinthSortField), index);

                                _sortByParams.Add(new SortByParamObject($"Modrinth{name}", (int)index));
                            }

                            SelectedSortByParam = _sortByParams.First();
                        }
                        break;
                    case InstanceSource.Curseforge:
                        {
                            SortByParams.Clear();
                            foreach (var index in Enum.GetValues(typeof(CfSortField)))
                            {
                                var name = Enum.GetName(typeof(CfSortField), index);

								var elem = new SortByParamObject($"Curseforge{name}", (int)index);
								if ((int)index == (int)CfSortField.Popularity) SelectedSortByParam = elem;

								_sortByParams.Add(elem);
                            }

                        }
                        break;
                    case InstanceSource.Nightworld:
                        _sortByParams.Clear();
                        FilterChangedExecuteEvent();
                        break;
                }


                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public CatalogFilterPanel(Action filterChange)
        {
            Sources.Add(new InstanceSourceObject("Curseforge", InstanceSource.Curseforge));
            Sources.Add(new InstanceSourceObject("Modrinth", InstanceSource.Modrinth));
            Sources.Add(new InstanceSourceObject("NightWorld", InstanceSource.Nightworld));

            UpdateCategories(Sources[0].Value);
            SelectedSource = Sources[0];

            FilterChanged += filterChange;

            if (MainViewModel.AllGameVersions == null)
            {
                MainViewModel.AllVersionsLoaded += MainViewModel_AllVersionsLoaded;
            }
            else 
            {
                MainViewModel_AllVersionsLoaded();
            }
        }

        private void MainViewModel_AllVersionsLoaded()
        {
            App.Current.Dispatcher.Invoke(() => 
            { 
                var versions = new ObservableCollection<MinecraftVersion>(MainViewModel.AllGameVersions);
                versions.Insert(0, AllVersion);
                SelectedVersion = versions[0];

                VersionCollectionViewSource.Source = versions;
                VersionCollectionViewSource.View.Filter = (mv) => ((mv as MinecraftVersion).Type == MinecraftVersion.VersionType.Release);
            });
        }


        #endregion Constructors


        private void UpdateCategories(InstanceSource instanceSource) 
        {
            AvailableCategories.Clear();
            var cats = Runtime.ServicesContainer.CategoriesService.GetModpackCategories(EnumManager.InstanceSourceToProjectSource(instanceSource)) ?? new List<CategoryBase>();
            foreach (var cat in cats.Where(mc => mc.Id != "-1"))
            {
                AvailableCategories.Add(cat);
            }
        }


        #region Public Methods


        public void ExecuteFilter() 
        {
            SelectedSource = Sources[0];
        }


        public void FilterChangedExecuteEvent([CallerMemberName] string member = "")
        {
            FilterChanged?.Invoke();
            Runtime.DebugWrite($"{member} FilterChanged Executed");
        }


        #endregion Public Methods
    }
}
