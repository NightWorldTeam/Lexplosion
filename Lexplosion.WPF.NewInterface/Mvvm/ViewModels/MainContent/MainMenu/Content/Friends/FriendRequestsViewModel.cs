using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;

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

        private void OnSearchBoxTextChanged(string value)
        {
            if (Outgoing.View == null || Incoming.View == null)
                return;

            value ??= string.Empty;

            Outgoing.View.Filter = (i => (i as NightWorldUser).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            Incoming.View.Filter = (i => (i as NightWorldUser).Login.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }


        #endregion Constructors


        #region Private Methods


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


        public FriendRequestsViewModel()
        {
            Model = new FriendRequestsModel();
        }
    }
}
