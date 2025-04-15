using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu;
using Lexplosion.Logic.Management.Accounts;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class NightWorldUser : NightWorldUserBase
    {
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

        public NightWorldUser(NwUser nwUser) : base(nwUser)
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
            NightWorldApi.AddFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
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
            NightWorldApi.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }
    }

    public class NightWorldUserRequest : NightWorldUserBase
    {
        #region Constructors


        public NightWorldUserRequest() : base()
        {

        }

        public NightWorldUserRequest(NwUser nwUser) : base(nwUser)
        {

        }


        #endregion Constructors


        public void AddFriend() 
        {
            NightWorldApi.AddFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }

        public void DeclineFriend() 
        {
            NightWorldApi.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, Login);
        }
    }


    public abstract class NightWorldUserBase : ObservableObject, IEquatable<NightWorldUser>
    {
        private readonly CancellationToken _cancellationToken;


        #region Properties


        public string Login { get; }
        public string AvatarUrl { get; private set; }
        public string RunningClientName { get; }
        public ActivityStatus Status { get; }


        #endregion Properties


        #region Constructors


        protected NightWorldUserBase()
        {

        }

        protected NightWorldUserBase(NwUser nwUser)
        {
            Login = nwUser.Login;
            RunningClientName = nwUser.GameClientName;
            Status = nwUser.ActivityStatus;
            AvatarUrl = nwUser.AvatarUrl;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override bool Equals(object obj)
        {
            if (obj == null || obj is not NightWorldUser)
            {
                return false;
            }

            return (obj as NightWorldUser).Login == Login;
        }

        public bool Equals(NightWorldUser other)
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
