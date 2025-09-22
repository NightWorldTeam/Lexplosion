using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;

namespace Lexplosion.WPF.NewInterface.TrayMenu
{
    public abstract class TrayComponentBase : ObservableObject, IComparable
    {
        public readonly int Id;

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled; set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        protected TrayComponentBase(int id)
        {
            Id = id;
        }

        public int CompareTo(object obj)
        {
            return ((int)obj).CompareTo(Id);
        }
    }
}
