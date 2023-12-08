using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class FriendsViewModel : ViewModelBase
    {
        private ObservableCollection<Friend> _friends = new ObservableCollection<Friend>();
        public IEnumerable<Friend> Friends { get => _friends; }


        public FriendsViewModel()
        {
            for (var i = 0; i < 3; i++) 
            {
                _friends.Add(new Friend($"Person {i}", "Online", (Friend.FriendState)i));
            }
        }
    }
}
