using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects.TranslatableObjects;
using Lexplosion.UI.WPF.Mvvm.Models;
using Lexplosion.UI.WPF.Mvvm.Models.InstanceControllers;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using Lexplosion.UI.WPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
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

        private RelayCommand _openInstancesGroupFactoryModalCommand;
        public ICommand OpenInstancesGroupFactoryModalCommand
        {
            get => RelayCommand.GetCommand(ref _openInstancesGroupFactoryModalCommand, OpenInstancesGroupFactoryModal);
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

        private RelayCommand _openInstanceToGroupsConfiguratorCommand;
        public ICommand OpenInstanceToGroupsConfiguratorCommand
        {
            get => RelayCommand.GetCommand<InstanceModelBase>(ref _openInstanceToGroupsConfiguratorCommand, OpenInstanceToGroupsConfigurator);
        }


        #endregion Commands


        // TODO: думаю делегат с инстансами это костыль ченить другое надо придумать
        public LibraryViewModel(AppCore appCore, ImportStartFunc importStart, ClientsManager clientsManager, ICommand toMainMenuLayoutCommand, ILibraryInstanceController instanceController, Action moveToCatalog)
        {
            _appCore = appCore;
            _clientsManager = clientsManager;
            Model = new LibraryModel(_appCore, importStart, clientsManager, instanceController);
            _navigationStore = appCore.NavigationStore;
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
            _modalNavigationStore = appCore.ModalNavigationStore;
            MoveToCatalogCommand = RelayCommand.GetCommand(ref _moveToCatalogCommand, moveToCatalog);

            _appCore.ModalNavigationStore.Opened += OnModalOpened;
            _appCore.ModalNavigationStore.Closed += OnModalClosed;
        }


        #region Private Methods


        private void OnModalClosed()
        {
            Model.IsGroupDrawerEnabled = true && !Model.IsGroupDrawerOpen;
            Model.IsModalOpened = false;
        }

        private void OnModalOpened()
        {
            Model.IsModalOpened = true;
            Model.IsGroupDrawerEnabled = false;
        }

        private void OpenInstanceToGroupsConfigurator(InstanceModelBase instanceModelBase) 
        {
            var instancesFactoryModalViewModel = new InstanceGroupsConfiguratorViewModel(instanceModelBase, _clientsManager);
            _appCore.ModalNavigationStore.Open(instancesFactoryModalViewModel);
        }


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
