using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class Theme : ObservableObject
    {
        public const string DefaultThemesResourcePath = "pack://application:,,,/Resources/Themes/";

        public event Action<Theme, bool> SelectedEvent = null;

        private readonly ResourceDictionary _dictionary;


        #region Properties


        public string Name { get; private set; }
        public ResourceDictionary ResourceDictionary { get => _dictionary; }
        public Uri? DictionaryUri { get; private set; }

        // colors
        public SolidColorBrush PrimaryBrush { get; }
        public SolidColorBrush SecondaryBrush { get; }
        public SolidColorBrush SeparateBrush { get; }

        private SolidColorBrush _activityBrush = null;
        public SolidColorBrush ActivityBrush
        {
            get => _activityBrush; private set
            {
                _activityBrush = value;
                OnPropertyChanged();
            }
        }

        public bool IsPresetColor { get; private set; } = false;


        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnSelectedChanged(value);
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public Theme(string name, string fileName) : this(new() { Source = new Uri(DefaultThemesResourcePath + fileName) })
        {
            Name = name;
            DictionaryUri = new Uri(DefaultThemesResourcePath + fileName);

            _dictionary = new ResourceDictionary()
            {
                Source = DictionaryUri
            };
        }

        public Theme(ResourceDictionary dictionary)
        {
            _dictionary = dictionary;

            PrimaryBrush = (SolidColorBrush)dictionary["PrimarySolidColorBrush"];
            SecondaryBrush = (SolidColorBrush)dictionary["SecondarySolidColorBrush"];
            SeparateBrush = (SolidColorBrush)dictionary["SeparateSolidColorBrush"];

            if (!IsPresetColor)
            {
                ActivityBrush = (SolidColorBrush)dictionary["ActivitySolidColorBrush"];
            }
        }


        #endregion Constructors


        public void ChangeActivityBrush(SolidColorBrush brush)
        {
            if (IsPresetColor)
                return;

            ActivityBrush = brush;
        }


        #region Private Methods


        private void OnSelectedChanged(bool val)
        {
            SelectedEvent?.Invoke(this, val);
        }


        #endregion Private Methods
    }
}
