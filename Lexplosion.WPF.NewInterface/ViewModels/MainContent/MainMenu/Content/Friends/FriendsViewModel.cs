using Lexplosion.WPF.NewInterface.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.MainMenu
{
    public struct Friend 
    {
        
    }

    public class FriendsViewModel : ViewModelBase
    {
        private ObservableCollection<Friend> _friends = new ObservableCollection<Friend>();
        public IEnumerable<Friend> Friends { get => _friends; }
    }
}
