using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MultiplayerLayoutViewModel : ContentLayoutViewModelBase, ILimitedAccess
    {
        private ViewModelBase _generalMultiplayerViewModel = new MultiplayerViewModel();
        private readonly Action _openAccountFactory;

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

        private RelayCommand _authorzation;
        public ICommand Authorzation
        {
            get => RelayCommand.GetCommand(ref _authorzation, () =>
            {
                _openAccountFactory?.Invoke();
            });
        }

        #endregion Properties


        #region Constructors


        public MultiplayerLayoutViewModel(Action openAccountFactory) : base()
        {
            _openAccountFactory = openAccountFactory;
            Account.ActiveAccountChanged += (acc) =>
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        RefreshAccessData();
                    });
                });
            };

            HasAccess = Account.ActiveAccount?.AccountType == AccountType.NightWorld;
            OnAccessChanged();
        }


        #endregion Constructors


        #region Public & Properties Methods


        public void RefreshAccessData()
        {
            HasAccess = Account.ActiveAccount?.AccountType == AccountType.NightWorld;
            if (!HasAccess)
            {
                _tabs.Clear();
                _generalMultiplayerViewModel = null;
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
                _generalMultiplayerViewModel = new MultiplayerViewModel();
                _tabs.Add(new TabItemModel { TextKey = "General", Content = _generalMultiplayerViewModel, IsSelected = true });
            }
        }


        #endregion Private Methods
    }
}
