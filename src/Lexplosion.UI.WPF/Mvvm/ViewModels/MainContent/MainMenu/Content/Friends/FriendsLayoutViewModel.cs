﻿using Lexplosion.Logic.Management.Accounts;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Core.ViewModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FriendsLayoutViewModel : ContentLayoutViewModelBase, ILimitedAccess
    {
        private readonly AppCore _appCore;

        private FriendsViewModel _friendsViewModel;
        private FriendRequestsViewModel _friendsRequestsViewModel;
        private FindFriendsViewModel _findFriendsViewModel;


        #region Properties


        private bool _isHasAccess;
        public bool HasAccess
        {
            get => _isHasAccess; set
            {
                _isHasAccess = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties


        #region Constructors


        public FriendsLayoutViewModel(AppCore appCore) : base()
        {
            _appCore = appCore;

            HasAccess = Account.ActiveAccount?.AccountType == AccountType.NightWorld;

            // TODO !!!IMPORTANT!!! не пытаться загрузить данные без аккаунта NW.
            OnAccessChanged();
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void RefreshAccessData()
        {
            HasAccess = Account.ActiveAccount?.AccountType == AccountType.NightWorld;
            if (!HasAccess) 
            {
                _tabs.Clear();
                _friendsViewModel = null;
                _friendsRequestsViewModel = null;
                _findFriendsViewModel = null;
                return;
            }

            OnAccessChanged();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void OnAccessChanged() 
        {
            if (HasAccess)
            {
                _tabs.Clear();

                _friendsViewModel = new FriendsViewModel();
                _friendsRequestsViewModel = new FriendRequestsViewModel(_appCore);
                _findFriendsViewModel = new FindFriendsViewModel(_appCore);

                _friendsViewModel.Model.Unfriended += (user) =>
                {
                    _friendsRequestsViewModel.Refresh();
                    _findFriendsViewModel.Refresh();
                };

                _friendsRequestsViewModel.Model.FriendAdded += (user) =>
                {
                    _friendsViewModel.Refresh();
                    _findFriendsViewModel.Refresh();
                };

                _findFriendsViewModel.Model.FriendRequestSent += (user) => 
                {
                    _friendsRequestsViewModel.Refresh();
                };


                _tabs.Add(new TabItemModel { TextKey = "Friends", Content = _friendsViewModel, IsSelected = true });
                _tabs.Add(new TabItemModel { TextKey = "FriendsRequests", Content = _friendsRequestsViewModel });
                _tabs.Add(new TabItemModel { TextKey = "FindFriends", Content = _findFriendsViewModel });
            }
        }


        #endregion Private Methods
    }
}
