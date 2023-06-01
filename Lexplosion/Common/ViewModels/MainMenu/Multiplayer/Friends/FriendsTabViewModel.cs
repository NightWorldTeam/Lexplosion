using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Controls;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Tools;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public class FriendsTabModel : VMBase, INotifiable
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 3) };

        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; protected set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }

        public ObservableCollection<NWUserWrapper> Friends { get; } = new ObservableCollection<NWUserWrapper>();
        public ObservableCollection<NWUserWrapper> FriendRequestsOutgoing { get; } = new ObservableCollection<NWUserWrapper>();
        public ObservableCollection<NWUserWrapper> FriendRequestsIncoming { get; } = new ObservableCollection<NWUserWrapper>();


        public FriendsTabModel()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var friendRequests = NightWorldApi.GetFriendRequests(GlobalData.User.UUID, GlobalData.User.SessionToken);
                var friends = NightWorldApi.GetFriends(GlobalData.User.UUID, GlobalData.User.SessionToken, GlobalData.User.Login);

                App.Current.Dispatcher?.Invoke(() => {
                    UpdateFriendRequestsOutcoming(friendRequests.Outgoing);
                    UpdateFriendRequestsIncoming(friendRequests.Incoming);
                    UpdateFriends(friends);
                });
            });
            _timer.Tick += timer_Tick;
        }

        public void StartDataUpdate() 
        {
            _timer.Start();
        }

        public void StopDataUpdate() 
        {
            _timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                var friendRequests = NightWorldApi.GetFriendRequests(GlobalData.User.UUID, GlobalData.User.SessionToken);
                var friends = NightWorldApi.GetFriends(GlobalData.User.UUID, GlobalData.User.SessionToken, GlobalData.User.Login);

                App.Current.Dispatcher?.Invoke(() => { 
                    UpdateFriendRequestsOutcoming(friendRequests.Outgoing);
                    UpdateFriendRequestsIncoming(friendRequests.Incoming);
                    UpdateFriends(friends);
                });
            });
        }


        #region Public Methods


        private void UpdateFriends(IEnumerable<NwUser> friends) 
        {
            Friends.Clear();
            foreach (var friend in friends)
            {
                Friends.Add(new NWUserWrapper(friend));
            }
        }

        private void UpdateFriendRequestsOutcoming(IEnumerable<NwUser> outgoingRequests) 
        {
            FriendRequestsOutgoing.Clear();
            foreach (var friendRequest in outgoingRequests)
            {
                FriendRequestsOutgoing.Add(new NWUserWrapper(friendRequest));
            }
        }

        private void UpdateFriendRequestsIncoming(IEnumerable<NwUser> incomingRequests) 
        {
            FriendRequestsIncoming.Clear();
            foreach (var friendRequest in incomingRequests)
            {
                FriendRequestsIncoming.Add(new NWUserWrapper(friendRequest));
            }
        }

        public void RemoveFriendRequest(NWUserWrapper user)
        {
            if (FriendRequestsOutgoing.Contains(user))
            {
                FriendRequestsOutgoing.Remove(user);
            }
            else if (FriendRequestsIncoming.Contains(user))
            {
                FriendRequestsIncoming.Remove(user);
            }
        }

        public void RemoveFriend(NWUserWrapper user)
        {
            if (Friends.Contains(user))
            {
                Friends.Remove(user);
                DoNotification(ResourceGetter.GetString("friendsChanged"), user.Login + " " + ResourceGetter.GetString("friendRemoved"), 5, 1);
            }
        }

        public void AddFriend(NWUserWrapper user) 
        {
            RemoveFriendRequest(user);
            Friends.Add(user);
            DoNotification(ResourceGetter.GetString("friendsChanged"), user.Login + " " + ResourceGetter.GetString("youAndFriendsNow"), 5, 1);
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
                Model.RemoveFriend(friend);
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
                Model.RemoveFriendRequest(friend);
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
                Model.AddFriend(user);
                // TODO: Friends Translate
                DoNotification("Friends Changed", user.Login + " is your friend now(", 5, 0);
            }));
        }


        #endregion Commands


        public FriendsTabViewModel(DoNotificationCallback doNotification)
        {
            DoNotification = _doNotification;
            Model = new FriendsTabModel();
        }
    }
}
