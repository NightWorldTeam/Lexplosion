using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using System;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels
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


        public void ToDarkTheme()
        {
            var themeService = _appCore.Settings.ThemeService;

            themeService.Themes.First().IsSelected = false;
            var darkTheme = themeService.Themes.Single(x => x.Name == "Open Space");
            darkTheme.HasChangeAnimation = false;
            themeService.ChangeTheme(darkTheme, true, ["welcome-page"], () =>
            {
                darkTheme.IsSelected = true;
                darkTheme.HasChangeAnimation = true;
            });
        }


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
