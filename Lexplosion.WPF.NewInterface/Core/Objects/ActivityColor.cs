using System;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public struct ActivityColor
    {
        public event Action<ActivityColor> SelectedEvent;

        public SolidColorBrush Brush { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnIsSelectedChanged();
            }
        }

        public ActivityColor(SolidColorBrush brush, bool isSelected = false)
        {
            Brush = brush;
            IsSelected = IsSelected;
        }

        public ActivityColor(string hexColor, bool isSelected = false)
        {
            Brush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
            IsSelected = isSelected;
        }

        private void OnIsSelectedChanged()
        {
            SelectedEvent?.Invoke(this);
        }
    }
}
