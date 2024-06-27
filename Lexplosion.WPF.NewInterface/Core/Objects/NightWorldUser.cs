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

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class NightWorldUser : ObservableObject, IEquatable<NightWorldUser>
    {
        private readonly CancellationToken _cancellationToken;


        #region Properties


        public string Login { get; }
        public ImageSource Avatar { get; private set; }
        public string RunningClientName { get; }
        public ActivityStatus Status { get; }


        private bool _hasFriendRequestSent;
        public bool HasFriendRequestSent
        {
            get => _hasFriendRequestSent; private set
            {
                _hasFriendRequestSent = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public NightWorldUser()
        {

        }

        public NightWorldUser(NwUser nwUser)
        {
            Login = nwUser.Login;
            RunningClientName = nwUser.GameClientName;
            Status = nwUser.ActivityStatus;
            Avatar = new BitmapImage(new Uri(nwUser.AvatarUrl));
        }


        #endregion Constructors


        #region Public & Protected Methods


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
            NightWorldApi.AddFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, Login);
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
            NightWorldApi.RemoveFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, Login);
        }

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
