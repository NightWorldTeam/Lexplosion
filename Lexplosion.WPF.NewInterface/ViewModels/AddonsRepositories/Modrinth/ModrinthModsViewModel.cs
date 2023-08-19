using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.AddonsRepositories.Modrinth
{
    public sealed class CategoryWrapper : ViewModelBase
    {
        public event Action<IProjectCategory, bool> SelectedEvent;

        private IProjectCategory _category { get; }

        public string Name { get => _category.Name; }


        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set 
            {
                _isSelected = value;
                OnPropertyChanged();
                OnSelectedChanged(value);
            }
        }

        public CategoryWrapper(IProjectCategory category)
        {
            _category = category;
        }

        private void OnSelectedChanged(bool value) 
        {
            SelectedEvent?.Invoke(_category, value);
        }
        
    }

    public sealed class ModrinthModsModel : ViewModelBase 
    {
        private readonly AddonType _addonType;
        private readonly ClientType _clientType;
        private readonly string _gameVersion;

        private int _pageSize = 10;
        public int PageSize 
        {
            get => _pageSize; set 
            {
                _pageSize = value;
                OnPageSizeChanged(value);
            }
        }

        private readonly ObservableCollection<CategoryWrapper> _categories;
        public IEnumerable<CategoryWrapper> Categories { get => _categories; }

        private readonly ObservableCollection<IProjectCategory> _selectedCategories;
        public IEnumerable<IProjectCategory> SelectedCategories;


        private ObservableCollection<ModrinthProjectInfo> _addonsList;
        public IEnumerable<ModrinthProjectInfo> AddonsList { get => _addonsList; }


        #region Constructors


        public ModrinthModsModel(AddonType addonType, ClientType clientType, string gameVersion)
        {
            _addonType = addonType;
            _clientType = clientType;
            _gameVersion = gameVersion;

            _categories = new ObservableCollection<CategoryWrapper>(PrepareCategories());

            var selectedCategories = new List<IProjectCategory>();
            var iterCount = 0;

            foreach (var category in _categories) 
            {
                //Console.WriteLine(category.Name + " " + category.Id.ToString());
                //if ((string.Equals(category.Name, "equipment", StringComparison.CurrentCultureIgnoreCase) || 
                //    string.Equals(category.Name, "food", StringComparison.CurrentCultureIgnoreCase)) && category.ClassId == "mod") { 
                //    selectedCategories.Add(category);
                //    iterCount++;
                //}
            }
        }


        #endregion Constructors


        #region Public Methods


        public void Search(string value) 
        {
            //_addonsList = new ObservableCollection<ModrinthProjectInfo>(ModrinthApi.GetAddonsList(_pageSize, 0, _addonType, selectedCategories, clientType, "", ""));
        }

        public void ClearFilters() { }


        #endregion Public Methods


        #region Private Methods

        private IEnumerable<CategoryWrapper> PrepareCategories() 
        {
            var categories = ModrinthApi.GetCategories();
            foreach (var category in categories) 
            {
                if (category.ClassId == "mod") 
                { 
                    var categoryWrapper = new CategoryWrapper(category);
                    categoryWrapper.SelectedEvent += OnSelectedCategoryChanged;
                    yield return categoryWrapper; 
                }
            }
        }


        private void OnSelectedCategoryChanged(IProjectCategory category, bool isSelected) 
        {
            if (isSelected)
            {
                _selectedCategories.Add(category);
            }
            else 
            {
                _selectedCategories.Remove(category);
            }
            //Search();
        }

        private void OnPageSizeChanged(int pageSize) 
        {
            
        }


        #endregion Private Methods
    }

    public class ModrinthModsViewModel : ViewModelBase
    {
        public ModrinthModsModel Model { get; } = new ModrinthModsModel(AddonType.Mods, ClientType.Fabric, "1.19.4");


        #region Commands


        private RelayCommand _clearFiltersCommand;
        public ICommand ClearFiltersCommand 
        {
            get => _clearFiltersCommand ?? (_clearFiltersCommand = new RelayCommand(obj => 
            {
                Model.ClearFilters();
            }));
        }

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand 
        {
            get => _searchCommand ?? (_searchCommand = new RelayCommand(obj => 
            {
                Model.Search((string)obj);
            }));
        }

        #endregion Commands
    }
}
