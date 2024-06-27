using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class FriendsLayoutViewModel : ContentLayoutViewModelBase, ILimitedAccess
    {
        private ViewModelBase _friendsViewModel;
        private ViewModelBase _friendsRequestsViewModel;
        private ViewModelBase _findFriendsViewModel;


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


        public FriendsLayoutViewModel() : base()
        {
            Account.ActiveAccountChanged += (acc) => RefreshAccessData();
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
                _friendsRequestsViewModel = new FriendRequestsViewModel();
                _findFriendsViewModel = new FindFriendsViewModel();
                _tabs.Add(new TabItemModel { TextKey = "Friends", Content = _friendsViewModel, IsSelected = true });
                _tabs.Add(new TabItemModel { TextKey = "FriendsRequests", Content = _friendsRequestsViewModel });
                _tabs.Add(new TabItemModel { TextKey = "FindFriends", Content = _findFriendsViewModel });
            }
        }


        #endregion Private Methods
    }
}
