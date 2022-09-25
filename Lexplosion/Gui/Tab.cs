using System;
using System.Windows.Input;

namespace Lexplosion.Gui
{
    public sealed class Tab<T> : IComparable<Tab<T>>
    {
        public ushort Id { get; set; }
        public string Header { get; set; }
        public T Content { get; set; }
        public ICommand Command { get; set; } = new RelayCommand(obj => { });
        public bool IsVisible { get; set; } = true;

        #nullable enable
        public int CompareTo(Tab<T>? tab)
        {
            if (tab is null) throw new ArgumentException("Некорректное значение параметра");
            return Id.CompareTo(tab.Id);
        }
    }
}
