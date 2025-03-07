using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
{
    public class WelcomeViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly Action _navigate;

        public WelcomeViewModel(AppCore appCore, Action navigate)
        {
            _appCore = appCore;
            _navigate = navigate;
        }

        #region Commands


        private RelayCommand _toThemeSelectCommand;
        public ICommand ToThemeSelectCommand 
        {
            get => RelayCommand.GetCommand(ref _toThemeSelectCommand, ToSelectTheme);
        }


        #endregion Commands


        private void ToSelectTheme() 
        {
            _appCore.NavigationStore.CurrentViewModel = new WelcomePageThemeSelectViewModel(_appCore, _navigate);
            OnPropertyChanged(nameof(_appCore.NavigationStore.CurrentViewModel));
        }
    }
}
