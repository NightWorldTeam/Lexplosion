using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public struct Theme : INotifyPropertyChanged
    {
        public const string ThemesResourcePath = "pack://application:,,,/Resources/Themes/";

        public event Action<Theme> SelectedEvent;
        public event PropertyChangedEventHandler PropertyChanged;


        #region Properties


        private ResourceDictionary Dictionary { get; }

        public string Name { get; private set; }

        // colors
        public SolidColorBrush PrimaryBrush { get; }
        public SolidColorBrush SecondaryBrush { get; }
        public SolidColorBrush SeparateBrush { get; }

        private SolidColorBrush _activityBrush;
        public SolidColorBrush ActivityBrush
        {
            get => _activityBrush; private set
            {
                _activityBrush = value;
                OnPropertyChanged();
            }
        }

        public bool IsPresetColor { get; private set; } = false;


        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnSelectedChanged();
            }
        }


        #endregion  Properties


        #region Constructors


        public Theme(string name, string fileName)
        {
            Name = name;
            Dictionary = new ResourceDictionary()
            {
                Source = new Uri(ThemesResourcePath + fileName),
            };

            PrimaryBrush = (SolidColorBrush)Dictionary["PrimarySolidColorBrush"];
            SecondaryBrush = (SolidColorBrush)Dictionary["SecondarySolidColorBrush"];
            SeparateBrush = (SolidColorBrush)Dictionary["SeparateSolidColorBrush"];

            if (!IsPresetColor)
            {
                ActivityBrush = (SolidColorBrush)Dictionary["ActivitySolidColorBrush"];
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


        private void OnSelectedChanged()
        {
            SelectedEvent?.Invoke(this);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion Private Methods
    }
}
