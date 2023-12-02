using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class CatalogViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly NavigateCommand<ViewModelBase> _navigationCommand;

        public CatalogModel Model { get; }


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

        #endregion Commands


        #region Constructors


        public CatalogViewModel(INavigationStore navigationStore, NavigateCommand<ViewModelBase> navigationCommand)
        {
            Model = new CatalogModel();
            _navigationCommand = navigationCommand;
            _navigationStore = navigationStore;
        }


        #endregion Consturctors
    }
}
