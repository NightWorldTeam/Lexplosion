using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class TabItemModel : ViewModelBase, IComparable<TabItemModel>
    {
        public event Action<TabItemModel> SelectedChanged;

        public uint Id { get; set; }

        public string TextKey { get; set; }
        public ViewModelBase Content { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                SelectedChanged?.Invoke(this);
                OnPropertyChanged();
            }
        }

        public int CompareTo(TabItemModel other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
