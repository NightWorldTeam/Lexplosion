using System;
using System.Windows.Input;

namespace Lexplosion.Gui
{
    public sealed class Tab : IComparable<Tab>
    {
        public ushort Id { get; set; }
        public string Header { get; set; }
        public VMBase Content { get; set; }
        public ICommand Command { get; set; } = new RelayCommand(obj => { });

        #nullable enable
        public int CompareTo(Tab? tab)
        {
            if (tab is null) throw new ArgumentException("Некорректное значение параметра");
            return Id.CompareTo(tab.Id);
        }
    }
}
