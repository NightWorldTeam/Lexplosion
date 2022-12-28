using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using System.Collections.Generic;

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

                var factory = new FactoryGeneralViewModel(MainVM);

                _mainViewModel.ModalWindowVM.ChangeCurrentModalContent(new CustomTabsMenuViewModel(
                new List<CustomTab>()
                {
                    //M7 42q-1.2 0-2.1-.9Q4 40.2 4 39V28.25q0-1.2.9-2.1.9-.9 2.1-.9h10.55q0 2.7 1.875 4.525Q21.3 31.6 24 31.6t4.575-1.825q1.875-1.825 1.875-4.525H41q1.2 0 2.1.9.9.9.9 2.1V39q0 1.2-.9 2.1-.9.9-2.1.9Zm0-3h34V28.25h-8.2q-.9 2.85-3.275 4.6Q27.15 34.6 24 34.6q-3.15 0-5.625-1.75t-3.175-4.6H7V39Zm28.65-17.75L33.5 19.1l6.75-6.75 2.15 2.15Zm-23.3 0L5.6 14.5l2.15-2.15 6.75 6.75Zm10.15-5.9V6h3v9.35ZM7 39h34Z
                    new CustomTab(
                        "Create",
                        "M22.65 34h3v-8.3H34v-3h-8.35V14h-3v8.7H14v3h8.65ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 23.95q0-4.1 1.575-7.75 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24.05 4q4.1 0 7.75 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Zm.05-3q7.05 0 12-4.975T41 23.95q0-7.05-4.95-12T24 7q-7.05 0-12.025 4.95Q7 16.9 7 24q0 7.05 4.975 12.025Q16.95 41 24.05 41ZM24 24Z",
                        factory
                        ),
                    new CustomTab(
                        "Import",
                        "M14 40v-3h20v3Zm8.5-8.5V13.7l-6.05 6.05-2.1-2.1L24 8l9.65 9.65-2.1 2.1-6.05-6.05v17.8Z",
                        new ImportViewModel(MainVM, factory), true
                        )
                }
                ));
            }));
        }

        public LibraryViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
    }
}
