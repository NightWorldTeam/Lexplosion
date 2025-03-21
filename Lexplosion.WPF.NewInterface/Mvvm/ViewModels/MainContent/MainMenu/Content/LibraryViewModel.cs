using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Diagnostics.Eventing.Reader;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class LibraryViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenuLayoutCommand;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly Func<IEnumerable<InstanceModelBase>> _getInstances;


        #region Properties


        public LibraryModel Model { get; }
        public NotifyCallback Notify { get; }

        public bool IsScrollToEnd { get; set; }

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


        private RelayCommand _openInstanceProfileMenuCommand;
        public ICommand OpenInstanceProfileMenuCommand
        {
            get => RelayCommand.GetCommand(ref _openInstanceProfileMenuCommand, (obj) =>
            {
                var ins = (InstanceModelBase)obj;

                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_appCore, _navigationStore, _toMainMenuLayoutCommand, ins);
            });
        }

        private RelayCommand _openAddonsPageCommand;
        public ICommand OpenAddonsPageCommand
        {
            get => RelayCommand.GetCommand(ref _openAddonsPageCommand, (obj) =>
            {
                var ins = (InstanceModelBase)obj;

                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_appCore, _navigationStore, _toMainMenuLayoutCommand, ins);

                (_navigationStore.CurrentViewModel as InstanceProfileLayoutViewModel).OpenAddonContainerPage();
            });
        }


        private RelayCommand _openInstanceFactory;
        public ICommand OpenInstanceFactoryCommand
        {
            get => RelayCommand.GetCommand(ref _openInstanceFactory, () =>
            {
                _modalNavigationStore.OpenModalPageByType(typeof(InstanceFactoryViewModel));
            });
        }


        private RelayCommand _selectSourceCommand;
        public ICommand SelectSourceCommand
        {
            get => RelayCommand.GetCommand<ITranslatableObject<InstanceSource>>(ref _selectSourceCommand, (o) =>
            {
                if (Model.FilterPanel.SelectedSource.Value == o.Value)
                    return;

                Model.FilterPanel.SelectedSource = o;
            });
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

        private RelayCommand _moveToCatalogCommand;
        public ICommand MoveToCatalogCommand { get; }


        #endregion Commands


        // TODO: думаю делегат с инстансами это костыль ченить другое надо придумать
        public LibraryViewModel(AppCore appCore, INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, ModalNavigationStore modalNavigationStore, IInstanceController instanceController, Action moveToCatalog, NotifyCallback? notify = null)
        {
            _appCore = appCore;
            Notify = notify;
            Model = new LibraryModel(instanceController);
            _navigationStore = navigationStore;
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
            _modalNavigationStore = modalNavigationStore;
            MoveToCatalogCommand = RelayCommand.GetCommand(ref _moveToCatalogCommand, moveToCatalog);
        }
    }
}
