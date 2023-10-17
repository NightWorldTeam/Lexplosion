using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.Models.MainContent;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public sealed class LibraryViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly NavigateCommand<ViewModelBase> _toMainMenuLayoutCommand;

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


        #endregion Commands


        public LibraryViewModel(INavigationStore navigationStore, NavigateCommand<ViewModelBase> toMainMenuLayoutCommand)
        {
            Model = new LibraryModel();
            _navigationStore = navigationStore;
            _toMainMenuLayoutCommand = toMainMenuLayoutCommand;
        }
    }
}
