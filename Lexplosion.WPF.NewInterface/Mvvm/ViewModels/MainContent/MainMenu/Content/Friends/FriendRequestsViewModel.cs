using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
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
                // TODO: Notification
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
                // TODO: Notification
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
