using Lexplosion.WPF.NewInterface.Core.ViewModel;

namespace Lexplosion.WPF.NewInterface.Core.Objects
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
