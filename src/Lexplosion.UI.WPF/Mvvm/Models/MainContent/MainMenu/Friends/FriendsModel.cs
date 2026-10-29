using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.Friends
{
    public sealed class FriendsModel : ViewModelBase
    {
        public event Action<Friend> Unfriended;

        private readonly ObservableCollection<Friend> _allFriends = new ObservableCollection<Friend>();
        private int _requestVersion;
        private bool _isLoading;
        private bool _hasLoadError;
        private string _searchBoxText;

        public FiltableObservableCollection AllFriends { get; } = new FiltableObservableCollection();
        public bool HasFriends => AllFriends.Count > 0;
        public bool IsEmpty => !IsLoading && !HasLoadError && _allFriends.Count == 0;

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public bool HasLoadError
        {
            get => _hasLoadError;
            private set
            {
                if (_hasLoadError == value) return;
                _hasLoadError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public string SearchBoxText
        {
            get => _searchBoxText;
            set
            {
                _searchBoxText = value;
                var search = value ?? string.Empty;
                AllFriends.Filter = obj => ((Friend)obj).Login.IndexOf(
                    search, StringComparison.InvariantCultureIgnoreCase) >= 0;
                OnPropertyChanged();
            }
        }

        public FriendsModel()
        {
            AllFriends.Source = _allFriends;
            UpdateRequestsData();
        }

        public void UpdateRequestsData()
        {
            int version = Interlocked.Increment(ref _requestVersion);
            IsLoading = true;
            HasLoadError = false;
            Runtime.TaskRun(() =>
            {
                try
                {
                    var account = Account.ActiveAccount;
                    if (account == null)
                        throw new InvalidOperationException("No active account");
#if DEBUG
                    using (UiDebugTrace.Measure("friends.wait-auth", 5))
#endif
                        account.WaitAuth();
                    List<NwUser> friends;
#if DEBUG
                    using (UiDebugTrace.Measure("friends.api-get", 5))
#endif
                        friends = Runtime.ServicesContainer.NwApi.GetFriends(
                            account.UUID, account.SessionToken, account.Login);
                    Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (version != Volatile.Read(ref _requestVersion)) return;
                        UpdateFriends(friends);
                        HasLoadError = false;
                        IsLoading = false;
                    }));
                }
                catch (Exception error)
                {
                    Trace.WriteLine("Friends loading failed: " + error.GetType().Name);
                    Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (version != Volatile.Read(ref _requestVersion)) return;
                        HasLoadError = true;
                        IsLoading = false;
                    }));
                }
            });
        }

        private void UpdateFriends(IEnumerable<NwUser> friends)
        {
#if DEBUG
            using var trace = UiDebugTrace.Measure("friends.update-collection", 3);
#endif
            foreach (var friend in _allFriends)
                friend.Unfriended -= FriendObj_Unfriended;
            _allFriends.Clear();
            foreach (var friend in friends)
            {
                var item = new Friend(friend);
                item.Unfriended += FriendObj_Unfriended;
                _allFriends.Add(item);
            }
            OnPropertyChanged(nameof(HasFriends));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void FriendObj_Unfriended(Friend friend)
        {
            Unfriended?.Invoke(friend);
            friend.Unfriended -= FriendObj_Unfriended;
        }
    }
}
