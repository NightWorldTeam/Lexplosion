using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class TabItemModel : ViewModelBase
    {
        public event Action<TabItemModel, bool> SelectedChanged;

        public string Name { get; }
        public string TextKey { get; set; }
        public ViewModelBase Content { get; set; }


        #region Constructors        


        public TabItemModel()
        {
            
        }

        public TabItemModel(string name, string textKey, ViewModelBase content, bool isSelected = false)
        {
            Name = name;
            TextKey = textKey;
            Content = content;
            IsSelected = isSelected;
        }


        #endregion Constructors


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
    }
}
