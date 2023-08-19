using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public sealed class LeftPanelMenuItem : ViewModelBase, IComparable<LeftPanelMenuItem>
    {
        public event Action<LeftPanelMenuItem> SelectedEvent;

        public uint Id { get; set; }
        public string TextKey { get; set; }
        public string Icon { get; set; }

        public double IconWidth { get; set; }
        public double IconHeight { get; set; }

        public ViewModelBase Content { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;

                if (_isSelected)
                {
                    SelectedEvent?.Invoke(this);
                }
                OnPropertyChanged();
            }
        }

        public int CompareTo(LeftPanelMenuItem other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
