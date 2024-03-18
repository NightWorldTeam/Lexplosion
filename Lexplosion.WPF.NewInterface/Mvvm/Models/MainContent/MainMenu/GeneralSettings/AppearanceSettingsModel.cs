using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            LoadActivityColors();
            LoadThemes();
        }


        #region Private Methods


        private void LoadThemes()
        {
            _themes.Add(new Theme("Light Punch", "LightColorTheme.xaml"));
            _themes.Add(new Theme("Open Space", "DarkColorTheme.xaml"));

            foreach (var theme in _themes)
            {
                theme.SelectedEvent += SelectedThemeChanged;
            }

            _themes[0].IsSelected = true;
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
        }

        private void SelectedColorChanged(ActivityColor color, bool isSelected)
        {
            if (isSelected)
            {
                RuntimeApp.AppColorThemeService.ChangeActivityColor(color.Brush.Color);
            }
        }

        private void SelectedThemeChanged(Theme theme, bool isSelected)
        {
            if (isSelected) 
            {
                RuntimeApp.AppColorThemeService.ChangeTheme(theme);
            }
        }


        #endregion Private Methods
    }
}
