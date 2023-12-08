using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends
{
    public class Friend : ViewModelBase
    {
        public enum FriendState
        {
            NotAdded,
            Requested,
            Added
        }

        public FriendState State { get; set; } = FriendState.NotAdded;
        public string Name { get; }
        public string Status { get; set; }


        public Friend(string name, string status, FriendState state)
        {
            Name = name;
            Status = status;
            State = state;
        }
    }
}
