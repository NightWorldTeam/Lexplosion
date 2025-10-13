using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects.Users;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FriendsViewModel : ViewModelBase, IRefreshable
    {
        public FriendsModel Model { get; private set; }


        #region Commands


        private RelayCommand _viewProfileCommand;
        public ICommand ViewProfileCommand
        {
            get => RelayCommand.GetCommand<Friend>(ref _viewProfileCommand, (friend) =>
            {
                // TODO: Open Profile Page here.
                Process.Start($"https://night-world.org/users/{friend.Login}");
            });
        }

        private RelayCommand _unfriendCommand;
        public ICommand UnfriendCommand
        {
            get => RelayCommand.GetCommand<Friend>(ref _unfriendCommand, (friend) =>
            {
                friend.Unfriend();
                Model.UpdateRequestsData();
            });
        }


        #endregion Commands


        public FriendsViewModel()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Model = new FriendsModel();
            });
        }

        public void Refresh()
        {
            Model.UpdateRequestsData();
        }
    }
}
