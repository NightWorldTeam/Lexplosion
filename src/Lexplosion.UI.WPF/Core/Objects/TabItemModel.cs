using System;

namespace Lexplosion.UI.WPF.Core.Objects
{
    public class TabItemModel : ViewModelBase, IComparable<TabItemModel>
    {
        public event Action<TabItemModel, bool> SelectedChanged;


        #region Properties


        public uint Id { get; set; }

        public string TextKey { get; set; }
        public ViewModelBase Content { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                SelectedChanged?.Invoke(this, value);
                OnPropertyChanged();
            }
        }

        public int CompareTo(TabItemModel other)
        {
            return Id.CompareTo(other.Id);
        }


        #endregion Properties


        #region Constructors        


        public TabItemModel()
        {
            
        }

        public TabItemModel(string textKey, ViewModelBase content, bool isSelected = false)
        {
            TextKey = textKey;
            Content = content;
            IsSelected = isSelected;
        }


        #endregion Constructors
    }
}
