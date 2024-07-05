using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends
{
    public class Friend : ObservableObject
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
        public ActivityStatus ActivityStatus { get; set; }
        public string AvatarUrl { get; set; }
        public string RunningClientName { get; set; }


        public Friend(string name, string status, FriendState state, string avatar, string runningClientName)
        {
            Name = name;
            Status = status;
            State = state;
            AvatarUrl = avatar;
            RunningClientName = runningClientName;
        }

        public void Unfriend(Action afterMethodAction = null) 
        {
            NightWorldApi.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Name);
            afterMethodAction?.Invoke();
        }
    }
}
