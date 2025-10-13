﻿using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Stores;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class CatalogViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly INavigationStore _navigationStore;
        private readonly NavigateCommand<ViewModelBase> _navigationCommand;


        #region Properties


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


        #endregion Properties


        #region Commands


        private RelayCommand _openInstanceProfileMenu;
        public ICommand OpenInstanceProfileMenu
        {
            get => RelayCommand.GetCommand(ref _openInstanceProfileMenu, (obj) =>
            {
                var ins = (InstanceModelBase)obj;
                ins.PrepareDataForProfile();
                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_appCore, _navigationStore, _navigationCommand, ins);
            });
        }

        private RelayCommand _openAddonsPageCommand;
        public ICommand OpenAddonsPageCommand
        {
            get => RelayCommand.GetCommand(ref _openAddonsPageCommand, (obj) =>
            {
                var ins = (InstanceModelBase)obj;

                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_appCore, _navigationStore, _navigationCommand, ins);
                (_navigationStore.CurrentViewModel as InstanceProfileLayoutViewModel).OpenAddonContainerPage();
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


        public CatalogViewModel(AppCore appCore, NavigateCommand<ViewModelBase> navigationCommand, IInstanceController instanceController)
        {
            _appCore = appCore;
            Model = new CatalogModel(appCore, instanceController);
            _navigationCommand = navigationCommand;
            _navigationStore = appCore.NavigationStore;
        }


        #endregion Consturctors
    }
}
