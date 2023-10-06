using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Controls.Paginator;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Models.MainContent
{
    public sealed class CatalogModel : VMBase, IPaginable
    {
        #region Properties


        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();
        public IEnumerable<InstanceModelBase> Instances { get => _instances; }


        private bool _isEmptyPage;
        public bool IsEmptyPage
        {
            get => _isEmptyPage; set
            {
                _isEmptyPage = value;
                OnPropertyChanged();
            }
        }

        private bool _isLastPage;
        public bool IsLastPage
        {
            get => _isLastPage; set
            {
                _isLastPage = value;
                OnPropertyChanged();
            }
        }


        public uint ItemsPerPage { get; set; } = 10;


        #endregion Properties


        #region Constructors


        public CatalogModel()
        {
            var instanceClientsTuple = GetInstanceClients("", 0, InstanceSource.Modrinth, new IProjectCategory[] { new SimpleCategory()
            {
                Id = "-1",
                ClassId = "",
                ParentCategoryId = "",
                Name = "All"
            }}, CfSortField.Featured, "1.19.2", false);


            foreach (var instanceClient in instanceClientsTuple.Item1)
            {
                _instances.Add(new InstanceModelBase(instanceClient));
            }
            Runtime.DebugWrite(instanceClientsTuple.Item2);
        }


        #endregion Constructors

        #region Public & Properties Methods


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

        public void Paginate(uint scrollTo)
        {
            var tuple = GetInstanceClients("", scrollTo, InstanceSource.Nightworld, new IProjectCategory[] { new SimpleCategory() }, CfSortField.Popularity, "");

            IsEmptyPage = tuple.Item2 == 0;
            IsLastPage = tuple.Item2 < ItemsPerPage;
        }


        #endregion Public & Properties Methods
    }
}
