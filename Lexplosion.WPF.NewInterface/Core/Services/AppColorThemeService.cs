using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Media;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Tools;
using Lexplosion.WPF.NewInterface.Extensions;
using System.Linq;
using Lexplosion.WPF.NewInterface.NWColorTools;
using System.Threading;
using Lexplosion.Global;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.ObjectModel;
using Lexplosion.Logic.FileSystem;

namespace Lexplosion.WPF.NewInterface.Core.Services
{
    public class AppColorThemeService : ObservableObject
    {
        public event Action ColorThemeChanged;
        public event Action ActivityColorChanged;

        private ResourceDictionary _selectedThemeResourceDictionary;
        private Color _selectedActivityColor;


        public Dictionary<string, Action<Action>> AnimationsList = [];
        public Dictionary<string, Action> BeforeAnimationsList = [];


        #region Properties

        public List<Action> Animations { get; } = new List<Action>();
        public List<Action> BeforeAnimations { get; } = new List<Action>();

        private ObservableCollection<Theme> _themes = [];
        public IEnumerable<Theme> Themes { get => _themes; set => _themes = new ObservableCollection<Theme>(value); }

        private ObservableCollection<ActivityColor> _colors = new ObservableCollection<ActivityColor>();
        public IEnumerable<ActivityColor> Colors { get => _colors; }

        public Theme SelectedTheme { get; private set; }
        public ActivityColor SelectedActivityColor { get; private set; }



        #endregion Properties


        public AppColorThemeService()
        {
            LoadDefaultTheme();
            LoadActivityColors();
        }


        #region Public Methods


        public void LoadDefaultTheme()
        {
            _themes.Add(new Theme("Light Punch", "LightColorTheme.xaml"));
            _themes.Add(new Theme("Open Space", "DarkColorTheme.xaml"));

            foreach (var theme in _themes)
            {
                theme.SelectedEvent += SelectedThemeChanged;
                //Runtime.DebugWrite(theme.Name + " >>> " + GlobalData.GeneralSettings.ThemeName, color: System.ConsoleColor.Red);
            }

            var savedTheme = _themes.FirstOrDefault(t => t.Name == GlobalData.GeneralSettings.ThemeName);

            if (savedTheme == null)
                _themes[0].IsSelected = true;
            else
                savedTheme.IsSelected = true;

        }

