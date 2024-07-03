using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FriendRequestsModel : ViewModelBase 
    {
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


        #endregion Publuc & Protected Methods


        #region Private Methods


        private void OnSearchBoxTextChanged(string value)
        {
            if (Outgoing.View == null || Incoming.View == null)
                return;

            value ??= string.Empty;

            Outgoing.View.Filter = (i => (i as NightWorldUser).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            Incoming.View.Filter = (i => (i as NightWorldUser).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }



        private void UpdateFriendRequestsOutcoming(IEnumerable<NwUser> outgoingRequests)
        {
            _outgoing.Clear();
            foreach (var friendRequest in outgoingRequests)
            {
                _outgoing.Add(new NightWorldUser(friendRequest));
            }
        }

        private void UpdateFriendRequestsIncoming(IEnumerable<NwUser> incomingRequests)
        {
            _incoming.Clear();
            foreach (var friendRequest in incomingRequests)
            {
                _incoming.Add(new NightWorldUser(friendRequest));
            }
        }


        #endregion Private Methods
    }

    public sealed class FriendRequestsViewModel : ViewModelBase
    {
        public FriendRequestsModel Model { get; }



        #region Command


        private RelayCommand _cancelFriendRequestCommand;
        public ICommand CancelFriendRequestCommand
        {
            get => RelayCommand.GetCommand<Friend>(ref _cancelFriendRequestCommand, friend =>
            {
                NightWorldApi.RemoveFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, friend.Name);
                Model.UpdateRequestsData();
                // TODO: Friends Translate
            });
        }

        /// <summary>
        /// Добавление друга, в качестве агрумента obj, будет передаваться ссылка на объект друга.
        /// </summary>
        private RelayCommand _addFriendCommand;
        public ICommand AddFriendCommand
        {
            get => RelayCommand.GetCommand<NightWorldUser>(ref _addFriendCommand, friend =>
            {
                NightWorldApi.AddFriend(Account.ActiveAccount.UUID, Account.ActiveAccount.SessionToken, friend.Login);
                Model.UpdateRequestsData();
                // TODO: Friends Translate
            });
        }


        #endregion Command


        public FriendRequestsViewModel()
        {
            Model = new FriendRequestsModel();
        }
    }
}
