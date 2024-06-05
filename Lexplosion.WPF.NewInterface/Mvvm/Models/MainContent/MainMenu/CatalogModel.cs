using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.FIlterPanel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
{
    public sealed class CatalogModel : VMBase
    {
        private static readonly SimpleCategory AllCategory = new SimpleCategory()
        {
            Id = "-1",
            ClassId = "",
            ParentCategoryId = "",
            Name = "All"
        };


        private readonly IInstanceController _instanceController;


        #region Properties


        public IEnumerable<InstanceModelBase> Instances { get => _instanceController.Instances; }

        public uint ItemsPerPage { get; set; } = 10;


        public CatalogFilterPanel FilterPanel { get; }


        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter; set
            {
                _searchFilter = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyPage;
        public bool IsEmptyPage
        {
            get => _isEmptyPage; set
            {
                _isEmptyPage = value;
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

        private uint _pageCount = 500;
        public uint PageCount
        {
            get => _pageCount; set 
            {
                _pageCount = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public CatalogModel(IInstanceController instanceController)
        {
            _instanceController = instanceController;
            FilterPanel = new CatalogFilterPanel();
            FilterPanel.FilterChanged += OnFilterChanged;
            LoadPageContent();
        }


        #endregion Constructors


        #region Public & Properties Methods
        

        private void OnFilterChanged() 
        {
            Runtime.TaskRun(() =>
            {
                var instanceClientsTuple = GetInstanceClients(
                    SearchFilter, 
                    CurrentPageIndex,
                    FilterPanel.SelectedSource.Value, 
                    FilterPanel.SelectedCategories.Count == 0 ? new IProjectCategory[] { AllCategory } : FilterPanel.SelectedCategories, 
                    CfSortField.Featured, 
                    FilterPanel.SelectedVersion, 
                    false);

                IsEmptyPage = instanceClientsTuple.Item2 == 0;

                if (PageCount != instanceClientsTuple.Item2)
                    PageCount = instanceClientsTuple.Item2;

                _instanceController.Clear();

                foreach (var i in instanceClientsTuple.Item1)
                    _instanceController.Add(i);
                OnPropertyChanged(nameof(Instances));
            });
        }

        
        public void Paginate(uint scrollTo)
        {
            CurrentPageIndex = scrollTo;
            LoadPageContent();
        }

        public void SearchFilterChanged(string searchFilter) 
        {
            SearchFilter = searchFilter;
            LoadPageContent();
        }

        private void LoadPageContent() 
        {
            Runtime.TaskRun(() => 
            {
                var instanceClientsTuple = GetInstanceClients(SearchFilter, CurrentPageIndex, InstanceSource.Modrinth, new IProjectCategory[] { AllCategory }, CfSortField.Featured, new MinecraftVersion(), false);

                IsEmptyPage = instanceClientsTuple.Item2 == 0;

                if (PageCount != instanceClientsTuple.Item2)
                    PageCount = instanceClientsTuple.Item2;

                foreach (var i in instanceClientsTuple.Item1)
                    _instanceController.Add(i);
                OnPropertyChanged(nameof(Instances));
            });
        }


        /// TODO: Сделать метод приватным
        /// <summary>
        /// Return a list of instances from curseforge/modrinth
        /// </summary>
        /// <param name="searchInput">SearchBox text</param>
        /// <param name="scrollTo">Page Index</param>
        /// <param name="source">Curseforge/Modrinth/Nightworld</param>
        /// <param name="selectedCategories">Selected Category(ies)</param>
        /// <param name="sortBy">Sort by data</param>
        /// <param name="gameVersion">version of minecraft</param>
        /// <param name="isPaginatorInvoke">Is page index changed?</param>
        /// <returns>Tuple[IEnumerable InstanceClient & InstanceClient count </returns>
        public Tuple<IEnumerable<InstanceClient>, uint> GetInstanceClients(string searchInput, uint scrollTo, InstanceSource source, IEnumerable<IProjectCategory> selectedCategories, CfSortField sortBy, MinecraftVersion gameVersion, bool isPaginatorInvoke = false)
        {
            Console.WriteLine(source);
            var instanceClientList = InstanceClient.GetOutsideInstances(
                source,
                (int)ItemsPerPage,
                (int)scrollTo,
                selectedCategories,
                searchInput,
                sortBy,
                gameVersion.Id == "All" ? "" : gameVersion.Id
                );

            return new Tuple<IEnumerable<InstanceClient>, uint>(instanceClientList, (uint)instanceClientList.Count);
        }


        #endregion Public & Properties Methods
    }
}
