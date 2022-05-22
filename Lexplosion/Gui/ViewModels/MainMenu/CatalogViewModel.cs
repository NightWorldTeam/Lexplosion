using Lexplosion.Gui.Models;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class CatalogViewModel : VMBase, IPaginable
    {
        private const int _pageSize = 10;

        public ObservableCollection<InstanceFormViewModel> InstanceForms { get; set; } = new ObservableCollection<InstanceFormViewModel>();
        public List<string> LibraryOutsideIds = new List<string>();

        public PaginatorViewModel PaginatorVM { get; }
        public SearchBoxViewModel SearchBoxVM { get; }

        public CatalogViewModel()
        {
            SearchBoxVM = new SearchBoxViewModel(() => GetInitializeInstance());
            PaginatorVM = new PaginatorViewModel(() => GetInitializeInstance());
            GetInitializeInstance();
        }

        public async void GetInitializeInstance()
        {
            await Task.Run(() => InstancesPageLoading());
        }

        private void InstancesPageLoading()
        {
            SearchBoxVM.IsLoaded = false;
            PaginatorVM.IsLoaded = false;
            LibraryOutsideIds.Clear();
            //LibraryOutsideIds = MainModel.GetOutsideIds();

            Lexplosion.Run.TaskRun(delegate ()
            {
                var instances = InstanceClient.GetOutsideInstances(
                SearchBoxVM.SelectedInstanceSource, _pageSize, PaginatorVM.PageIndex, ModpacksCategories.All, SearchBoxVM.SearchTextComfirmed);

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
                            if (MainModel.LibraryInstances.ContainsKey(instance))
                            {
                                InstanceForms.Add(MainModel.LibraryInstances[instance]);
                            }
                            else
                            {
                                InstanceForms.Add(new InstanceFormViewModel(instance));
                            }
                        }
                    });

                }
                SearchBoxVM.IsLoaded = true;
                PaginatorVM.IsLoaded = true;
            });
        }
    }
}