using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Forms;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends
{
    public class FriendStatus
    {
        public ActivityStatus Priority { get; set; }
        public string Value { get; set; }

        public FriendStatus(ActivityStatus activityStatus)
        {
            Priority = activityStatus;
            Value = activityStatus.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FriendStatus))
                return false;


            var priority = (obj as FriendStatus).Priority;

            return Priority == priority;
        }

        public override int GetHashCode()
        {
            return Priority.GetHashCode();
        }
    }

    public class Friend : ObservableObject
    {
        public event Action<Friend> Unfriended;

        public enum FriendState
        {
            NotAdded,
            Requested,
            Added
        }

        public FriendState State { get; set; } = FriendState.NotAdded;
        public string Name { get; }
        public FriendStatus Status { get; set; }
        public ActivityStatus ActivityStatus { get; set; }
        public string AvatarUrl { get; set; }
        public string RunningClientName { get; set; }
        public string BannerUrl { get; }


        public Friend(string name, FriendStatus status, FriendState state, string avatar, string runningClientName, string bannerUrl = null)
        {
            Name = name;
            Status = status;
            State = state;
            AvatarUrl = avatar;
            RunningClientName = runningClientName;
            BannerUrl = bannerUrl;
        }

        public void Unfriend() 
        {
            Runtime.ServicesContainer.NwApi.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Name);
            Unfriended?.Invoke(this);
        }
    }
}
