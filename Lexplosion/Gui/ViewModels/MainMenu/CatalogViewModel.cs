using Lexplosion.Gui.Models;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class CatalogViewModel : VMBase, IPaginable
    {
        private const int _pageSize = 10;
        private readonly MainViewModel _mainViewModel;


        #region props
        
        
        public ObservableCollection<InstanceFormViewModel> InstanceForms { get; set; } = new ObservableCollection<InstanceFormViewModel>();

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
            get => _onScrollCommand ?? new RelayCommand(obj => 
            {
                // TODO: Возможно тяжелый код.
                foreach (var instance in InstanceForms)
                {
                    instance.IsDropdownMenuOpen = false;
                }
            });
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
            IsLoaded = false;
            Lexplosion.Run.TaskRun(delegate ()
            {
                //var instances = InstanceClient.GetOutsideInstances(
                //  SearchBoxVM.SelectedInstanceSource, _pageSize, PaginatorVM.PageIndex - 1, ModpacksCategories.All, SearchText);

                var instances = InstanceClient.GetOutsideInstances(
                    SearchBoxVM.SelectedInstanceSource, _pageSize, PaginatorVM.PageIndex - 1, ModpacksCategories.All, SearchBoxVM.SearchTextComfirmed);

                Console.WriteLine("Поиск по запросу: " + SearchBoxVM.SearchTextComfirmed + ". Найдено: " + instances.Count);

                if (instances.Count == _pageSize)
                {
                    IsPaginatorVisible = true;
                }
                else IsPaginatorVisible = false;

                if (instances.Count == 0)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceForms.Clear();
                        IsEmptyList = true;
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (IsEmptyList)
                            IsEmptyList = false;
                        
                        InstanceForms.Clear();

                        foreach (var instance in instances)
                        {
                            Console.WriteLine("\nInstance [" + instance.Name + "] in library ? " + instance.InLibrary );
                            if (_mainViewModel.Model.IsLibraryContainsInstance(instance))
                            {
                                Console.WriteLine("Get InstanceForm from Library [" + instance.Name + "]");
                                InstanceForms.Add(_mainViewModel.Model.GetInstance(instance));
                            }
                            else
                            {
                                Console.WriteLine("Create new InstanceForm [" + instance.Name + "]");
                                InstanceForms.Add(new InstanceFormViewModel(_mainViewModel, instance));
                            }
                        }
                    });

                }
                IsLoaded = true;
            });
        }
    }
}