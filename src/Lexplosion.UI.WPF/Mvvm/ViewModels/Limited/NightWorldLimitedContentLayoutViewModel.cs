using Lexplosion.Logic.Management.Accounts;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Limited
{
    public class NightWorldLimitedContentLayoutViewModel : LimitedContentLayoutViewModelBase
    {
        private readonly Action _toCreateAccount;
        private readonly Action _toAuthorization;

        public override bool HasAccess
        {
            get => Account.ActiveAccount != null && Account.ActiveAccount.AccountType == AccountType.NightWorld;
        }

        public bool IsModalTarget { get; }

        private RelayCommand toCreateAccountCommand;
        public ICommand ToCreateAccountCommand
        {
            get => RelayCommand.GetCommand(ref toCreateAccountCommand, _toCreateAccount);
        }

        private RelayCommand _toAuthorizationCommand;
        public ICommand ToAuthorizationCommand
        {
            get => RelayCommand.GetCommand(ref _toAuthorizationCommand, _toAuthorization);
        }

        public NightWorldLimitedContentLayoutViewModel(ILimitedAccess viewModelBase, Action toAuthorization, bool isModalTarget = false) : base(viewModelBase)
        {
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

            IsModalTarget = isModalTarget;

            _toCreateAccount = () => Process.Start("https://night-world.org/auth");
            _toAuthorization = toAuthorization;
        }


        public override void RefreshAccessData()
        {
            OnPropertyChanged(nameof(HasAccess));
            Content.RefreshAccessData();
        }
    }
}
