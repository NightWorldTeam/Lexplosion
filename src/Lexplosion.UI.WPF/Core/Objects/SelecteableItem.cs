using Lexplosion.UI.WPF.Core.ViewModel;

namespace Lexplosion.UI.WPF.Core.Objects
{
    public class SelecteableItem : ObservableObject
    {
        public string Name { get; }
        public bool IsSelected { get; set; }
        public object Item { get; }
    }

    public class SelecteableItem<T> : SelecteableItem
    {
        public new T Item { get; }
    }
}
