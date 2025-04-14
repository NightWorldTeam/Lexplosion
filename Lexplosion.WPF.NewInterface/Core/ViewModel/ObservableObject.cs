using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Core.ViewModel
{
    /// <summary>
    /// TODO: Изменить namespace
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
