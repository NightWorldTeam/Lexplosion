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
using Lexplosion.Logic.Management.Instances;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class LibraryViewModel : ViewModelBase
    {
        public event Action<InstanceModelBase> InstanceProfileOpened;

        private readonly AppCore _appCore;
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenuLayoutCommand;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly Func<IEnumerable<InstanceModelBase>> _getInstances;
        private readonly ClientsManager _clientsManager;


        #region Properties


        public LibraryModel Model { get; }

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
                InstanceProfileOpened?.Invoke(ins);
                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_appCore, _navigationStore, _toMainMenuLayoutCommand, ins);
            });
        }

        private RelayCommand _openAddonsPageCommand;
        public ICommand OpenAddonsPageCommand
        {
            get => RelayCommand.GetCommand(ref _openAddonsPageCommand, (obj) =>
            {
                var ins = (InstanceModelBase)obj;
                InstanceProfileOpened?.Invoke(ins);
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

        private RelayCommand _selectGroupCommand;
        public ICommand SelectGroupCommand
        {
            get => RelayCommand.GetCommand<InstancesGroup>(ref _selectGroupCommand, Model.OpenInstanceGroup);
        }


        private RelayCommand _changeOpenStateGroupDrawerCommand;
        public ICommand ChangeOpenStateGroupDrawerCommand
        {
            get => RelayCommand.GetCommand<bool>(ref _changeOpenStateGroupDrawerCommand, Model.ChangeOpenStateGroupDrawer);
        }

        private RelayCommand _openInstancesFactoryModalCommand;
        public ICommand OpenInstancesFactoryModalCommand
        {
            get => RelayCommand.GetCommand(ref _openInstancesFactoryModalCommand, OpenInstancesGroupFactoryModal);
        }

        private RelayCommand _editInstancesGroupCommand;
        public ICommand EditInstancesGroupCommand
        {
            get => RelayCommand.GetCommand<InstancesGroup>(ref _editInstancesGroupCommand, OpenInstancesGroupEditModal);
        }

        private RelayCommand _deleteInstancesGroupCommand;
        public ICommand DeleteInstancesGroupCommand
        {
            get => RelayCommand.GetCommand<InstancesGroup>(ref _deleteInstancesGroupCommand, RemoveInstancesGroupModal);
        }


        #endregion Commands


        // TODO: думаю делегат с инстансами это костыль ченить другое надо придумать
        public LibraryViewModel(AppCore appCore, ClientsManager clientsManager, ICommand toMainMenuLayoutCommand, ILibraryInstanceController instanceController, Action moveToCatalog)
        {
            _appCore = appCore;
            _clientsManager = clientsManager;
            Model = new LibraryModel(_appCore, clientsManager, instanceController);
            _navigationStore = appCore.NavigationStore;
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
            _modalNavigationStore = appCore.ModalNavigationStore;
            MoveToCatalogCommand = RelayCommand.GetCommand(ref _moveToCatalogCommand, moveToCatalog);
        }


        #region Private Methods


        private void OpenInstancesGroupFactoryModal()
        {
            var defaultGroup = _clientsManager.GetExistsGroups().First();
            var instancesFactoryModalViewModel = new InstancesGroupFactoryViewModel(_clientsManager, defaultGroup.Clients);

            instancesFactoryModalViewModel.Model.GroupCreated += Model.AddGroup;
            _appCore.ModalNavigationStore.Open(instancesFactoryModalViewModel);
        }

        private void OpenInstancesGroupEditModal(InstancesGroup instancesGroup)
        {
            var instancesFactoryModalViewModel = new InstancesGroupEditViewModel(instancesGroup, _clientsManager);
            _appCore.ModalNavigationStore.Open(instancesFactoryModalViewModel);
        }

        private void RemoveInstancesGroupModal(InstancesGroup instancesGroup)
        {
            var instancesFactoryModalViewModel = new ConfirmActionViewModel(
                    _appCore.Resources("RemoveInstancesGroupTitle") as string,
                    string.Format(_appCore.Resources("RemoveInstancesGroupDescription") as string, instancesGroup.Name),
                    _appCore.Resources("YesIWantToRemoveGroup") as string,
                (obj) => Model.RemoveGroup(instancesGroup));
            _appCore.ModalNavigationStore.Open(instancesFactoryModalViewModel);
        }


        #endregion Private Methods
    }
}
