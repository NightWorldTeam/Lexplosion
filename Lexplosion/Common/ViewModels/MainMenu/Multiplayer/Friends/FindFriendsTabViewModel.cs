using Lexplosion.Common.Models.Objects;
using Lexplosion.Controls;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Tools;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer.Friends
{
    public class FindFrinedsTabModel : VMBase
    {
        public bool IsLoading { get; private set; }

        public ObservableCollection<NWUserWrapper> Users { get; } = new ObservableCollection<NWUserWrapper>();
        
        private UsersCatalogPage _usersCatalogPage;
        public UsersCatalogPage CurrentUsersCatalogPage 
        {
            get => _usersCatalogPage; private set 
            {
                _usersCatalogPage = value;
                OnPropertyChanged();
            }
        }

        //private bool _isNextPagesExists
        public bool IsNextPageExist { get; set; }
        public bool IsPrevPageExist { get; set; }


        private int _currentUsersCatalogPageIndex = 0;
        public int CurrentUserCatalogPageIndex
        {
            get => _currentUsersCatalogPageIndex; private set
            {
                _currentUsersCatalogPageIndex = value;
                OnPropertyChanged();
            }
        }

        public Action<string, bool> SetNewFilterValueAction => ExecuteSearchFilterValue; 

        public string CurrentFilterString { get; private set; } = string.Empty;
        private string _lastFilterString = string.Empty;


        #region Contructors


        public FindFrinedsTabModel()
        {
            Users.Clear();
            CurrentUsersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, (uint)CurrentUserCatalogPageIndex, "");

            UpdateIsPageExistsProperties();

            foreach (var user in CurrentUsersCatalogPage.Data)
            {
                Users.Add(new NWUserWrapper(user));
            }
            //MoveNextUserCatalogPage();
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void ExecuteSearchFilterValue(string newValue, bool _ = false) 
        {
            if (_lastFilterString != newValue) {
                _lastFilterString = newValue;
                CurrentUserCatalogPageIndex = 0;
                CurrentFilterString = newValue;
                MoveNextUserCatalogPage(CurrentFilterString);
            }
        }


        public void MoveNextUserCatalogPage(string filterString = "")
        {
            IsLoading = true;
            Users.Clear();
            CurrentUserCatalogPageIndex++;
            CurrentUsersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, (uint)CurrentUserCatalogPageIndex, filterString);

            UpdateIsPageExistsProperties();

            foreach (var user in CurrentUsersCatalogPage.Data)
            {
                Users.Add(new NWUserWrapper(user));
            }
            IsLoading = false;
        }

        public void MovePrevUserCatalogPage(string filterString = "")
        {
            IsLoading = true;
            Users.Clear();
            CurrentUserCatalogPageIndex--;
            CurrentUsersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, (uint)CurrentUserCatalogPageIndex, filterString);

            UpdateIsPageExistsProperties();

            foreach (var user in CurrentUsersCatalogPage.Data)
            {
                Users.Add(new NWUserWrapper(user));
            }
            IsLoading = false;
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void UpdateIsPageExistsProperties() 
        {
            IsNextPageExist = CurrentUsersCatalogPage.NextPage;
            OnPropertyChanged(nameof(IsNextPageExist));

            IsPrevPageExist = CurrentUserCatalogPageIndex != 0 && CurrentUserCatalogPageIndex - 1 > -1;
            OnPropertyChanged(nameof(IsPrevPageExist));
        }


        #endregion Private Methods
    }

    public class FindFriendsTabViewModel : VMBase, INotifiable
    {
#pragma warning disable CS0067 // Событие "FindFriendsTabViewModel.FriendRequestSended" никогда не используется.
        public event Action<NWUserWrapper> FriendRequestSended;
#pragma warning restore CS0067 // Событие "FindFriendsTabViewModel.FriendRequestSended" никогда не используется.


        private DoNotificationCallback _doNotification;
        public DoNotificationCallback DoNotification
        {
            get => _doNotification; protected set
            {
                _doNotification = value ?? ((header, message, time, type) => { });
            }
        }


        public FindFrinedsTabModel Model { get; }


        #region Commands


        private RelayCommand _movePrevPageCommand;
        public RelayCommand MovePrevPageCommand
        {
            get => _movePrevPageCommand ?? (_movePrevPageCommand = new RelayCommand(obj =>
            {
                if (Model.IsPrevPageExist && !Model.IsLoading)
                {
                    Model.MovePrevUserCatalogPage(Model.CurrentFilterString);
                }
            }));
        }


        private RelayCommand _moveNextPageCommand;
        public RelayCommand MoveNextPageCommand
        {
            get => _moveNextPageCommand ?? (_moveNextPageCommand = new RelayCommand(obj =>
            {
                if (Model.IsNextPageExist && !Model.IsLoading) 
                {
                    Model.MoveNextUserCatalogPage(Model.CurrentFilterString);
                }
            }));
        }

        private RelayCommand _sendFriendRequestCommand;
        public ICommand SendFriendRequestCommand 
        {
            get => _sendFriendRequestCommand ?? (_sendFriendRequestCommand = new RelayCommand(obj => 
            {
                var user = (NWUserWrapper)obj;
                NightWorldApi.AddFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, user.Login);
                user.IsSendFriendRequests = true;
                user.ExecuteOnPropertiesChanged();
                DoNotification(ResourceGetter.GetString("friendsChanged"), user.Login + " " + ResourceGetter.GetString("requestsWasSend"), 5, 0);
            }));
        }


        private RelayCommand _cancelFriendRequestCommand;
        public ICommand CancelFriendRequestCommand 
        {
            get => _cancelFriendRequestCommand ?? (_cancelFriendRequestCommand = new RelayCommand(obj =>
            {
                var user = (NWUserWrapper)obj;
                NightWorldApi.RemoveFriend(GlobalData.User.UUID, GlobalData.User.SessionToken, user.Login);
                user.IsSendFriendRequests = false;
                user.ExecuteOnPropertiesChanged();
                DoNotification(ResourceGetter.GetString("friendsChanged"), user.Login + " " + ResourceGetter.GetString("requestsWasCancel"), 5, 0);
            }));
        }


        #endregion Commands


        public FindFriendsTabViewModel(DoNotificationCallback doNotification)
        {
            DoNotification = doNotification ?? DoNotification;
            Model = new FindFrinedsTabModel();
        }
    }
}
