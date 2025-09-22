using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using System.Collections.ObjectModel;
using System;
using System.Windows.Input;
using System.Collections.Generic;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels
{
    public sealed class WelcomePageThemeSelectViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly Action _navigateToAuth;


        public IEnumerable<Theme> Themes { get; }


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

            Themes = appCore.Settings.ThemeService.Themes;
        }

        private void SelectedThemeChanged(Theme theme, bool isSelected)
        {
            if (isSelected)
            {
                _appCore.Settings.ThemeService.ChangeTheme(theme, true);
                GlobalData.GeneralSettings.ThemeName = theme.Name;
				Runtime.ServicesContainer.DataFilesService.SaveSettings(GlobalData.GeneralSettings);
            }
        }
    }
}
