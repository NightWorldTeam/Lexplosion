using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class FriendsModel : ViewModelBase
    {
        private List<Friend> _allFriends = new();

        private ObservableCollection<Friend> _inGameFriends = new();
        private ObservableCollection<Friend> _onlineFriends = new();
        private ObservableCollection<Friend> _offlineFriends = new();


        #region Properties


        public CollectionViewSource AllFriends { get; } = new();
        public CollectionViewSource InGameFriends { get; } = new();
        public CollectionViewSource OnlineFriends { get; } = new();
        public CollectionViewSource OfflineFriends { get; } = new();


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


        #endregion Properties


        #region Constructors


        public FriendsModel()
        {
            AllFriends.Source = _allFriends;
            InGameFriends.Source = _inGameFriends;
            OnlineFriends.Source = _onlineFriends;
            OfflineFriends.Source = _offlineFriends;

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
                App.Current.Dispatcher.Invoke( () => UpdateFriends(friends) );
            });
        }


        /// <summary>
        /// Пересобирает списки с друзьями ники которых содержат выражение.
        /// </summary>
        /// <param name="value">Выражение</param>
        public void SearchFriendsByNickname(string value)
        {
            _allFriends.Where(f => f.Name.Contains(value));

            foreach (var f in _allFriends)
            {
                if (f.ActivityStatus == ActivityStatus.InGame)
                    _inGameFriends.Add(f);
                else if (f.ActivityStatus == ActivityStatus.Offline)
                    _offlineFriends.Add(f);
                else
                    _onlineFriends.Add(f);
            }
        }


        #endregion Public Methods


        #region Private Methods


        private void UpdateFriends(IEnumerable<NwUser> friends)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _allFriends.Clear();
                _inGameFriends.Clear();
                _onlineFriends.Clear();
                _offlineFriends.Clear();

                foreach (var friend in friends)
                {
                    var friendObj = new Friend(friend.Login, friend.ActivityStatus.ToString(), Friend.FriendState.Added, new BitmapImage(new System.Uri(friend.AvatarUrl)), friend.GameClientName);
                    _allFriends.Add(friendObj);

                    switch (friend.ActivityStatus)
                    {
                        case ActivityStatus.Online:
                            {
                                _onlineFriends.Add(friendObj);
                            }
                            break;
                        case ActivityStatus.Offline:
                            {
                                _offlineFriends.Add(friendObj);
                            }
                            break;
                        case ActivityStatus.InGame:
                            {
                                _inGameFriends.Add(friendObj);
                            }
                            break;
                        default:
                            {
                                _offlineFriends.Add(friendObj);
                            }
                            break;
                    }
                }
            });
        }


        private void OnSearchBoxTextChanged(string value)
        {
            if (AllFriends.View == null || InGameFriends.View == null || OnlineFriends == null || OfflineFriends == null)
                return;

            value ??= string.Empty;

            AllFriends.View.Filter = (i => (i as Friend).Name.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            InGameFriends.View.Filter = (i => (i as Friend).Name.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            OnlineFriends.View.Filter = (i => (i as Friend).Name.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            OfflineFriends.View.Filter = (i => (i as Friend).Name.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }


        #endregion Private Methods
    }

    public class FriendsViewModel : ViewModelBase
    {
        public FriendsModel Model { get; private set; }


        #region Commands


        private RelayCommand _viewProfileCommand;
        public ICommand ViewProfileCommand
        {
            get => RelayCommand.GetCommand<Friend>(ref _viewProfileCommand, (friend) =>
            {
                // Open Profile Page here.
                Process.Start($"https://night-world.org/users/{friend.Name}");
            });
        }

        private RelayCommand _unfriendCommand;
        public ICommand UnfriendCommand
        {
            get => RelayCommand.GetCommand<Friend>(ref _unfriendCommand, (friend) =>
            {
                friend.Unfriend();
                Model.UpdateRequestsData();
            });
        }


        #endregion Commands


        public FriendsViewModel()
        {
            App.Current.Dispatcher.Invoke(() => {
                Model = new FriendsModel();
            });
        }
    }
}
