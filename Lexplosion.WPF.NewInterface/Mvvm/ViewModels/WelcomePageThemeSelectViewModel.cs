using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.ObjectModel;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels
{
    public sealed class WelcomePageThemeSelectViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly Action _navigateToAuth;


        public ObservableCollection<Theme> Themes { get; } = [];


        #region Commands


        private RelayCommand _toAuthCommand;
        public ICommand ToAuthCommand 
        {
            get => RelayCommand.GetCommand(ref _toAuthCommand, _navigateToAuth);
        }


        #endregion Commands


        public WelcomePageThemeSelectViewModel(AppCore appCore, Action navigate)
        {
            _appCore = appCore;
            _navigateToAuth = navigate;

            Themes.Add(new Theme("Light Punch", "LightColorTheme.xaml"));
            Themes.Add(new Theme("Open Space", "DarkColorTheme.xaml"));

            foreach (var theme in Themes)
            {
                theme.SelectedEvent += SelectedThemeChanged;
            }

            Themes[0].IsSelected = true;
        }

        private void SelectedThemeChanged(Theme theme, bool isSelected)
        {
            if (isSelected)
            {
                RuntimeApp.Settings.ThemeService.ChangeTheme(theme, false);
                GlobalData.GeneralSettings.ThemeName = theme.Name;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }
    }
}
