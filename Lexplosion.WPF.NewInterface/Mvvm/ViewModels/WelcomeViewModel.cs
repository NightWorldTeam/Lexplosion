using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
{
    public class WelcomeViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly ViewModelBase _authViewModel;

        public WelcomeViewModel(AppCore appCore, ViewModelBase authViewModel)
        {
            _appCore = appCore;
            _authViewModel = authViewModel;
        }

        #region Commands


        private RelayCommand _toThemeSelectCommand;
        public ICommand ToThemeSelectCommand 
        {
            get => RelayCommand.GetCommand(ref _toThemeSelectCommand, ToSelectTheme);
        }


        #endregion Commands

        private void ToAuthMenu() 
        {
            _appCore.NavigationStore.CurrentViewModel = _authViewModel;
        }

        private void ToSelectTheme() 
        {
            _appCore.NavigationStore.CurrentViewModel = new WelcomePageThemeSelectViewModel(_appCore, ToAuthMenu);
            OnPropertyChanged(nameof(_appCore.NavigationStore.CurrentViewModel));
        }
    }
}
