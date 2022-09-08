using Lexplosion.Gui.Models;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class CatalogViewModel : VMBase, IPaginable
    {
        private const int _pageSize = 10;
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<InstanceFormViewModel> InstanceList { get => _mainViewModel.Model.CurrentInstanceCatalog; } 

        #region props

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel(true);

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


        #endregion props


        #region commads


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


        #endregion commands


        public CatalogViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            GetInitializeInstance();
            SearchBoxVM.SearchChanged += GetInitializeInstance;
            PaginatorVM.PageChanged += GetInitializeInstance;
        }

        public async void GetInitializeInstance()
        {
            await Task.Run(() => InstancesPageLoading());
        }

        private void InstancesPageLoading()
        {
            // Сделать метод асинхронным.
            IsLoaded = false;
            Lexplosion.Run.TaskRun(delegate ()
            {
                var instances = InstanceClient.GetOutsideInstances(
                    SearchBoxVM.SelectedInstanceSource, _pageSize, PaginatorVM.PageIndex - 1, ModpacksCategories.All, SearchBoxVM.SearchTextComfirmed);

                if (instances.Count == _pageSize)
                {
                    IsPaginatorVisible = true;
                }
                else IsPaginatorVisible = false;

                if (instances.Count == 0)
                {
                    InstanceList.Clear();
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceList.Clear();
                        IsEmptyList = true;
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (IsEmptyList)
                            IsEmptyList = false;

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
    }
}