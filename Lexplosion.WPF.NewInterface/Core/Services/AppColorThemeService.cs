using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Media;
using Lexplosion.WPF.NewInterface.Core.Objects;

namespace Lexplosion.WPF.NewInterface.Core.Services
{
    public class AppColorThemeService
    {
        public event Action ColorThemeChanged;
        public event Action ActivityColorChanged;

        private ResourceDictionary _selectedThemeResourceDictionary;


        #region Properties

        public IEnumerable<Theme> LoadedThemes { get; } = new List<Theme>();

        public List<Action> Animations { get; } = new List<Action>();
        public List<Action> BeforeAnimations { get; } = new List<Action>();

        public Theme SelectedTheme { get; private set; }
        public ActivityColor SelectedActivityColor { get; private set; }


        #endregion Properties


        #region Public Methods


        public void ChangeActivityColor(Color color) 
        {
            if (_selectedThemeResourceDictionary != null)
            {
                _selectedThemeResourceDictionary["ActivityColor"] = color;
                _selectedThemeResourceDictionary["ActivitySolidColorBrush"] = new SolidColorBrush(color);
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

            App.Current.Resources.MergedDictionaries.Add(_selectedThemeResourceDictionary);

            SelectedTheme = theme;

            ColorThemeChanged?.Invoke();

            Animations.ForEach(item => item?.Invoke());
        }


        #endregion Public Methods
    }
}
