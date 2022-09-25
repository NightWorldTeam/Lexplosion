using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class CatalogViewModel : VMBase, IPaginable
    {
        private const int _pageSize = 10;
        private readonly MainViewModel _mainViewModel;


        #region Properties


        public ObservableCollection<InstanceFormViewModel> InstanceList { get => _mainViewModel.Model.CurrentInstanceCatalog; }

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel(true, true);


        private bool _isLoaded;
        /// <summary>
        /// <para>Отвечает на вопрос загрузилась ли страница.</para>
        /// <br><c>Загрузилась - true</c></br>
        /// <br><c>Загружается - false</c></br>
        /// </summary>
        public bool IsLoaded
        {
            get => _isLoaded; set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        private bool _isEmptyList;
        /// <summary>
        /// <para>Отвечает на вопрос количество найденого контента равно 0?</para>
        /// </summary>
        public bool IsEmptyList 
        {
            get => _isEmptyList; set 
            {
                _isEmptyList = value; 
                OnPropertyChanged();
            }
        }

        private bool _isPaginatorVisible = false;
        public bool IsPaginatorVisible 
        {
            get => _isPaginatorVisible; set 
            {
                _isPaginatorVisible = value; 
                OnPropertyChanged();
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText; set 
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _onScrollCommand;
        public RelayCommand OnScrollCommand 
        {
            get => _onScrollCommand ?? (_onScrollCommand = new RelayCommand(obj => 
            {
                // TODO: Возможно тяжелый код.
                foreach (var instance in _mainViewModel.Model.CurrentInstanceCatalog)
                {
                    instance.IsDropdownMenuOpen = false;
                }
            }));
        }


        #endregion Commands


        #region Constructors


        public CatalogViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            SearchBoxVM.SearchChanged += InstancesPageLoading;
            PaginatorVM.PageChanged += InstancesPageLoading;
            InstancesPageLoading();
        }


        #endregion Constructors


        #region Public & Protected Methods





        #endregion Public & Protected Methods


        #region Private Methods

        private void InstancesPageLoading(string searchText = "")
        {
            IsLoaded = false;
            Lexplosion.Run.TaskRun(() =>
            {
                var gameVersion = SearchBoxVM.SelectedVersion == null || SearchBoxVM.SelectedVersion.Contains(ResourceGetter.GetString("allVersions")) ? "" : SearchBoxVM.SelectedVersion;
                Console.WriteLine(gameVersion);
                var instances = InstanceClient.GetOutsideInstances(
                    SearchBoxVM.SelectedInstanceSource,
                    _pageSize,
                    PaginatorVM.PageIndex - 1,
                    SearchBoxVM.SelectedCurseforgeCategory.id,
                    SearchBoxVM.SearchTextComfirmed,
                    SearchBoxVM.SelectedCfSortBy,
                    gameVersion
                    );

                if (instances.Count == _pageSize) IsPaginatorVisible = true;
                else IsPaginatorVisible = false;

                if (instances.Count == 0)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        InstanceList.Clear();
                        IsEmptyList = true;
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (IsEmptyList) IsEmptyList = false;

                        InstanceList.Clear();

                        foreach (var instance in instances)
                        {
                            if (_mainViewModel.Model.IsLibraryContainsInstance(instance))
                            {
                                InstanceList.Add(_mainViewModel.Model.GetInstance(instance));
                            }
                            else
                            {
                                var instanceForm = new InstanceFormViewModel(_mainViewModel, instance);
                                InstanceList.Add(instanceForm);
                            }
                        }
                    });

                }
                IsLoaded = true;
            });
        }

        #endregion Private Methods
    }
}