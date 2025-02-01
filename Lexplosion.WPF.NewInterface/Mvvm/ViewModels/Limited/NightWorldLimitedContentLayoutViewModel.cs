using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Limited
{
    public class NightWorldLimitedContentLayoutViewModel : LimitedContentLayoutViewModelBase
    {
        public override bool HasAccess 
        { 
            get => Account.ActiveAccount != null && Account.ActiveAccount.AccountType == AccountType.NightWorld; 
        }

        public bool IsModalTarget { get; }


        public NightWorldLimitedContentLayoutViewModel(ILimitedAccess viewModelBase, bool isModalTarget = false) : base(viewModelBase)
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
        }


        public override void RefreshAccessData()
        {
            OnPropertyChanged(nameof(HasAccess));
            Content.RefreshAccessData();
        }
    }
}
