using Lexplosion.Core.Extensions;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Core.Objects
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


        #region Constructors


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
            hexColor = hexColor.Contains("#") ? hexColor : $"#{hexColor}";

            Brush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
        }

        protected ActivityColor(bool isSelected = false)
        {
            IsSelected = isSelected;
        }


        #endregion Constructors


        private void OnIsSelectedChanged()
        {
            SelectedEvent?.Invoke(this, IsSelected);
            OnPropertyChanged();
        }

        public static bool TryCreateColor(string value, out ActivityColor? color)
        {
            if (value.IsHexColor())
            {
                color = new(value);
                return true;
            }

            try
            {
                color = new((SolidColorBrush)new BrushConverter().ConvertFrom(value));
                return true;
            }
            catch
            {
                color = null;
                return false;
            }
        }
    }
}
