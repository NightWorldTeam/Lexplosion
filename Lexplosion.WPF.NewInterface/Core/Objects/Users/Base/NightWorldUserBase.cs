using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Core.Objects.Users
{
    public abstract class NightWorldUserBase : ObservableObject, IEquatable<NightWorldUserBase>
    {
        private readonly CancellationToken _cancellationToken;


        #region Properties


        public string Login { get; }
        public string AvatarUrl { get; private set; }
        public string RunningClientName { get; }
        /// <summary>
        /// Статус (онлайн,оффлайн, и т.д)
        /// </summary>
        public NightWorldUserStatus Status { get; }
        /// <summary>
        /// Если я правильно помню, это отношение данного пользователя к пользователю. (Дружеские)
        /// </summary>
        public NightWorldUserFriendshipState FriendshipState { get; }
        public NwUserBanner Banner { get; }


        #endregion Properties


        #region Constructors


        protected NightWorldUserBase()
        {

        }

        protected NightWorldUserBase(NwUser nwUser, NightWorldUserFriendshipState friendshipState)
        {
            Login = nwUser.Login;
            RunningClientName = nwUser.GameClientName;
            Status = new NightWorldUserStatus(nwUser.ActivityStatus);
            AvatarUrl = nwUser.AvatarUrl;
            Banner = nwUser.Banner;
            FriendshipState = friendshipState;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override bool Equals(object obj)
        {
            if (obj == null || obj is not NightWorldUserBase)
            {
                return false;
            }

            return (obj as NightWorldUserBase).Login == Login;
        }

        public bool Equals(NightWorldUserBase other)
        {
            return Login == other.Login;
        }


        #endregion Public Methods


        #region Private Methods


        private string ActivityStatusToStringKey(ActivityStatus status)
        {
            switch (status)
            {
                case ActivityStatus.Offline:
                    return "Offline";
                case ActivityStatus.Online:
                    return "Online";
                case ActivityStatus.InGame:
                    {
                        // TODO: Localization
                        return "Playing in";
                    }
                case ActivityStatus.NotDisturb:
                    return "DoNotDisturb";
            }
            return string.Empty;
        }


        #endregion Private Methods
    }
}
