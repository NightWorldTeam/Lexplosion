using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FindFriendsModel : ViewModelBase 
    {
        private CancellationTokenSource _cancellationToken = new();


        #region Properties


        private ObservableCollection<NightWorldUser> _users = new();
        /// <summary>
        /// Список пользователей на странице.
        /// </summary>
        public IEnumerable<NightWorldUser> Users { get => _users; }

        /// <summary>
        /// Загружается ли страница.
        /// </summary>
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Пустая ли коллекция пользователей.
        /// </summary>
        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty; set 
            {
                _isEmpty = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Существует ли следующая страница.
        /// </summary>
        public bool IsNextPageExist { get; private set; }
        /// <summary>
        /// Существует ли предыдущая страница.
        /// </summary>
        public bool IsPrevPageExist { get; private set; }

        /// <summary>
        /// Индекс выбранной страницы.
        /// </summary>
        private int _currentPageIndex = 1;
        public int CurrentPageIndex 
        { 
            get => _currentPageIndex; private set 
            {
                _currentPageIndex = value;
                //Console.WriteLine(_currentPageIndex);
                OnPropertyChanged();
            }
        }


        private UsersCatalogPage _usersCatalogPage;


        #endregion Properties


        #region Constructors


        public FindFriendsModel()
        {
            LoadUsersList();
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Перемещает на следущую страницу, если она существует.
        /// </summary>
        public void MoveToNextPage() 
        {
            if (IsNextPageExist && !IsLoading)
            {
                CurrentPageIndex++;
                LoadUsersList(isClear: true);
            }
        }

        /// <summary>
        /// Перемещает на предыдущую страницу, если она существует.
        /// </summary>
        public void MoveToPrevPage() 
        {
            if (IsPrevPageExist && !IsLoading)
            {
                CurrentPageIndex--;
                LoadUsersList(isClear: true);
            }
        }

        /// <summary>
        /// Отправляет запрос пользователя в друзья.
        /// </summary>
        public void SendFriendRequest(NightWorldUser user) 
        {
            user.SendFriendRequest();
            //DoNotification(ResourceGetter.GetString("friendsChanged"), user.Login + " " + ResourceGetter.GetString("requestsWasSend"), 5, 0);
        }

        /// <summary>
        /// Отменяет запрос пользователя в друзья.
        /// </summary>
        public void CancelFriendRequest(NightWorldUser user) 
        {
            user.CancelFriendRequest();
            //DoNotification(ResourceGetter.GetString("friendsChanged"), user.Login + " " + ResourceGetter.GetString("requestsWasCancel"), 5, 0);
        }


        #endregion Public Methods


        #region Private Methods


        private void UpdateIsPageExistsProperties()
        {
            IsNextPageExist = _usersCatalogPage.NextPage;
            OnPropertyChanged(nameof(IsNextPageExist));

            IsPrevPageExist = CurrentPageIndex != 1;
            OnPropertyChanged(nameof(IsPrevPageExist));
        }


        public void LoadUsersList(string searchFilter = null, bool reboot = false, bool isClear = false) 
        {
            IsLoading = true;
            
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();

            if (isClear)
                _users.Clear();

            ThreadPool.QueueUserWorkItem((obj) =>
            {
                if (reboot) 
                {
                    CurrentPageIndex = 1;
                    _usersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, 0, string.Empty);
                }
                else 
                {
                    _usersCatalogPage = NightWorldApi.FindUsers(GlobalData.User.UUID, GlobalData.User.SessionToken, (uint)CurrentPageIndex - 1, searchFilter);
                }

                App.Current.Dispatcher.Invoke(() => 
                {
                    UpdateIsPageExistsProperties();

                    IsEmpty = _usersCatalogPage.Data.Count == 0;

                    foreach (var user in _usersCatalogPage.Data) 
                    {
                        _users.Add(new NightWorldUser(user));
                    }
                });

                Thread.Sleep(250);
                IsLoading = false;
            });
        }


        #endregion Private Methods
    }


    public sealed class FindFriendsViewModel : ViewModelBase
    {
        public FindFriendsModel Model { get; }


        #region Properties


        /// <summary>
        /// Перемещает на следующую страницу, если он существует.
        /// </summary>
        private RelayCommand _movePrevPageCommand;
        public ICommand MovePrevPageCommand
        {
            get => RelayCommand.GetCommand(ref _movePrevPageCommand, Model.MoveToPrevPage);
        }

        /// <summary>
        /// Перемещает на предыдущую страницу, если она существует.
        /// </summary>
        private RelayCommand _moveNextPageCommand;
        public ICommand MoveNextPageCommand
        {
            get => RelayCommand.GetCommand(ref _moveNextPageCommand, Model.MoveToNextPage);
        }

        private RelayCommand _searchCommand;
        public ICommand SearchCommand 
        {
            get => RelayCommand.GetCommand(ref _searchCommand, (obj) => Model.LoadUsersList(obj as string, reboot: (obj as string) == ""));
        }

        /// <summary>
        /// Отправляет запрос пользователя в друзья.
        /// </summary>
        private RelayCommand _sendFriendRequestCommand;
        public ICommand SendFriendRequestCommand
        {
            get => RelayCommand.GetCommand<NightWorldUser>(ref _sendFriendRequestCommand, Model.SendFriendRequest);
        }

        /// <summary>
        /// Отменяет запрос пользователя в друзья.
        /// </summary>
        private RelayCommand _cancelFriendRequestCommand;
        public ICommand CancelFriendRequestCommand
        {
            get => RelayCommand.GetCommand<NightWorldUser>(ref _cancelFriendRequestCommand, Model.CancelFriendRequest);
        }


        #endregion Properties


        public FindFriendsViewModel()
        {
            Model = new FindFriendsModel();
        }
    }
}
