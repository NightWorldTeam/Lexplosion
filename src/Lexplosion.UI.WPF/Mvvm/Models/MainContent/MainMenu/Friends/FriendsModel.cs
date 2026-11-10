using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using Lexplosion.Logic.Lifecycle;

namespace Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.Friends
{
    public sealed class FriendsModel : ViewModelBase, IAsyncLoadable
    {
        public event Action<Friend> Unfriended;

        private readonly BulkObservableCollection<Friend> _allFriends = new BulkObservableCollection<Friend>();
        private int _requestVersion;
        private Task _loadTask;
        private bool _loaded;
        private Account _loadingAccount;
        private Account _loadedAccount;
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
        }

        public Task EnsureLoadedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_loaded && ReferenceEquals(_loadedAccount, Account.ActiveAccount))
                return Task.CompletedTask;

            if (_loadTask != null && !_loadTask.IsCompleted &&
                ReferenceEquals(_loadingAccount, Account.ActiveAccount))
                return _loadTask;

            return RefreshAsync(cancellationToken);
        }

        // Keep the existing command/event entry point during migration.
        public void UpdateRequestsData()
        {
            _ = RefreshAsync();
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            int version = Interlocked.Increment(ref _requestVersion);
            _loaded = false;
            _loadingAccount = Account.ActiveAccount;
            _loadTask = LoadCoreAsync(version, _loadingAccount, cancellationToken);
            return _loadTask;
        }

        private async Task LoadCoreAsync(int version, Account account, CancellationToken cancellationToken)
        {
            IsLoading = true;
            HasLoadError = false;
            try
            {
                if (account == null)
                    throw new InvalidOperationException("No active account");

#if DEBUG
                using (UiDebugTrace.Measure("friends.wait-auth", 5))
#endif
                    await account.WaitAuthAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (!ReferenceEquals(account, Account.ActiveAccount))
                    return;

                List<NwUser> friends;
#if DEBUG
                using (UiDebugTrace.Measure("friends.api-get", 5))
#endif
                    friends = await Runtime.ServicesContainer.NwApi.GetFriendsAsync(
                        account.UUID, account.SessionToken, account.Login, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                if (version != Volatile.Read(ref _requestVersion) ||
                    !ReferenceEquals(account, Account.ActiveAccount)) return;

                UpdateFriends(friends);
                _loadedAccount = account;
                _loaded = true;
                HasLoadError = false;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The next activation can start a new request.
            }
            catch (Exception error)
            {
                Runtime.DebugWrite("Friends loading failed: " + error.GetType().Name);
                if (version == Volatile.Read(ref _requestVersion) &&
                    ReferenceEquals(account, Account.ActiveAccount))
                    HasLoadError = true;
            }
            finally
            {
                if (version == Volatile.Read(ref _requestVersion))
                    IsLoading = false;
            }
        }

        private void UpdateFriends(IEnumerable<NwUser> friends)
        {
#if DEBUG
            using var trace = UiDebugTrace.Measure("friends.update-collection", 3);
#endif
            // Build the next snapshot before notifying any WPF bindings.
            // The filtered collection can then refresh exactly once.
            var items = new List<Friend>();
            foreach (var friend in friends)
            {
                var item = new Friend(friend);
                item.Unfriended += FriendObj_Unfriended;
                items.Add(item);
            }

            foreach (var previous in _allFriends)
                previous.Unfriended -= FriendObj_Unfriended;
            _allFriends.ReplaceAll(items);
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
