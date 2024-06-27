using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class TabItemModel : ViewModelBase
    {
        public event Action<TabItemModel, bool> SelectedChanged;


        #region Properties


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
