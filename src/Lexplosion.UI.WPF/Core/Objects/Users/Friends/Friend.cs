using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects.Nightworld;
using System;

namespace Lexplosion.UI.WPF.Core.Objects.Users
{
    public partial class Friend : NightWorldUserBase
    {
        public event Action<Friend> Unfriended;


        public Friend(NwUser user) : base(user, NightWorldUserFriendshipState.Added)
        {
        }


        public void Unfriend()
        {
            Runtime.ServicesContainer.NwApi.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
            Unfriended?.Invoke(this);
        }
    }
}
