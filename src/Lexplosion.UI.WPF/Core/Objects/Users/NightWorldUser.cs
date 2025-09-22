using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;

namespace Lexplosion.UI.WPF.Core.Objects.Users
{
    public class NightWorldUser : NightWorldUserBase
    {
		private readonly NightWorldApi _api = Runtime.ServicesContainer.NwApi;

        public string Test { get; } = "123";

        private bool _hasFriendRequestSent;
        public bool HasFriendRequestSent
        {
            get => _hasFriendRequestSent; private set
            {
                _hasFriendRequestSent = value;
                OnPropertyChanged();
            }
        }


        #region Constructors


        public NightWorldUser() : base()
        {
            
        }

        public NightWorldUser(NwUser nwUser, NightWorldUserFriendshipState friendshipState) : base(nwUser, friendshipState)
        {
            
        }


        #endregion Constructors


        /// <summary>
        /// Отправляет заявку в друзья.
        /// </summary>
        /// <param name="other">Пользователь которому отправляется заявка.</param>
        public void SendFriendRequest()
        {
            if (_hasFriendRequestSent)
            {
                return;
            }

            HasFriendRequestSent = true;
			_api.AddFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }

        /// <summary>
        /// Отменяет запрос в друзья.
        /// </summary>
        /// <param name="other">Пользователь с запросом в друзья.</param>
        public void CancelFriendRequest()
        {
            if (!HasFriendRequestSent)
            {
                return;
            }

            HasFriendRequestSent = false;
			_api.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }
    }
}
