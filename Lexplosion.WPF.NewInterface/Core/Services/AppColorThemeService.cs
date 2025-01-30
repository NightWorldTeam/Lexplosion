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

namespace Lexplosion.WPF.NewInterface.Core.Services
{
    public class AppColorThemeService : ObservableObject
    {
        public event Action ColorThemeChanged;
        public event Action ActivityColorChanged;

        private ResourceDictionary _selectedThemeResourceDictionary;
        private Color _selectedActivityColor;


        #region Properties


        private List<Theme> _themes = [];
        public IEnumerable<Theme> Themes { get => _themes; set => _themes = value.ToList(); }

        public List<Action> Animations { get; } = new List<Action>();
        public List<Action> BeforeAnimations { get; } = new List<Action>();

        public Theme SelectedTheme { get; private set; }
        public ActivityColor SelectedActivityColor { get; private set; }


        #endregion Properties


        public AppColorThemeService()
        {
            
        }


        #region Public Methods


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

        public void ChangeTheme(Theme theme)
        {
            var currentThemeName = string.Empty;
            var resourceDictionaries = new List<ResourceDictionary>();

            BeforeAnimations.ForEach(item => item?.Invoke());

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
            Animations.ForEach(item => item?.Invoke());
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

            if (color != Colors.Black)
            {
                var luminance = ColorTools.CalculateLuminance(color);
                Func<Color, float, Color> colorSelectorByLuminance = luminance < 140 ? ColorTools.GetLighterColor : ColorTools.GetDarkerColor;

                hoverColor = colorSelectorByLuminance(color, 10);
                pressedColor = colorSelectorByLuminance(color, 20);
                disabledColor = colorSelectorByLuminance(Colors.Gray, 50);
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


        }
    }
}
