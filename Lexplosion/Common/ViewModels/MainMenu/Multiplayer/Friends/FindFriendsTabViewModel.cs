using Lexplosion.Common.Models.Objects;
using Lexplosion.Controls;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer.Friends
{
    public class FindFrinedsTabModel : VMBase
    {
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


        private uint _nextUsersCatalogPageIndex = 0;
        public uint NextUsersCatalogPageIndex
        {
            get => _nextUsersCatalogPageIndex; private set
            {
                _nextUsersCatalogPageIndex = value;
                OnPropertyChanged();
            }
        }


        public Action<string, bool> SetNewFilterValueAction => ExecuteSearchFilterValue; 

        public string CurrentFilterString { get; private set; } = string.Empty;
        private string _lastFilterString = string.Empty;


        #region Contructors


        public FindFrinedsTabModel()
        {
            MoveNextUserCatalogPage();
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void ExecuteSearchFilterValue(string newValue, bool _ = false) 
        {
            if (_lastFilterString != newValue) {
                _lastFilterString = newValue;
                NextUsersCatalogPageIndex = 0;
                CurrentFilterString = newValue;
                MoveNextUserCatalogPage(CurrentFilterString);
            }
        }


        public void MoveNextUserCatalogPage(string filterString = "")
        {
            Users.Clear();
            CurrentUsersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, NextUsersCatalogPageIndex, filterString);
            foreach (var user in CurrentUsersCatalogPage.Data)
            {
                Users.Add(new NWUserWrapper(user));
            }
            NextUsersCatalogPageIndex++;
        }

        public void MovePrevUserCatalogPage(string filterString = "")
        {
            Users.Clear();
            NextUsersCatalogPageIndex--;
            CurrentUsersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, NextUsersCatalogPageIndex - 1, filterString);
            foreach (var user in CurrentUsersCatalogPage.Data)
            {
                Users.Add(new NWUserWrapper(user));
            }
        }


        #endregion Public & Protected Methods
    }

    public class FindFriendsTabViewModel : VMBase, INotifiable
    {
        public event Action<NWUserWrapper> FriendRequestSended;


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
                Model.MovePrevUserCatalogPage(Model.CurrentFilterString);
            }));
        }


        private RelayCommand _moveNextPageCommand;
        public RelayCommand MoveNextPageCommand
        {
            get => _moveNextPageCommand ?? (_moveNextPageCommand = new RelayCommand(obj =>
            {
                Model.MoveNextUserCatalogPage(Model.CurrentFilterString);
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
                DoNotification("Friends Changed", user.Login + " requests was send(", 5, 0);
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
