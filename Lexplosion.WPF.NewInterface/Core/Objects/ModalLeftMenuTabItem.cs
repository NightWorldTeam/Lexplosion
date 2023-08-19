using System;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public struct ModalLeftMenuTabItem
    {
        public event Action<ViewModelBase> SelectedEvent;

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
                OnIsSelectedChanged(Content);
            }
        }

        private void OnIsSelectedChanged(ViewModelBase content)
        {
            SelectedEvent?.Invoke(content);
        }
    }
}
