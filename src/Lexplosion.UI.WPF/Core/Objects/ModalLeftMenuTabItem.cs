using System;

namespace Lexplosion.UI.WPF.Core.Objects
{
    public struct ModalLeftMenuTabItem
    {
        public event Action<ModalLeftMenuTabItem, bool> SelectedEvent;


        #region Properties


        public int Id { get; set; }
        public string TitleKey { get; set; }
        public string IconKey { get; set; }
        public ViewModelBase Content { get; set; }
        public bool IsEnable { get; set; }


        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnIsSelectedChanged(value);
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


        private void OnIsSelectedChanged(bool state)
        {
            SelectedEvent?.Invoke(this, state);
        }


        #endregion Private Methods
    }
}
