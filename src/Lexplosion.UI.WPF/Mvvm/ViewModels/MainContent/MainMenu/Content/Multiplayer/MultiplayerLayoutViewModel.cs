using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Args;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public sealed class MultiplayerLayoutViewModel : ContentLayoutViewModelBase, ILimitedAccess
    {
        private readonly AppCore _appCore;
        private readonly Action _openAccountFactory;

        private ViewModelBase _generalMultiplayerViewModel;
        private ViewModelBase _adServersViewModel;

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


        public MultiplayerLayoutViewModel(AppCore appCore, ICommand backCommand, MultiplayerLayoutArgs multiplayerLayoutArgs) : base()
        {
            _appCore = appCore;
            _adServersViewModel = new AdServersViewModel(appCore, backCommand, multiplayerLayoutArgs.SelectInstanceForServerArgs);
            _openAccountFactory = multiplayerLayoutArgs.OpenAccountFactory;
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

            if (HasAccess) 
            {
                _generalMultiplayerViewModel = new MultiplayerViewModel(appCore);
            }
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
                _generalMultiplayerViewModel = new MultiplayerViewModel(_appCore);
                _tabs.Add(new TabItemModel { TextKey = "PartnerServers", Content = _adServersViewModel, IsSelected = true });
                _tabs.Add(new TabItemModel { TextKey = "General", Content = _generalMultiplayerViewModel, IsSelected = false });
            }
        }


        #endregion Private Methods
    }
}
