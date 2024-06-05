using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using System.Windows.Input;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.Logic.Objects;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class CatalogViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly NavigateCommand<ViewModelBase> _navigationCommand;

        public CatalogModel Model { get; }


        private bool _isCategoriesListOpen;
        public bool IsCategoriesListOpen 
        {
            get => _isCategoriesListOpen; set 
            {
                _isCategoriesListOpen = value;
                OnPropertyChanged();
            }
        }


        #region Commands


        private RelayCommand _openInstanceProfileMenu;
        public ICommand OpenInstanceProfileMenu
        {
            get => RelayCommand.GetCommand(ref _openInstanceProfileMenu, (obj) =>
            {
                var ins = (InstanceModelBase)obj;

                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_navigationStore, _navigationCommand, ins);
            });
        }

        private RelayCommand _searchCommand;
        public ICommand SearchCommand 
        {
            get => RelayCommand.GetCommand(ref _searchCommand, (obj) => Model.SearchFilterChanged(obj.ToString()));
        }

        private RelayCommand _nextPageCommand;
        public ICommand NextPageCommand 
        {
            get => RelayCommand.GetCommand<uint>(ref _nextPageCommand, Model.Paginate);
        }

        private RelayCommand _prevPageCommand;
        public ICommand PrevPageCommand 
        {
            get => RelayCommand.GetCommand<uint>(ref _prevPageCommand, Model.Paginate);
        }

        private RelayCommand _toCurrentPageIndexCommand;
        public ICommand ToCurrentPageIndexCommand
        {
            get => RelayCommand.GetCommand<uint>(ref _toCurrentPageIndexCommand, Model.Paginate);
        }


        private RelayCommand _selectCategoryCommand;
        public ICommand SelectCategoryCommand
        {
            get => RelayCommand.GetCommand<CategoryBase>(ref _selectCategoryCommand, (category) =>
            {
                if (Model.FilterPanel.SelectedCategories.Contains(category))
                {
                    Model.FilterPanel.SelectedCategories.Remove(category);
                }
                else 
                {
                    Model.FilterPanel.SelectedCategories.Add(category);
                }

                Model.FilterPanel.FilterChangedExecuteEvent();
            });
        }


        #endregion Commands


        #region Constructors


        public CatalogViewModel(INavigationStore navigationStore, NavigateCommand<ViewModelBase> navigationCommand, IInstanceController instanceController)
        {
            Model = new CatalogModel(instanceController);
            _navigationCommand = navigationCommand;
            _navigationStore = navigationStore;
        }


        #endregion Consturctors
    }
}
