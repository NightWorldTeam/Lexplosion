using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
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
        public byte[] Logo { get; private set; } = new byte[0];
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
            //HasFriendRequestSent = nwUser.

            Task.Run(() =>
            {
                Logo = DownloadLogoAsync(nwUser.AvatarUrl).Result;
                OnPropertyChanged(nameof(Logo));
            });
        }


        #endregion Constructors




        #region Public Methods


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

        #endregion Public Methods



        #region Private Methods


        private async Task<byte[]> DownloadLogoAsync(string url)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (_cancellationToken.Register(httpClient.CancelPendingRequests))
                    {
                        using (var response = await httpClient.GetAsync(url))
                        {
                            response.EnsureSuccessStatusCode();

                            return await response.Content.ReadAsByteArrayAsync();
                        }
                    }
                }
            }
            catch
            {
                return new byte[0];
            }
        }


        private string ActivityStatusToStringKey(ActivityStatus status)
        {
            switch (status)
            {
                case ActivityStatus.Offline:
                    return "Offiline";
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
