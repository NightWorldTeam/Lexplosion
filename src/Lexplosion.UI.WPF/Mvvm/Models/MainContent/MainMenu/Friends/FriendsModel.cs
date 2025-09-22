using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.Friends
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
                activeAccount.WaitAuth();
                var friends = Runtime.ServicesContainer.NwApi.GetFriends(activeAccount.UUID, activeAccount.SessionToken, activeAccount.Login);
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

                var i = 0;
                foreach (var friend in friends)
                {
                    var friendObj = new Friend(friend);
                    friendObj.Unfriended += FriendObj_Unfriended;
                    _allFriends.Add(friendObj);
                    i++;
                }

                OnPropertyChanged(nameof(HasFriends));
            });
        }

        private void OnSearchBoxTextChanged(string value)
        {
            value ??= string.Empty;
            AllFriends.Filter = (obj => (obj as Friend).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
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
