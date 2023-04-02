using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lexplosion
{
    public abstract class VMBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public virtual void Dispose() { }
    }
}
