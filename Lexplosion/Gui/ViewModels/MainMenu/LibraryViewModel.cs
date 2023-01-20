using Lexplosion.Gui.Models.MainMenu;

namespace Lexplosion.Gui.ViewModels.MainMenu
{
    public sealed class LibraryViewModel : VMBase
    {
        public LibraryModel Model { get; }

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

        public LibraryViewModel(MainViewModel mainViewModel)
        {
            Model = new LibraryModel(mainViewModel);
        }
    }
}
