using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class ActivityColor : ObservableObject
    {
        public event Action<ActivityColor, bool> SelectedEvent = null;

        public SolidColorBrush Brush { get; }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnIsSelectedChanged();
                OnPropertyChanged();
            }
        }

        public ActivityColor(SolidColorBrush brush, bool isSelected = false) : this(isSelected)
        {
            Brush = brush;
        }

        public ActivityColor(Color color, bool isSelected) : this(isSelected) 
        {
            Brush = new SolidColorBrush(color);
        }

        public ActivityColor(string hexColor, bool isSelected = false) : this(isSelected)
        {
            Brush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
        }

        protected ActivityColor(bool isSelected = false)
        {
            IsSelected = isSelected;
        }

        private void OnIsSelectedChanged()
        {
            SelectedEvent?.Invoke(this, IsSelected);
            OnPropertyChanged();
        }
    }
}
