using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class AppearanceSettingsModel : ViewModelBase
    {
        public Theme SelectedTheme { get => RuntimeApp.AppColorThemeService.SelectedTheme; }
        public ActivityColor SelectedColor { get => RuntimeApp.AppColorThemeService.SelectedActivityColor; }


        private ObservableCollection<ActivityColor> _colors = new ObservableCollection<ActivityColor>();
        public IEnumerable<ActivityColor> Colors { get => _colors; }


        private ObservableCollection<Theme> _themes = new ObservableCollection<Theme>();
        public IEnumerable<Theme> Themes { get => _themes; }


        public AppearanceSettingsModel()
        {
            LoadThemes();
            LoadActivityColors();
        }


        #region Private Methods


        private void LoadThemes()
        {
            _themes.Add(new Theme("Light Punch", "LightColorTheme.xaml"));
            _themes.Add(new Theme("Open Space", "DarkColorTheme.xaml"));

            RuntimeApp.AppColorThemeService.Themes = _themes;

            foreach (var theme in _themes)
            {
                theme.SelectedEvent += SelectedThemeChanged;
            }

            var savedTheme = _themes.FirstOrDefault(t => t.Name == GlobalData.GeneralSettings.ThemeName);
            
            if (savedTheme == null)
                _themes[0].IsSelected = true;
            else
                savedTheme.IsSelected = true;

        }

        private void LoadActivityColors()
        {
            _colors.Add(new ActivityColor("#167ffc"));
            _colors.Add(new ActivityColor("#A020F0"));
            _colors.Add(new ActivityColor("#FFE600"));
            _colors.Add(new ActivityColor("#40A710"));
            _colors.Add(new ActivityColor("#FF0000"));

            foreach (var color in _colors)
            {
                color.SelectedEvent += SelectedColorChanged;
            }

            var savedColorBrush = new BrushConverter().ConvertFrom(GlobalData.GeneralSettings.AccentColor);
            var savedColor = _colors.FirstOrDefault(c => c.Brush.ToString() == savedColorBrush.ToString());
            if (savedColor == null)
                _colors[0].IsSelected = true;
            else
                savedColor.IsSelected = true;

        }

        private void SelectedColorChanged(ActivityColor color, bool isSelected)
        {
            if (isSelected)
            {
                RuntimeApp.AppColorThemeService.ChangeActivityColor(color.Brush.Color);
                GlobalData.GeneralSettings.AccentColor = color.Brush.Color.ToString();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        private void SelectedThemeChanged(Theme theme, bool isSelected)
        {
            if (isSelected) 
            {
                RuntimeApp.AppColorThemeService.ChangeTheme(theme);
                GlobalData.GeneralSettings.ThemeName = theme.Name;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }


        #endregion Private Methods
    }
}
