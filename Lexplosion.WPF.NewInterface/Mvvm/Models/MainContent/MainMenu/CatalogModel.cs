using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            LoadPageContent();
        }


        #endregion Constructors


        #region Public & Properties Methods
        
        
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
                var instanceClientsTuple = GetInstanceClients(SearchFilter, CurrentPageIndex, InstanceSource.Modrinth, new IProjectCategory[] { AllCategory }, CfSortField.Featured, "1.19.2", false);

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
        public Tuple<IEnumerable<InstanceClient>, uint> GetInstanceClients(string searchInput, uint scrollTo, InstanceSource source, IEnumerable<IProjectCategory> selectedCategories, CfSortField sortBy, string gameVersion, bool isPaginatorInvoke = false)
        {
            var instanceClientList = InstanceClient.GetOutsideInstances(
                source,
                (int)ItemsPerPage,
                (int)scrollTo,
                selectedCategories,
                searchInput,
                sortBy,
                gameVersion
                );

            return new Tuple<IEnumerable<InstanceClient>, uint>(instanceClientList, (uint)instanceClientList.Count);
        }


        #endregion Public & Properties Methods
    }
}
