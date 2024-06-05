using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Linq;
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
                OnFilterChanged();
            }
        }


        public LibraryFilterPanel FilterPanel { get; }


        #region Constructors


        public LibraryModel(IInstanceController instanceController)
        {
            _instanceController = instanceController;
            InstancesCollectionViewSource.Source = _instanceController.Instances;

            FilterPanel = new(instanceController);

            FilterPanel.FilterChanged += OnFilterChanged;
        }


        #endregion Constructors


        #region Private Methods


        private void OnFilterChanged() 
        {
            InstancesCollectionViewSource.View.Filter = (i =>
            {
                var instanceModelBase = i as InstanceModelBase;
                var searchBoxRes = string.IsNullOrEmpty(SearchText) ? true : instanceModelBase.Name.IndexOf(SearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1;
                var selectedVersionRes = false;


                // check versions
                if (FilterPanel.SelectedVersion == null) 
                {
                    FilterPanel.SelectedVersion = FilterPanel.Versions[0];
                    FilterPanel.SelectedIndex = 0;
                    OnPropertyChanged(nameof(FilterPanel.SelectedIndex));
                }

                if (FilterPanel.SelectedVersion.Id == "All" || FilterPanel.SelectedVersion.Id == instanceModelBase.GameVersion.Id)
                {
                    selectedVersionRes = true;
                }
                else 
                {
                    return false;
                }

                var selectedSourceRes = false;

                // check source
                if (FilterPanel.SelectedSource.Value == InstanceSource.None || FilterPanel.SelectedSource.Value == instanceModelBase.Source)
                {
                    selectedSourceRes = true;
                }
                else 
                {
                    return false;
                }

                // categories with or/and operators

                // skip first element because its version.
                var categories = instanceModelBase.InstanceData.Categories.Skip(0);
                
                var selectedCategoriesRes = false;
                if (FilterPanel.SelectedCategories.Count == 0) 
                {
                    return selectedSourceRes && selectedVersionRes && searchBoxRes;
                }
                else if (FilterPanel.IsOperatorAnd)
                {
                    selectedCategoriesRes = categories.Union(FilterPanel.SelectedCategories).ToArray().Length == categories.Count();
                }
                else 
                {
                    selectedCategoriesRes = categories.Intersect(FilterPanel.SelectedCategories).Any();
                }

                return selectedCategoriesRes && selectedSourceRes && selectedVersionRes && searchBoxRes;
            });
        }


        #endregion Private Methods
    }
}
