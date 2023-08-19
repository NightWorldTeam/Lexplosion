using Lexplosion.Common.Models.MainMenu;
using Lexplosion.Common.ViewModels.ModalVMs;

namespace Lexplosion.Common.ViewModels.MainMenu
{
    public sealed class LibraryViewModel : VMBase
    {
        public LibraryModel Model { get; }


        #region Commands


        private RelayCommand _onScrollCommand;
        public RelayCommand OnScrollCommand
        {
            get => _onScrollCommand ?? (_onScrollCommand = new RelayCommand(obj =>
            {
                Model.CloseAllOpenedDropDownMenus();
            }));
        }

        private RelayCommand _openInstanceFactoryCommand;
        public RelayCommand OpenInstanceFactoryCommand
        {
            get => _openInstanceFactoryCommand ?? (_openInstanceFactoryCommand = new RelayCommand(obj =>
            {
                Model.OpenInstanceFactoryModalWindow();
            }));
        }

        private RelayCommand _openGroupsManagerCommand;
        public RelayCommand OpenGroupsManagerCommand
        {
            get => _openGroupsManagerCommand ?? (_openGroupsManagerCommand = new RelayCommand(obj =>
            {
                ModalWindowViewModelSingleton.Instance.Open(new InstanceGroupsManagerViewModel());
            }));
        }

        #endregion Commands


        #region Constructors


        public LibraryViewModel(MainViewModel mainViewModel)
        {
            Model = new LibraryModel(mainViewModel);
        }


        #endregion Constructors
    }
}
