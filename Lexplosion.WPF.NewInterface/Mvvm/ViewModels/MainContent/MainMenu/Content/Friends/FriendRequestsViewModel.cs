using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FriendRequestsModel : ViewModelBase 
    {
        public event Action<NightWorldUserBase> FriendAdded;


        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private ObservableCollection<object> _outgoing = new ObservableCollection<object>();
        private ObservableCollection<object> _incoming = new ObservableCollection<object>();

        public CollectionViewSource Incoming { get; } = new();
        public CollectionViewSource Outgoing { get; } = new();

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


        public FriendRequestsModel()
        {
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
                var friendRequests = NightWorldApi.GetFriendRequests(activeAccount.UUID, activeAccount.SessionToken);

                App.Current.Dispatcher?.Invoke(() =>
                {
                    UpdateFriendRequestsOutcoming(friendRequests.Outgoing);
                    UpdateFriendRequestsIncoming(friendRequests.Incoming);
                });
            });
        }


        public void AddFriend(NightWorldUserRequest friend) 
        {
            friend.AddFriend();
            UpdateRequestsData();
            FriendAdded?.Invoke(friend);
        }

        public void DeclineFriend(NightWorldUserRequest friend) 
        {
            friend.DeclineFriend();
            UpdateRequestsData();
        }

        #endregion Publuc & Protected Methods


        #region Private Methods


        private void OnSearchBoxTextChanged(string value)
        {
            if (Outgoing.View == null || Incoming.View == null)
                return;

            value ??= string.Empty;

            Outgoing.View.Filter = (i => (i as NightWorldUserBase).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            Incoming.View.Filter = (i => (i as NightWorldUserBase).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
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

    public sealed class FriendRequestsViewModel : ViewModelBase, IRefreshable
    {
        public FriendRequestsModel Model { get; }



        #region Command


        private RelayCommand _declineFriendRequestCommand;
        public ICommand DeclineFriendRequestCommand
        {
            get => RelayCommand.GetCommand<NightWorldUserRequest>(ref _declineFriendRequestCommand, friend =>
            {
                Model.DeclineFriend(friend);
                // TODO: Friends Translate
            });
        }

        /// <summary>
        /// Добавление друга, в качестве агрумента obj, будет передаваться ссылка на объект друга.
        /// </summary>
        private RelayCommand _addFriendCommand;
        public ICommand AddFriendCommand
        {
            get => RelayCommand.GetCommand<NightWorldUserRequest>(ref _addFriendCommand, friend =>
            {
                Model.AddFriend(friend);
                // TODO: Friends Translate
            });
        }


        #endregion Command


        public FriendRequestsViewModel()
        {
            Model = new FriendRequestsModel();
        }

        public void Refresh()
        {
            Model.UpdateRequestsData();
        }
    }
}
