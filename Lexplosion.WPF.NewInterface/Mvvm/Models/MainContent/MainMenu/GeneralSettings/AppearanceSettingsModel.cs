using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class AppearanceSettingsModel : ViewModelBase
    {
        public Theme SelectedTheme { get; set; }
        public ActivityColor SelectedColor { get; private set; }


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
        }

        private void LoadActivityColors()
        {
            _colors.Add(new ActivityColor("#167ffc"));
            _colors.Add(new ActivityColor("#A020F0"));
            _colors.Add(new ActivityColor("#FFE600"));
            _colors.Add(new ActivityColor("#40A710"));

            foreach (var color in _colors)
            {
                color.SelectedEvent += SelectedColorChanged;
            }
        }

        private void SelectedColorChanged(ActivityColor color)
        {
            SelectedColor = color;
        }

        private void SelectedThemeChanged(Theme theme)
        {
            SelectedTheme = theme;
        }


        #endregion Private Methods
    }
}
