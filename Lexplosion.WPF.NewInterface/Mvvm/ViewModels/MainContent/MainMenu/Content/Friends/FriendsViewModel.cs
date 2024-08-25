using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.Friends;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class FriendsViewModel : ViewModelBase
    {
        public FriendsModel Model { get; private set; }


        #region Commands


        private RelayCommand _viewProfileCommand;
        public ICommand ViewProfileCommand
        {
            get => RelayCommand.GetCommand<Friend>(ref _viewProfileCommand, (friend) =>
            {
                // TODO: Open Profile Page here.
                Process.Start($"https://night-world.org/users/{friend.Name}");
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
            App.Current.Dispatcher.Invoke(() => {
                Model = new FriendsModel();
            });
        }
    }
}
