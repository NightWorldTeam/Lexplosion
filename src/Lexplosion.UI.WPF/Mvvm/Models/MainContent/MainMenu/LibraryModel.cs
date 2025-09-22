using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent
{
    public sealed class LibraryModel : ViewModelBase
    {
        private readonly ILibraryInstanceController _instanceController;
        private readonly AppCore _appCore;
        private readonly InstancesGroup _defaultGroup;


        #region Properties


        internal ILibraryInstanceController InstanceController { get; }
        public IEnumerable<string> AvailableImportFileExtensions { get; } = ["zip", "nwpk", "mrpack"];
        public Action<IEnumerable<string>> ImportAction { get; }

        public FiltableObservableCollection InstancesCollectionViewSource { get; } = new();
        /// <summary>
        /// Группы сборок.
        /// </summary>
        public FiltableObservableCollection Groups { get; } = new();
        /// <summary>
        /// Группа пустая?
        /// </summary>
        public bool IsEmpty { get; private set; }
        /// <summary>
        /// Выбранная группа.
        /// </summary>
        public InstancesGroup SelectedGroup { get => _instanceController.SelectedGroup; }
        /// <summary>
        /// Класс отвечающий за логику панели фильтрации.
        /// </summary>
        public LibraryFilterPanel FilterPanel { get; private set; }
        /// <summary>
        /// Открыто ли меню со списком групп
        /// </summary>
        private bool _isGroupDrawerOpen;
        public bool IsGroupDrawerOpen
        {
            get => _isGroupDrawerOpen; set
            {
                _isGroupDrawerOpen = value;
                IsGroupDrawerEnabled = !value;
                OnPropertyChanged();
            }
        }

        private bool _isGroupDrawerEnabled = true;
        public bool IsGroupDrawerEnabled
        {
            get => _isGroupDrawerEnabled; set
            {
                _isGroupDrawerEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isModalOpened = false;
        public bool IsModalOpened
        {
            get => _isModalOpened; set
            {
                _isModalOpened = value;
                OnPropertyChanged();
            }
        }

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

        public bool IsSelectedGroupDefault { get => SelectedGroup == null ? true : SelectedGroup.IsDefaultGroup; }


        #endregion Properties


        #region Constructors


        public LibraryModel(AppCore appCore, ImportStartFunc importStart, ClientsManager clientsManager, ILibraryInstanceController instanceController, string defaultGroupName = "default")
        {
            _appCore = appCore;
            _instanceController = instanceController;
            InstanceController = instanceController;

            if (defaultGroupName == "default")
            {
                // Предполагаем, что стандартная группа всегда первая.
                _defaultGroup = _instanceController.InstancesGroups.First();
            }
            else
            {
                _defaultGroup = _instanceController.InstancesGroups.FirstOrDefault(ig => ig.Name == defaultGroupName);
            }

            OpenInstanceGroup(_defaultGroup);

            ImportAction = (files) =>
            {
                Runtime.TaskRun(() =>
                {
                    foreach (var file in files) 
                    {
                        importStart(file, false, null, null, null);
                    }
                });
            };
        }




        #endregion Constructors


        #region Public Methods


        public void OpenInstanceGroup(InstancesGroup instancesGroup)
        {
            if (FilterPanel != null)
            {
                FilterPanel.FilterChanged -= OnFilterChanged;
            }

            if (SelectedGroup != null && SelectedGroup.Clients is INotifyCollectionChanged oldNotifyChangeCollection)
            {
                oldNotifyChangeCollection.CollectionChanged -= OnInstancesCollectionChanged;
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
            IsEmpty = _instanceController.Instances.Count == 0;
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsSelectedGroupDefault));
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
            if (SelectedGroup == instancesGroup)
            {
                _instanceController.SelectGroup(_defaultGroup);
            }
            _instanceController.RemoveGroup(instancesGroup);
        }

        #endregion Public Methods


        #region Private Methods

        private void OnInstancesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (e.OldItems.Count == _instanceController.Instances.Count)
                    IsEmpty = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (_instanceController.Instances.Count == 0)
                    IsEmpty = false;
            }

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
