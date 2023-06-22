using System;
using System.Windows.Input;

namespace Lexplosion.Common
{
    public sealed class Tab<T> : IComparable<Tab<T>>
    {
        public event Action<bool> IsSelectedChangedEvent;

        public ushort Id { get; set; }
        public string Header { get; set; }
        public T Content { get; set; }

        private bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected; set 
            {
                _isSelected = value;
                IsSelectedChangedEvent?.Invoke(value);
            }
        }

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
