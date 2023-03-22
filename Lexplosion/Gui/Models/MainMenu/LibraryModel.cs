using Lexplosion.Gui.ViewModels;
using Lexplosion.Gui.ViewModels.FactoryMenu;
using Lexplosion.Gui.ViewModels.ModalVMs;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.MainMenu
{
    public class LibraryModel
    {
        public MainViewModel MainViewModel { get; }

        public LibraryModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        /// <summary>
        /// Закрывает все выпадающие меню у InstanceForm.
        /// !!! Скорость O(n) !!!
        /// </summary>
        public void CloseAllOpenedDropDownMenus()
        {
            foreach (var instance in MainViewModel.Model.LibraryInstances)
            {
                if (instance.IsDropdownMenuOpen)
                    instance.IsDropdownMenuOpen = false;
            }
        }

        /// <summary>
        /// Открывает модальное окно для создания, импорта сборок.
        /// </summary>
        public void OpenInstanceFactoryModalWindow()
        {
            var factory = new FactoryGeneralViewModel(MainViewModel);
            var import = new ImportViewModel(MainViewModel, factory);

            ModalWindowViewModelSingleton.Instance.Open(
                new CustomTabsMenuViewModel(
                    new List<CustomTab>()
                    {
                        new CustomTab(
                            "Create",
                            "M22.65 34h3v-8.3H34v-3h-8.35V14h-3v8.7H14v3h8.65ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 23.95q0-4.1 1.575-7.75 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24.05 4q4.1 0 7.75 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Zm.05-3q7.05 0 12-4.975T41 23.95q0-7.05-4.95-12T24 7q-7.05 0-12.025 4.95Q7 16.9 7 24q0 7.05 4.975 12.025Q16.95 41 24.05 41ZM24 24Z",
                            factory
                        ),
                        new CustomTab(
                            "Import",
                            "M14 40v-3h20v3Zm8.5-8.5V13.7l-6.05 6.05-2.1-2.1L24 8l9.65 9.65-2.1 2.1-6.05-6.05v17.8Z",
                            import,
                            true
                        )
                    }
                )
            );
        }
    }
}