        public void LoadActivityColors()
        {
            _colors.Add(new ActivityColor("#167ffc"));
            _colors.Add(new ActivityColor("#A020F0"));

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

        public void ChangeActivityColor(Color color, bool animatedChanging = false)
        {
            if (_selectedThemeResourceDictionary == null)
            {
                return;
            }

            if (animatedChanging)
            {
                Runtime.TaskRun(() =>
                {
                    var currentActivityColor = (Color)_selectedThemeResourceDictionary["ActivityColor"];
                    var intervalColors = Gradient.GenerateGradient(currentActivityColor, color, 50);

                    foreach (var gradColor in intervalColors)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            _selectedThemeResourceDictionary["ActivityColor"] = gradColor;
                            _selectedThemeResourceDictionary["ActivitySolidColorBrush"] = new SolidColorBrush(gradColor);
                        });

                        Thread.Sleep(2);
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _selectedThemeResourceDictionary["ActivityColor"] = color;
                        _selectedThemeResourceDictionary["ActivitySolidColorBrush"] = new SolidColorBrush(color);
                        ActivityColorChanged?.Invoke();
                        _selectedActivityColor = color;
                        SelectedActivityColor = new(_selectedActivityColor, true);
                        OnPropertyChanged(nameof(SelectedActivityColor));

                        ChangeDefaultAdvancedButtonColors(color);
                    });
                });
            }
            else
            {
                _selectedThemeResourceDictionary["ActivityColor"] = color;
                _selectedThemeResourceDictionary["ActivitySolidColorBrush"] = new SolidColorBrush(color);
                ActivityColorChanged?.Invoke();
                //_selectedActivityColor = color;

                ChangeDefaultAdvancedButtonColors(color);
            }
        }

        public void AddTheme(ResourceDictionary resourceDictionary)
        {
            var theme = new Theme(resourceDictionary);
            _themes.Add(theme);
        }

        public void AddAndActiveTheme(ResourceDictionary resourceDictionary)
        {
            AddTheme(resourceDictionary);
            ChangeTheme(_themes[_themes.Count - 1]);
        }

        public void ChangeTheme(Theme theme, bool playAnimations = false, string[] animationsKeys = null, Action afterAnimation = null)
        {
            var currentThemeName = string.Empty;
            var resourceDictionaries = new List<ResourceDictionary>();

            if (playAnimations)
            {
                if (animationsKeys == null)
                    BeforeAnimations.ForEach(item => item?.Invoke());
                else
                {
                    foreach (var key in animationsKeys)
                    {
                        if (BeforeAnimationsList.TryGetValue(key, out var action))
                        {
                            action();
                        }
                    }
                }
            }

            foreach (var md in App.Current.Resources.MergedDictionaries)
            {
                if (md.TryGetValue<string, string>("type", out var result))
                {
                    if (result.Contains("_ColorTheme"))
                    {
                        currentThemeName = result;
                        resourceDictionaries.Add(md);
                    }
                }
            }

            foreach (var s in resourceDictionaries)
            {
                App.Current.Resources.MergedDictionaries.Remove(s);
            }

            _selectedThemeResourceDictionary = theme.ResourceDictionary;

            if (_selectedActivityColor == default)
                _selectedActivityColor = (Color)_selectedThemeResourceDictionary["ActivityColor"];
            else
                _selectedThemeResourceDictionary["ActivityColor"] = _selectedActivityColor;

            SelectedActivityColor = new(_selectedActivityColor, true);
            OnPropertyChanged(nameof(SelectedActivityColor));

            App.Current.Resources.MergedDictionaries.Add(_selectedThemeResourceDictionary);

            SelectedTheme = theme;

            ColorThemeChanged?.Invoke();

            // TODO: При быстрых вызовах смены темы, будет происходить утечка памяти июо circle animation создает brush в виде image картинки.
            if (playAnimations)
            {
                if (animationsKeys == null)
                {
                    Animations.ForEach(item => item?.Invoke());
                    return;
                }

                // вызываем кастомные анимации
                foreach (var key in animationsKeys)
                {
                    if (AnimationsList.TryGetValue(key, out var startAnimation))
                        startAnimation(afterAnimation);
                }
            }
        }


        public void SelectedColorChanged(ActivityColor color, bool isSelected)
        {
            if (isSelected && color != null)
            {
                ChangeActivityColor(color.Brush.Color);
                GlobalData.GeneralSettings.AccentColor = color.Brush.Color.ToString();
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }

        public void SelectedThemeChanged(Theme theme, bool isSelected)
        {
            if (isSelected)
            {
                ChangeTheme(theme);
                GlobalData.GeneralSettings.ThemeName = theme.Name;
                DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
            }
        }


        #endregion Public Methods


        /// <summary>
        /// Изменяет цвет у всех advanced button со стандартным стилем.
        /// </summary>
        /// <param name="color">Фон кнопки</param>
        private void ChangeDefaultAdvancedButtonColors(Color color)
        {
            var hoverColor = Color.FromRgb(51, 51, 51);
            var pressedColor = Color.FromRgb(77, 77, 77);
            var disabledColor = Color.FromRgb(179, 179, 179);

            if (color != System.Windows.Media.Colors.Black)
            {
                var luminance = ColorTools.CalculateLuminance(color);
                Func<Color, float, Color> colorSelectorByLuminance = luminance < 140 ? ColorTools.GetLighterColor : ColorTools.GetDarkerColor;

                hoverColor = colorSelectorByLuminance(color, 10);
                pressedColor = colorSelectorByLuminance(color, 20);
                disabledColor = colorSelectorByLuminance(color, -70);
            }

            ////*** Default Button Background ***//

            _selectedThemeResourceDictionary["DefaultButtonBackgroundColor"] = color;
            _selectedThemeResourceDictionary["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(color);

            ////*** Default Button Hover Background ***//
            _selectedThemeResourceDictionary["DefaultButtonHoverBackgroundColor"] = hoverColor;
            _selectedThemeResourceDictionary["DefaultButtonHoverBackgroundColorBrush"] = new SolidColorBrush(hoverColor);

            //*** Default Button Pressed Background ***//
            _selectedThemeResourceDictionary["DefaultButtonPressedBackgroundColor"] = pressedColor;
            _selectedThemeResourceDictionary["DefaultButtonPressedBackgroundColorBrush"] = new SolidColorBrush(pressedColor);

            ////*** Default Button Disable Background ***//
            _selectedThemeResourceDictionary["DefaultButtonDisableBackgroundColor"] = disabledColor;
            _selectedThemeResourceDictionary["DefaultButtonDisableBackgroundColorBrush"] = new SolidColorBrush(disabledColor);

            ///*** Default Button Foreground Color ***/
            var foregroundColor = ColorTools.ForegroundByColor(color);
            _selectedThemeResourceDictionary["DefaultButtonForegroundColor"] = foregroundColor;
            _selectedThemeResourceDictionary["DefaultButtonForegroundColorBrush"] = new SolidColorBrush(foregroundColor);

            _selectedThemeResourceDictionary["BrandColorForegroundColor"] = foregroundColor;
            _selectedThemeResourceDictionary["BrandColorForegroundSolidColorBrush"] = new SolidColorBrush(foregroundColor);


        }
    }
}
