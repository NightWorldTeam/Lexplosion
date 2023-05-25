using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Tools;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class NWUserWrapper : VMBase
    {
        private readonly NwUser _user;

        public string Login => _user.Login;
        public BitmapImage Avatar { get; }
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

        public NWUserWrapper(NwUser user)
        {
            Debug.WriteLine(_user);
            _user = user;
            Avatar = new BitmapImage();
            Avatar.BeginInit();
            Avatar.UriSource = new Uri(user.AvatarUrl, UriKind.Absolute);
            Avatar.EndInit();
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
