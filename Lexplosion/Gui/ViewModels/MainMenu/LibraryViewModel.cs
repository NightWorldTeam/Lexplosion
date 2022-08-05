using Lexplosion.Gui.ViewModels.FactoryMenu;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public class LibraryViewModel : VMBase
    {
        private MainViewModel _mainViewModel;
        public MainViewModel MainVM 
        {
            get => _mainViewModel;
        }

        private RelayCommand _openInstanceFactoryCommand;
        public RelayCommand OpenInstanceFactoryCommand 
        {
            get => _openInstanceFactoryCommand ?? new RelayCommand(obj => 
            {
                _mainViewModel.ModalWindowVM.IsModalOpen = true;
                _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(new FactoryGeneralViewModel(_mainViewModel));
            });
        }

        public LibraryViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
    }
}
