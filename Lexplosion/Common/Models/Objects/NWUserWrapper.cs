using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Tools;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class NWUserWrapper : VMBase
    {
        private readonly NwUser _user;
        private readonly CancellationToken _cancellationToken;

        public string Login => _user.Login;

        private byte[] _logo;
        public byte[] Logo 
        { 
            get => _logo; private set 
            {
                _logo = value;
                OnPropertyChanged();
            }
        }

        public ActivityStatus Status => _user.ActivityStatus;
        public uint ManualFriendsCount => 0;
        public string CurrentRunningInstanceName => ActivityStatusToString(Status);

        private bool _isSendFriendRequests;
        public bool IsSendFriendRequests
        {
            get => _isSendFriendRequests; set
            {
                _isSendFriendRequests = value;
                OnPropertyChanged();
            }
        }

        public NWUserWrapper(NwUser user, CancellationToken cancellationToken)
        {
            _user = user;
            _cancellationToken = cancellationToken;
            Task.Run(async () => await DownloadFile());
        }

        public async Task DownloadFile()
        {
            // регистрируем вызов CancelAsync() как реакцию на отмену токена,
            // using нужен для отмены регистрации после завершения скачивания
            try
            {
                using (var web = new WebClient())
                { 
                    using (_cancellationToken.Register(web.CancelAsync))
                    {
                        Logo = await web.DownloadDataTaskAsync(new Uri(_user.AvatarUrl));
                        Console.WriteLine(_user.Login);
                    }
                }
            }
            catch 
            {
                Logo = new byte[0] { };
            }
        }


        public void ExecuteOnPropertiesChanged()
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                OnPropertyChanged(prop.Name);
            }
        }

        private string ActivityStatusToString(ActivityStatus status)
        {
            switch (status)
            {
                case ActivityStatus.Offline: return ResourceGetter.GetString("offline");
                case ActivityStatus.Online: return ResourceGetter.GetString("online");
                case ActivityStatus.InGame:
                    var s = String.Format(ResourceGetter.GetString("playingIn"), _user.GameClientName);
                    if (string.IsNullOrEmpty(_user.GameClientName)) 
                    {
                        s = ResourceGetter.GetString("playing");
                    }
                    return s;
                case ActivityStatus.NotDisturb: return ResourceGetter.GetString("doNotDisturb");
            }
            return "";
        }
    }
}
