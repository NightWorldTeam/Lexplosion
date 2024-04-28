using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceAddonsContainerViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly InstanceModelBase _instanceModelBase;

        public InstanceAddonsContainerModel Model { get; private set; }


        #region Commands


        // TODO: Rename to Repository
        private RelayCommand _openMarketCommand;
        public ICommand OpenMarketCommand
        {
            get => RelayCommand.GetCommand(ref _openMarketCommand, (obj) =>
            {
                var currentViewModel = _navigationStore.CurrentViewModel;
                var backNavCommand = new NavigateCommand<ViewModelBase>(_navigationStore, () => currentViewModel);
                _navigationStore.CurrentViewModel = new ModrinthRepositoryViewModel(_instanceModelBase, Model.Type, backNavCommand, _navigationStore);
            });
        }

        private RelayCommand _reloadCommand;
        public ICommand ReloadCommand
        {
            get => RelayCommand.GetCommand(ref _reloadCommand, (obj) => { });
        }

        private RelayCommand _openFolderCommand;
        public ICommand OpenFolderCommand
        {
            get => RelayCommand.GetCommand(ref _openFolderCommand, _instanceModelBase.OpenFolder);
        }

        private RelayCommand _updateCommand;
        public ICommand UpdateCommand
        {
            get => RelayCommand.GetCommand(ref _updateCommand, Model.UpdateAddon);
        }

        private RelayCommand _uninstallCommand;
        public ICommand UninstallCommand
        {
            get => RelayCommand.GetCommand(ref _uninstallCommand, (obj) =>
            {
                var dialogViewModel = new DialogBoxViewModel("delete", "delete",
                (obj) =>
                {
                    Model.UninstallAddon(obj);
                }, (obj) => { //ModalNavigationStore.Close();
                });
                //ModalNavigationStore.Instance.Open(dialogViewModel);
            });
        }


        #endregion Commands


        public InstanceAddonsContainerViewModel(INavigationStore navigationStore, AddonType addonType, InstanceModelBase instanceModelBase)
        {
            _navigationStore = navigationStore;
            _instanceModelBase = instanceModelBase;
            Model = new InstanceAddonsContainerModel(addonType, instanceModelBase);
        }
    }
}
