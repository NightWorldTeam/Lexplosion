using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
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

        public PaginatorViewModel PaginatorVM { get; private set; }
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
            LibraryOutsideIds = MainModel.GetOutsideIds();
            Lexplosion.Run.TaskRun(delegate ()
            {
                var instances = OutsideDataManager.GetInstances(
                    SearchBoxVM.SelectedInstanceSource, _pageSize, PaginatorVM.PageIndex, ModpacksCategories.All, SearchBoxVM.SearchTextComfirmed
                    );

                if (instances.Count == 0)
                {

                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        InstanceForms.Clear();
                        for (int j = 0; j < instances.Count; j++)
                        {
                            if (LibraryOutsideIds.Contains(instances[j].Id))
                            {
                                InstanceForms.Add(MainModel.GetSpecificVM(instances[j].Id));
                            }
                            else
                            {
                                InstanceForms.Add(
                                    new InstanceFormViewModel(
                                        new InstanceFormModel(
                                            new InstanceProperties
                                            {
                                                Name = instances[j].Name,
                                                Type = instances[j].Type,
                                                LocalId = instances[j].LocalId,
                                                InstanceAssets = new InstanceAssets()
                                                {
                                                    author = instances[j].InstanceAssets.author,
                                                    description = instances[j].InstanceAssets.description,
                                                    categories = instances[j].Categories
                                                },
                                                Id = instances[j].Id,
                                                Logo = Utilities.ToImage(instances[j].MainImage),
                                                IsDownloadingInstance = false,
                                                IsInstalled = false,
                                                UpdateAvailable = false,
                                                IsInstanceAddedToLibrary = false
                                            }
                                            )
                                        )
                                    );
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