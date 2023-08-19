namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class TabItemModel : ViewModelBase
    {
        public string TextKey { get; set; }
        public ViewModelBase Content { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
