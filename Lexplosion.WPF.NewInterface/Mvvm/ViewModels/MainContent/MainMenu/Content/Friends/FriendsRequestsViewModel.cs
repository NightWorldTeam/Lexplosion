using Lexplosion.WPF.NewInterface.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class FriendsRequestsViewModel : ViewModelBase
    {
        private ObservableCollection<object> _outgoing = new ObservableCollection<object>();
        private ObservableCollection<object> _incoming = new ObservableCollection<object>();

        public IEnumerable<object> Outgoing { get => _outgoing; }
        public IEnumerable<object> Incoming { get => _incoming; }
    }
}
