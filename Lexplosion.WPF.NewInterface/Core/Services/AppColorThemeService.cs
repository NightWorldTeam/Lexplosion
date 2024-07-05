using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Media;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Tools;

namespace Lexplosion.WPF.NewInterface.Core.Services
{
    public class AppColorThemeService
    {
        public event Action ColorThemeChanged;
        public event Action ActivityColorChanged;

        private ResourceDictionary _selectedThemeResourceDictionary;

        private Color _selectedActivityColor; 


        #region Properties


        public IEnumerable<Theme> LoadedThemes { get; } = new List<Theme>();

        public List<Action> Animations { get; } = new List<Action>();
        public List<Action> BeforeAnimations { get; } = new List<Action>();

        public Theme SelectedTheme { get; private set; }
        public ActivityColor SelectedActivityColor { get; private set; }


        #endregion Properties


        public AppColorThemeService()
        {
            
        }


        #region Public Methods


        public void ChangeActivityColor(Color color) 
        {
            if (_selectedThemeResourceDictionary != null)
            {
                _selectedThemeResourceDictionary["ActivityColor"] = color;
                _selectedThemeResourceDictionary["ActivitySolidColorBrush"] = new SolidColorBrush(color);

                _selectedActivityColor = color;




                ActivityColorChanged?.Invoke();
            }
        }

        public void ChangeTheme(Theme theme) 
        {
            var currentThemeName = string.Empty;
            var resourceDictionaries = new List<ResourceDictionary>();

            BeforeAnimations.ForEach(item => item?.Invoke());

            foreach (var s in App.Current.Resources.MergedDictionaries)
            {
                if (s.Source.ToString().Contains("ColorTheme"))
                {
                    currentThemeName = s.Source.ToString();
                    resourceDictionaries.Add(s);
                }
            }

            foreach (var s in resourceDictionaries)
            {
                App.Current.Resources.MergedDictionaries.Remove(s);
            }

            _selectedThemeResourceDictionary = new ResourceDictionary()
            {
                Source = theme.DictionaryUri
            };

            if (_selectedActivityColor == default)
                _selectedActivityColor = (Color)_selectedThemeResourceDictionary["ActivityColor"];
            else
                _selectedThemeResourceDictionary["ActivityColor"] = _selectedActivityColor;

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
            //*** Default Button Background ***//

            _selectedThemeResourceDictionary["DefaultButtonBackgroundColor"] = color;
            _selectedThemeResourceDictionary["DefaultButtonBackgroundColorBrush"] = new SolidColorBrush(color);

            //*** Default Button Hover Background ***//

            _selectedThemeResourceDictionary["DefaultButtonHoverBackgroundColor"] = ColorTools.GetDarkerColor(color, 10);
            _selectedThemeResourceDictionary["DefaultButtonHoverBackgroundColorBrush"] = new SolidColorBrush(color);

            //*** Default Button Pressed Background ***//

            _selectedThemeResourceDictionary["DefaultButtonPressedBackgroundColor"] = color;
            _selectedThemeResourceDictionary["DefaultButtonPressedBackgroundColorBrush"] = new SolidColorBrush(color);

            //*** Default Button Disable Background ***//

            _selectedThemeResourceDictionary["DefaultButtonDisableBackgroundColor"] = color;
            _selectedThemeResourceDictionary["DefaultButtonDisableBackgroundColorBrush"] = new SolidColorBrush(color);

            /*** Default Button Foreground Color ***/

            _selectedThemeResourceDictionary["DefaultButtonForegroundColor"] = color;
            _selectedThemeResourceDictionary["DefaultButtonForegroundColorBrush"] = new SolidColorBrush(color);
        }
    }
}
