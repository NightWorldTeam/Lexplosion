using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public struct ModalLeftMenuTabItem
    {
        public event Action<ModalLeftMenuTabItem> SelectedEvent;


        #region Properties


        public int Id { get; set; }
        public string TitleKey { get; set; }
        public string IconKey { get; set; }
        public ViewModelBase Content { get; set; }
        public bool IsEnable { get; set; }


        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnIsSelectedChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public ModalLeftMenuTabItem(int id, string titleKey, string iconKey, ViewModelBase content, bool isEnable = true, bool isSelected = false)
        {
            Id = id;
            TitleKey = titleKey;
            IconKey = iconKey;
            Content = content;
            IsEnable = isEnable;
            IsSelected = isSelected;
        }


        #endregion Constructors


        #region Private Methods


        private void OnIsSelectedChanged()
        {
            SelectedEvent?.Invoke(this);
        }


        #endregion Private Methods
    }
}
