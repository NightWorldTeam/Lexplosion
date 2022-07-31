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

        public LibraryViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
    }
}
