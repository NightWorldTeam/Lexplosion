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

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class LibraryViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenuLayoutCommand;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly Func<IEnumerable<InstanceModelBase>> _getInstances;

        public LibraryModel Model { get; }


        #region Commands


        private RelayCommand _openInstanceProfileMenu;
        public ICommand OpenInstanceProfileMenu
        {
            get => RelayCommand.GetCommand(ref _openInstanceProfileMenu, (obj) =>
            {
                var ins = (InstanceModelBase)obj;

                _navigationStore.CurrentViewModel = new InstanceProfileLayoutViewModel(_navigationStore, _toMainMenuLayoutCommand, ins);
            });
        }


        private RelayCommand _openInstanceFactory;
        public ICommand OpenInstanceFactory
        {
            get => RelayCommand.GetCommand(ref _openInstanceFactory, () => 
            {
                _modalNavigationStore.OpenModalPageByType(ModalAbstractFactory.ModalPage.InstanceFactory);
            });
        }


        #endregion Commands


        // TODO: думаю делегат с инстансами это костыль ченить другое надо придумать
        public LibraryViewModel(INavigationStore navigationStore, ICommand toMainMenuLayoutCommand, ModalNavigationStore modalNavigationStore, IInstanceController instanceController)
        {
            Model = new LibraryModel(instanceController);
            _navigationStore = navigationStore;
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
            _modalNavigationStore = modalNavigationStore;
        }
    }
}
