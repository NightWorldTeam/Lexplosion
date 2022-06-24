using System;
using System.Collections;
using System.Collections.Generic;

namespace Lexplosion.Gui
{
    public sealed class Tab : IComparable<Tab>
    {
        public ushort Id { get; set; }
        public string Header { get; set; }
        public VMBase Content { get; set; }

        public int CompareTo(Tab? tab)
        {
            if (tab is null) throw new ArgumentException("Некорректное значение параметра");
            return Id.CompareTo(tab.Id);
        }
    }
}
