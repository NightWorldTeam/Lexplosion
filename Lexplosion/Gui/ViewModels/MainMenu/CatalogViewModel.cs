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

        public ObservableCollection<InstanceFormViewModel> InstanceForms { get; set; } = new ObservableCollection<InstanceFormViewModel>();

        public PaginatorViewModel PaginatorVM { get; } = new PaginatorViewModel();
        public SearchBoxViewModel SearchBoxVM { get; } = new SearchBoxViewModel(true);

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded; set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        public CatalogViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            SearchBoxVM.SearchChanged += GetInitializeInstance;
            PaginatorVM.PageChanged += GetInitializeInstance;
            GetInitializeInstance();
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
                var instances = InstanceClient.GetOutsideInstances(
                SearchBoxVM.SelectedInstanceSource, _pageSize, PaginatorVM.PageIndex - 1, ModpacksCategories.All, SearchBoxVM.SearchTextComfirmed);

                if (instances.Count == 0)
                {

                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceForms.Clear();
                        foreach (var instance in instances)
                        {
                            if (_mainViewModel.Model.IsLibraryContainsInstance(instance))
                            {
                                InstanceForms.Add(_mainViewModel.Model.GetInstance(instance));
                            }
                            else
                            {
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