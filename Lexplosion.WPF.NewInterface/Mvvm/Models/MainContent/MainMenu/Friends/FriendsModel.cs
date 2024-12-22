﻿using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends
{
    public sealed class FriendsModel : ViewModelBase
    {
        public event Action<Friend> Unfriended;

        private readonly ObservableCollection<Friend> _allFriends = [];
        public FiltableObservableCollection AllFriends { get; set; } = new();


        #region Properties


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


        public bool HasFriends { get => AllFriends.Count > 0; }


        #endregion Properties


        #region Constructors


        public FriendsModel()
        {
            AllFriends.Source = _allFriends;
            UpdateRequestsData();
        }


        #endregion Constructors


        #region Public Methods


        public void UpdateRequestsData()
        {
            Runtime.TaskRun(() =>
            {
                var activeAccount = Account.ActiveAccount;
                var friends = NightWorldApi.GetFriends(activeAccount.UUID, activeAccount.SessionToken, activeAccount.Login);
                App.Current.Dispatcher.Invoke(() => UpdateFriends(friends));
            });
        }

        #endregion Public Methods


        #region Private Methods


        private void UpdateFriends(IEnumerable<NwUser> friends)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _allFriends.Clear();

                foreach (var friend in friends)
                {
                    var friendObj = new Friend(friend.Login, new FriendStatus(friend.ActivityStatus), Friend.FriendState.Added, friend.AvatarUrl, friend.GameClientName);
                    friendObj.Unfriended += FriendObj_Unfriended;
                    _allFriends.Add(friendObj);
                    OnPropertyChanged(nameof(HasFriends));
                }
            });
        }

        private void OnSearchBoxTextChanged(string value)
        {
            value ??= string.Empty;
            AllFriends.Filter = (obj => (obj as Friend).Name.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }

        /// <summary>
        /// Вызывает эвент удаления из друзей
        /// </summary>
        /// <param name="obj">Удаленный друг</param>
        private void FriendObj_Unfriended(Friend obj)
        {
            Unfriended?.Invoke(obj);
            obj.Unfriended -= FriendObj_Unfriended;
        }


        #endregion Private Methods
    }

}
