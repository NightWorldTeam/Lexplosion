using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class FriendsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _friendsViewModel = new FriendsViewModel();
        private readonly ViewModelBase _friendsRequestsViewModel = new FriendsRequestsViewModel();
        private readonly ViewModelBase _findFriendsViewModel = new FindFriendsViewModel();

        public FriendsLayoutViewModel() : base()
        {
            InitDefaultTabMenu();
        }

        private void InitDefaultTabMenu()
        {
            _tabs.Add(new TabItemModel { TextKey = "Friends", Content = _friendsViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "FriendsRequests", Content = _friendsRequestsViewModel });
            _tabs.Add(new TabItemModel { TextKey = "FindFriends", Content = _findFriendsViewModel });
        }
    }
}
