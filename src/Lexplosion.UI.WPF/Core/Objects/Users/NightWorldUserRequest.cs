using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.UI.WPF.Core.Objects.Users
{
    public class NightWorldUserRequest : NightWorldUserBase
    {
        private NightWorldApi _api = Runtime.ServicesContainer.NwApi;


        #region Constructors


        public NightWorldUserRequest() : base()
        {

        }

        public NightWorldUserRequest(NwUser nwUser) : base(nwUser, NightWorldUserFriendshipState.Requested)
        {

        }


        #endregion Constructors


        public void AddFriend()
        {
            _api.AddFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }

        public void DeclineFriend()
        {
            _api.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }
    }
}
