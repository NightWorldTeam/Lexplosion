using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MultiplayerLayoutViewModel : ContentLayoutViewModelBase, ILimitedAccess
    {
        private ViewModelBase _generalMultiplayerViewModel = new MultiplayerViewModel();


        #region Properties


        private bool _hasAccess;
        public bool HasAccess 
        { 
            get => _hasAccess; set 
            {
                _hasAccess = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        public MultiplayerLayoutViewModel() : base()
        {
            Account.ActiveAccountChanged += (acc) => RefreshAccessData();
            HasAccess = Account.ActiveAccount?.AccountType == AccountType.NightWorld;
            OnAccessChanged();
        }


        #endregion Constructors


        #region Public & Properties Methods


        public void RefreshAccessData()
        {
            HasAccess = Account.ActiveAccount?.AccountType == AccountType.NightWorld;
            App.Current.Dispatcher.Invoke(() => 
            { 
                if (!HasAccess) 
                {
                    _tabs.Clear();
                    _generalMultiplayerViewModel = null;
                    return;
                }

                OnAccessChanged();
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void OnAccessChanged() 
        {
            if (HasAccess) 
            {
                _tabs.Clear();
                _generalMultiplayerViewModel = new MultiplayerViewModel();
                _tabs.Add(new TabItemModel { TextKey = "General", Content = _generalMultiplayerViewModel, IsSelected = true });
            }
        }


        #endregion Private Methods
    }
}
