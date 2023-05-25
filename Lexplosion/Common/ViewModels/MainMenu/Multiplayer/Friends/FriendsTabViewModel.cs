using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Controls;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Tools;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public class FriendsTabModel : VMBase
    {
        public ObservableCollection<NWUserWrapper> Friends { get; } = new ObservableCollection<NWUserWrapper>();
        public ObservableCollection<NWUserWrapper> FriendRequestsOutgoing { get; } = new ObservableCollection<NWUserWrapper>();
        public ObservableCollection<NWUserWrapper> FriendRequestsIncoming { get; } = new ObservableCollection<NWUserWrapper>();


        public FriendsTabModel()
        {
            //var nicks = new List<string> { "_Hel2x_", "VagOne", "Andrysha", "Sklaip", "Petya", "Eblan Kakoyta" };

            var random = new Random();

            foreach (var friend in NightWorldApi.GetFriends(GlobalData.User.UUID, GlobalData.User.SessionToken, GlobalData.User.Login))
            {
                Friends.Add(new NWUserWrapper(friend));
            }

            UpdateFriendRequestsOutcoming();
            UpdateFriendRequestsIncoming();
        }


        #region Public Methods


        public void UpdateFriendRequestsOutcoming() 
        {
            FriendRequestsOutgoing.Clear();
            foreach (var friendRequest in NightWorldApi.GetFriendRequests(GlobalData.User.UUID, GlobalData.User.SessionToken).Outgoing)
            {
                FriendRequestsOutgoing.Add(new NWUserWrapper(friendRequest));
            }
        }

        public void UpdateFriendRequestsIncoming() 
        {
            FriendRequestsIncoming.Clear();
            foreach (var friendRequest in NightWorldApi.GetFriendRequests(GlobalData.User.UUID, GlobalData.User.SessionToken).Incoming)
            {
                FriendRequestsIncoming.Add(new NWUserWrapper(friendRequest));
            }
        }


        #endregion Public Methods
    }

    public class FriendsTabViewModel : VMBase, INotifiable
    {
        public FriendsTabModel Model { get; }


        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; protected set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }


        #region Commands


        private RelayCommand _removeFriendCommand;
        public ICommand RemoveFriendCommand 
        {
            get => _removeFriendCommand ?? (_removeFriendCommand = new RelayCommand(obj => 
            {
                var friend = (NWUserWrapper)obj;
                NightWorldApi.RemoveFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, friend.Login);
                // TODO: Friends Translate
                DoNotification("Friends Changed", friend.Login + " is not your friend now(", 5, 0);
            }));
        }

        private RelayCommand _cancelFriendRequestCommand;
        public ICommand CancelFriendRequestCommand
        {
            get => _cancelFriendRequestCommand ?? (_cancelFriendRequestCommand = new RelayCommand(obj =>
            {
                var friend = (NWUserWrapper)obj;
                NightWorldApi.RemoveFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, friend.Login);
                // TODO: Friends Translate
                DoNotification("Friends Changed", friend.Login + " friends requests was cancel(", 5, 0);
            }));
        }

        /// <summary>
        /// Добавление друга, в качестве агрумента obj, будет передаваться ссылка на объект друга.
        /// </summary>
        private RelayCommand _addFriendCommand;
        public RelayCommand AddFriendCommand
        {
            get => _addFriendCommand ?? (_addFriendCommand = new RelayCommand(obj =>
            {
                var user = (NWUserWrapper)obj;
                NightWorldApi.AddFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, user.Login);
                // TODO: Friends Translate
                DoNotification("Friends Changed", user.Login + " is your friend now(", 5, 0);
            }));
        }


        #endregion Commands


        public FriendsTabViewModel(DoNotificationCallback doNotification)
        {
            DoNotification = doNotification ?? DoNotification;
            Model = new FriendsTabModel();
        }
    }
}
