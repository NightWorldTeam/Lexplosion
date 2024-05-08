using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
{
    public sealed class LibraryModel : ViewModelBase
    {
        private readonly IInstanceController _instanceController;
        public IReadOnlyCollection<InstanceModelBase> InstanceList { get => _instanceController.Instances; }
        public CollectionViewSource InstancesCollectionViewSource { get; } = new();


        private string _searchText;
        public string SearchText 
        {
            get => _searchText; set 
            {
                _searchText = value;
                OnSearchTextChanged();
            }
        }


        #region Constructors


        public LibraryModel(IInstanceController instanceController)
        {
            _instanceController = instanceController;
            InstancesCollectionViewSource.Source = _instanceController.Instances;
        }


        #endregion Constructors


        #region Private Methods


        private void OnSearchTextChanged() 
        {
            InstancesCollectionViewSource.View.Filter = (m => (m as InstanceModelBase).Name.IndexOf(SearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }


        #endregion Private Methods
    }
}
