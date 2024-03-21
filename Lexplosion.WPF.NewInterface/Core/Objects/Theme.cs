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
        public const string ThemesResourcePath = "pack://application:,,,/Resources/Themes/";

        public event Action<Theme, bool> SelectedEvent = null;


        #region Properties


        private ResourceDictionary _dictionary;

        public string Name { get; private set; }
        public Uri DictionaryUri { get; private set; }

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


        public Theme(string name, string fileName)
        {
            Name = name;
            _dictionary = new ResourceDictionary()
            {
                Source = new Uri(ThemesResourcePath + fileName),
            };

            PrimaryBrush = (SolidColorBrush)_dictionary["PrimarySolidColorBrush"];
            SecondaryBrush = (SolidColorBrush)_dictionary["SecondarySolidColorBrush"];
            SeparateBrush = (SolidColorBrush)_dictionary["SeparateSolidColorBrush"];

            if (!IsPresetColor)
            {
                ActivityBrush = (SolidColorBrush)_dictionary["ActivitySolidColorBrush"];
            }

            DictionaryUri = new Uri(ThemesResourcePath + fileName);
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
