using Lexplosion.Common.ViewModels;
using Lexplosion.Common.ViewModels.FactoryMenu;
using Lexplosion.Common.ViewModels.ModalVMs;
using Lexplosion.Global;
using Lexplosion.Tools;
using System.Collections.Generic;

namespace Lexplosion.Common.Models.MainMenu
{
    public sealed class LibraryModel
    {
        #region Properties


        public MainViewModel MainViewModel { get; }
        public IEnumerable<InstanceFormViewModel> Instances { get => MainModel.Instance.LibraryController.Instances; }


        #endregion Properties


        #region Construcotors


        public LibraryModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }


        #endregion Construcotors


        #region Public & Protected Method


        /// <summary>
        /// Закрывает все выпадающие меню у InstanceForm.
        /// !!! Скорость O(n) !!!
        /// </summary>
        public void CloseAllOpenedDropDownMenus()
        {
            foreach (var instance in MainModel.Instance.LibraryController.Instances)
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
            FactoryGeneralViewModel factory = null;
            ImportViewModel import;
            InstanceSharingListViewModel shares = null;

            if (GlobalData.User.AccountType != AccountType.NoAuth) 
            { 
                factory = new FactoryGeneralViewModel(MainViewModel);
                shares = new InstanceSharingListViewModel(MainViewModel.ShowToastMessage);
            }
            import = new ImportViewModel(MainViewModel.ShowToastMessage);
            
            ModalWindowViewModelSingleton.Instance.Open(
                new CustomTabsMenuViewModel(
                    new List<CustomTab>()
                    {
                        new CustomTab(
                            ResourceGetter.GetString("creation"),
                            "M22.65 34h3v-8.3H34v-3h-8.35V14h-3v8.7H14v3h8.65ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 23.95q0-4.1 1.575-7.75 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24.05 4q4.1 0 7.75 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Zm.05-3q7.05 0 12-4.975T41 23.95q0-7.05-4.95-12T24 7q-7.05 0-12.025 4.95Q7 16.9 7 24q0 7.05 4.975 12.025Q16.95 41 24.05 41ZM24 24Z",
                            factory, GlobalData.User.AccountType != AccountType.NoAuth
                        ),
                        new CustomTab(
                            ResourceGetter.GetString("import"),
                            "M14 40v-3h20v3Zm8.5-8.5V13.7l-6.05 6.05-2.1-2.1L24 8l9.65 9.65-2.1 2.1-6.05-6.05v17.8Z",
                            import
                        ),
                        new CustomTab(
                            ResourceGetter.GetString("sharingList"),
                            "M80 856v-60h400v60H80Zm0-210v-60h200v60H80Zm0-210v-60h200v60H80Zm758 420L678 696q-26 20-56 30t-62 10q-83 0-141.5-58.5T360 536q0-83 58.5-141.5T560 336q83 0 141.5 58.5T760 536q0 32-10 62t-30 56l160 160-42 42ZM559.765 676Q618 676 659 635.235q41-40.764 41-99Q700 478 659.235 437q-40.764-41-99-41Q502 396 461 436.765q-41 40.764-41 99Q420 594 460.765 635q40.764 41 99 41Z",
                            shares,
                            Global.GlobalData.User.AccountType == AccountType.NightWorld
                            )
                    },
                    GlobalData.User.AccountType != AccountType.NoAuth ? 0 : 1
                )
            );
        }


        #endregion Public & Protected Method
    }
}
