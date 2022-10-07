using Lexplosion.Gui.ViewModels.FactoryMenu;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class LibraryViewModel : VMBase
    {
        private MainViewModel _mainViewModel;
        public MainViewModel MainVM
        {
            get => _mainViewModel;
        }

        private RelayCommand _onScrollCommand;
        public RelayCommand OnScrollCommand
        {
            get => _onScrollCommand ?? (_onScrollCommand = new RelayCommand(obj =>
            {
                // TODO: Возможно тяжелый код.
                foreach (var instance in _mainViewModel.Model.LibraryInstances)
                {
                    if (instance.IsDropdownMenuOpen)
                        instance.IsDropdownMenuOpen = false;
                }
            }));
        }

        private RelayCommand _openInstanceFactoryCommand;
        public RelayCommand OpenInstanceFactoryCommand
        {
            get => _openInstanceFactoryCommand ?? (_openInstanceFactoryCommand = new RelayCommand(obj =>
            {
                _mainViewModel.ModalWindowVM.IsOpen = true;
                _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(new FactoryGeneralViewModel(_mainViewModel));
            }));
        }

        public LibraryViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
    }
}
