using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lexplosion.UI.WPF.Core.Objects.Users;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.Friends
{
    public sealed class FriendRequestsModel : ViewModelBase
    {
        public event Action<NightWorldUserBase> FriendAdded;

        private AppCore _appCore;
        private ObservableCollection<object> _outgoing = [];
        private ObservableCollection<object> _incoming = [];


        public FiltableObservableCollection Incoming { get; } = new();
        public FiltableObservableCollection Outgoing { get; } = new();


        private string _searchBoxText;
        public string SearchBoxText
        {
            get => _searchBoxText; set
            {
                _searchBoxText = value;
                OnSearchBoxTextChanged(value);
                OnPropertyChanged();
            }
        }


        #region Contrustors


        public FriendRequestsModel(AppCore appCore)
        {
            _appCore = appCore;
            Outgoing.Source = _outgoing;
            Incoming.Source = _incoming;

            UpdateRequestsData();
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void UpdateRequestsData()
        {
            Runtime.TaskRun(() =>
            {
                var activeAccount = Account.ActiveAccount;
                var friendRequests = Runtime.ServicesContainer.NwApi.GetFriendRequests(activeAccount.UUID, activeAccount.SessionToken);

                App.Current.Dispatcher?.Invoke(() =>
                {
                    UpdateFriendRequestsOutcoming(friendRequests.Outgoing);
                    UpdateFriendRequestsIncoming(friendRequests.Incoming);
                });
            });
        }


        public void AddFriend(NightWorldUserRequest friend)
        {
            Runtime.TaskRun(() =>
            {
                friend.AddFriend();
                _appCore.UIThread(() =>
                {
                    UpdateRequestsData();
                    FriendAdded?.Invoke(friend);
                });
            });
        }

        public void DeclineFriend(NightWorldUserRequest friend)
        {
            Runtime.TaskRun(() =>
            {
                friend.DeclineFriend();
                _appCore.UIThread(() =>
                {
                    UpdateRequestsData();
                });
            });
        }

        #endregion Publuc & Protected Methods


        #region Private Methods


        private void OnSearchBoxTextChanged(string value)
        {
            if (Outgoing.Source == null || Incoming.Source == null)
                return;

            value ??= string.Empty;

            // TODO: Есть возможно оптимизировать, зная какая конкретная вкладка сейчас открыта.

            Outgoing.Filter = (i => (i as NightWorldUserBase).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            Incoming.Filter = (i => (i as NightWorldUserBase).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }



        private void UpdateFriendRequestsOutcoming(IEnumerable<NwUser> outgoingRequests)
        {
            _outgoing.Clear();
            foreach (var friendRequest in outgoingRequests)
            {
                _outgoing.Add(new NightWorldUserRequest(friendRequest));
            }
        }

        private void UpdateFriendRequestsIncoming(IEnumerable<NwUser> incomingRequests)
        {
            _incoming.Clear();
            foreach (var friendRequest in incomingRequests)
            {
                _incoming.Add(new NightWorldUserRequest(friendRequest));
            }
        }


        #endregion Private Methods
    }
}
