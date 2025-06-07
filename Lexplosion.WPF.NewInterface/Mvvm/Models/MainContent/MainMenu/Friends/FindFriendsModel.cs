using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Network;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends
{
    public sealed class FindFriendsModel : ViewModelBase
    {
        public event Action<NightWorldUser> FriendRequestSent;

        private readonly AppCore _appCore;
        private CancellationTokenSource _cancellationToken = new();


        #region Properties


        private ObservableCollection<NightWorldUser> _users = new();
        /// <summary>
        /// Список пользователей на странице.
        /// </summary>
        public FiltableObservableCollection Users { get; } = [];


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
        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get => _currentPageIndex; private set
            {
                _currentPageIndex = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Количество страниц
        /// </summary>
        public int PageCount { get; private set; }

        private UsersCatalogPage _usersCatalogPage;


        #endregion Properties


        #region Constructors


        public FindFriendsModel(AppCore appCore)
        {
            _appCore = appCore;
            Users.Source = _users;
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
            Runtime.TaskRun(() =>
            {
                user.SendFriendRequest();
                _appCore.UIThread(() => 
                {
                    FriendRequestSent?.Invoke(user);
                    _appCore.MessageService.Success("FriendRequestHasBeenSent", true);
                });
            });
        }

        /// <summary>
        /// Отменяет запрос пользователя в друзья.
        /// </summary>
        public void CancelFriendRequest(NightWorldUser user)
        {
            Runtime.TaskRun(() =>
            {
                user.CancelFriendRequest();
                _appCore.UIThread(() =>
                {
                    FriendRequestSent?.Invoke(user);
                    _appCore.MessageService.Info("FriendRequestWasCancelled", true);
                });
            });
        }


        #endregion Public Methods


        #region Private Methods


        private void UpdateIsPageExistsProperties()
        {
            IsNextPageExist = _usersCatalogPage.NextPage;
            OnPropertyChanged(nameof(IsNextPageExist));

            IsPrevPageExist = CurrentPageIndex != 0;
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
                var services = Runtime.ServicesContainer;

                var activeAccount = Account.ActiveAccount;
                if (reboot)
                {
                    CurrentPageIndex = 0;
                    _usersCatalogPage = services.NwApi.FindUsers(activeAccount.UUID, activeAccount.SessionToken, 0, string.Empty);
                }
                else
                {
                    _usersCatalogPage = services.NwApi.FindUsers(activeAccount.UUID, activeAccount.SessionToken, (uint)CurrentPageIndex, searchFilter ?? string.Empty);
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
                PageCount = _usersCatalogPage.PagesCount;
                OnPropertyChanged(nameof(PageCount));
                Thread.Sleep(250);
                IsLoading = false;
            });
        }


        #endregion Private Methods
    }
}
