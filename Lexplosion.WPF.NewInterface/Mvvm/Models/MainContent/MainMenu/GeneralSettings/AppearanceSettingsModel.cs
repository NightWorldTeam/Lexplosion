using Lexplosion.Core.Extensions;
using Lexplosion.Global;
using Lexplosion.Logic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.Content.GeneralSettings
{
    public sealed class AppearanceSettingsModel : ViewModelBase
    {
        public Theme SelectedTheme { get => RuntimeApp.Settings.ThemeService.SelectedTheme; }
        public ActivityColor SelectedColor { get => RuntimeApp.Settings.ThemeService.SelectedActivityColor; }


        private ObservableCollection<ActivityColor> _colors = new ObservableCollection<ActivityColor>();
        public IEnumerable<ActivityColor> Colors { get => _colors; }


        private ObservableCollection<Theme> _themes = new ObservableCollection<Theme>();
        public IEnumerable<Theme> Themes { get => _themes; }


        private string _newHexActivityColor;
        public string NewHexActivityColor
        {
            get => _newHexActivityColor; set
            {
                _newHexActivityColor = value;

                if (ActivityColor.TryCreateColor(value, out var color))
                {
                    SelectedColorChanged(color, true);
                }
                else
                {
                    SelectedColorChanged(SelectedColor, true);
                }

                OnPropertyChanged();
            }
        }


        #region Tooltip


        private bool _isToolTipsEnabled;
        public bool IsToolTipsEnabled
        {
            get => _isToolTipsEnabled; set
            {
                _isToolTipsEnabled = value;
                OnPropertyChanged();
                OnToolTipStateChanged();
            }
        }


        private int _initialShowDelay;
        public int InitialShowDelay
        {
            get => _initialShowDelay; set
            {
                _initialShowDelay = value;
                OnPropertyChanged();
                OnInitialShowDelayChanged();
            }
        }

        private int _betweenShowDelay;
        public int BetweenShowDelay
        {
            get => _betweenShowDelay; set
            {
                _betweenShowDelay = value;
                OnPropertyChanged();
                OnBetweenShowDelayChanged();
            }
        }


        #endregion Tooltip

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

            RuntimeApp.Settings.ThemeService.Themes = _themes;

            foreach (var theme in _themes)
            {
                theme.SelectedEvent += SelectedThemeChanged;
                Runtime.DebugWrite(theme.Name + " >>> " + GlobalData.GeneralSettings.ThemeName, color: System.ConsoleColor.Red);
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
            _colors.Add(new ActivityColor("#000"));

            foreach (var color in _colors)
            {
                color.SelectedEvent += SelectedColorChanged;
            }

            var savedColorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(GlobalData.GeneralSettings.AccentColor);
            var savedColor = new ActivityColor(savedColorBrush);
            savedColor.SelectedEvent += SelectedColorChanged;
            if (savedColorBrush == null)
            {
                _colors[0].IsSelected = true;
            }
            else
            {
                savedColor.IsSelected = true;
            }

        }

        private void SelectedColorChanged(ActivityColor color, bool isSelected)
        {
            if (isSelected && color != null)
            {
                RuntimeApp.Settings.ThemeService.ChangeActivityColor(color.Brush.Color);
                GlobalData.GeneralSettings.AccentColor = color.Brush.Color.ToString();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        private void SelectedThemeChanged(Theme theme, bool isSelected)
        {
            if (isSelected)
            {
                RuntimeApp.Settings.ThemeService.ChangeTheme(theme);
                GlobalData.GeneralSettings.ThemeName = theme.Name;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }


        /// <summary>
        /// Изменяет состояние всплывающих подсказок
        /// </summary>
        private void OnToolTipStateChanged()
        {
            RuntimeApp.ChangeToolTipState(IsToolTipsEnabled);
        }


        private void OnInitialShowDelayChanged()
        {
            RuntimeApp.ChangeSettingInitialShowDelay(InitialShowDelay);
        }

        private void OnBetweenShowDelayChanged()
        {
            RuntimeApp.ChangeSettingBetweenShowDelay(BetweenShowDelay);
        }


        #endregion Private Methods
    }
}
