using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Controls.Paginator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Models.MainContent.Content
{
    public class CatalogModel : VMBase, IPaginable
    {
        #region Properties


        private ObservableCollection<object> _instanceFormVMs = new ObservableCollection<object>();
        public IEnumerable<object> InstanceFormVMs { get => _instanceFormVMs; }


        private bool _isEmptyPage;
        public bool IsEmptyPage 
        {
            get => _isEmptyPage; set 
            {
                _isEmptyPage= value;
                OnPropertyChanged();
            }
        }

        private bool _isLastPage;
        public bool IsLastPage 
        {
            get => _isLastPage; set 
            {
                _isLastPage= value; 
                OnPropertyChanged();
            }
        }


        public uint ItemsPerPage { get; set; } = 10;


        #endregion Properties


        #region Public & Properties Methods


        public Tuple<IEnumerable<InstanceClient>, uint> GetInstanceClients(string searchInput, uint scrollTo, InstanceSource source, IEnumerable<IProjectCategory> selectedCategories, CfSortField sortBy, string gameVersion,  bool isPaginatorInvoke = false) 
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
