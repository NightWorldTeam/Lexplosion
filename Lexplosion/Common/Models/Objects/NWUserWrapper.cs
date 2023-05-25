using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Tools;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Media.Imaging;

namespace Lexplosion.Common.Models.Objects
{
    public sealed class NWUserWrapper : VMBase
    {
        private readonly NwUser _user;

        public string Login => _user.Login;
        public BitmapImage Avatar { get; }
        public ActivityStatus Status => _user.ActivityStatus;
        public uint ManualFriendsCount => 0;
        public string CurrentRunningInstanceName => string.IsNullOrEmpty(_user.GameClientName) ? ResourceGetter.GetString("minecraftIsNotRunning") : _user.GameClientName;

        public bool IsSendFriendRequests { get; set; }

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
    }
}
