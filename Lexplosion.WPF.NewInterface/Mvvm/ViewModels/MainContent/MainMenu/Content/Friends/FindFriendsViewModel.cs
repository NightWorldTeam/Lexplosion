using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FindFriendsViewModel : ViewModelBase, IRefreshable
    {
        public FindFriendsModel Model { get; }


        #region Commands


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
            get => RelayCommand.GetCommand(ref _searchCommand, (obj) => Model.LoadUsersList(obj as string, reboot: string.IsNullOrEmpty(obj as string), isClear: true));
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


        #endregion Commands


        public FindFriendsViewModel()
        {
            Model = new FindFriendsModel();
        }

        public void Refresh()
        {
            
        }
    }
}
