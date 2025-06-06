using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Specialized;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
{
    public sealed class LibraryModel : ViewModelBase
    {
        private readonly ILibraryInstanceController _instanceController;
        private readonly AppCore _appCore;


        #region Properties


        public FiltableObservableCollection InstancesCollectionViewSource { get; } = new();
        /// <summary>
        /// Группы сборок.
        /// </summary>
        public FiltableObservableCollection Groups { get; } = new();
        /// <summary>
        /// Группа пустая?
        /// </summary>
        public bool IsEmpty { get => _instanceController.Instances.Count == 0; }
        /// <summary>
        /// Выбранная группа.
        /// </summary>
        public InstancesGroup SelectedGroup { get => _instanceController.SelectedGroup;}
        /// <summary>
        /// Класс отвечающий за логику панели фильтрации.
        /// </summary>
        public LibraryFilterPanel FilterPanel { get; private set; }
        /// <summary>
        /// Открыто ли меню со списком групп
        /// </summary>
        public bool IsGroupDrawerOpen { get; set; }


        private string _searchText;
        /// <summary>
        /// Текст поиска для сборки
        /// </summary>
        public string SearchText 
        {
            get => _searchText; set 
            {
                _searchText = value;
                OnFilterChanged();
            }
        }

        private string _groupSearchText;
        /// <summary>
        /// Текст поиска для групп сборок
        /// </summary>
        public string GroupSearchText
        {
            get => _groupSearchText; set
            {
                _groupSearchText = value;
                OnGroupsFilterChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public LibraryModel(AppCore appCore, ClientsManager clientsManager, ILibraryInstanceController instanceController, string defaultGroupName = "default")
        {
            _appCore = appCore;
            _instanceController = instanceController;

            InstancesGroup defaultGroup;

            if (defaultGroupName == "default")
            {
                // Предполагаем, что стандартная группа всегда первая.
                defaultGroup = _instanceController.InstancesGroups.First();
            }
            else 
            {
                defaultGroup = _instanceController.InstancesGroups.FirstOrDefault(ig => ig.Name == defaultGroupName);
            }



            OpenInstanceGroup(defaultGroup);
        }


        #endregion Constructors


        #region Public Methods


        public void OpenInstanceGroup(InstancesGroup instancesGroup) 
        {
            if (FilterPanel != null) 
            {
                FilterPanel.FilterChanged -= OnFilterChanged;
            }

            _instanceController.SelectGroup(instancesGroup);

            FilterPanel = new(_instanceController);
            FilterPanel.FilterChanged += OnFilterChanged;


            if (instancesGroup.Clients is INotifyCollectionChanged notifyChangeCollection)
            {
                notifyChangeCollection.CollectionChanged += OnInstancesCollectionChanged;
            }

            OnPropertyChanged(nameof(SelectedGroup));

            Groups.Source = _instanceController.InstancesGroups;
            InstancesCollectionViewSource.Source = _instanceController.Instances;
        }

        /// <summary>
        /// Открывает/закрывает меню со списком групп
        /// </summary>
        public void ChangeOpenStateGroupDrawer(bool state) 
        {
            IsGroupDrawerOpen = state;
            OnPropertyChanged(nameof(IsGroupDrawerOpen));
        }

        ///
        public void AddGroup(InstancesGroup instancesGroup) 
        {
            _instanceController.AddGroup(instancesGroup);
        }

        public void RemoveGroup(InstancesGroup instancesGroup) 
        {
            _instanceController.RemoveGroup(instancesGroup);
        }

        #endregion Public Methods


        #region Private Methods

        private void OnInstancesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void OnFilterChanged() 
        {
            InstancesCollectionViewSource.Filter = (i =>
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
                var categories = instanceModelBase.BaseData.Categories.Skip(0);
                
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


        private void OnGroupsFilterChanged()
        {
            Groups.Filter = (i =>
            {
                var group = i as InstancesGroup;

                if (string.IsNullOrEmpty(GroupSearchText)) 
                {
                    return true;
                }

                if (group.Name == "All") 
                {
                    var defaultGroupCurrentLangName = _appCore.Resources("AllInstanceGroupName") as string;
                    return defaultGroupCurrentLangName.IndexOf(GroupSearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1;
                }



                return group.Name.IndexOf(GroupSearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1;
            });
        }

        #endregion Private Methods
    }
}
