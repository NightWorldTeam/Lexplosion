using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public sealed class AuthorizationMenuViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly ICommand _toMainMenu;

        private readonly ViewModelBase _microsoft;
        private readonly ViewModelBase _nightWorld;
        private readonly ViewModelBase _withoutAccount;

        public AuthorizationMenuModel Model { get; }


        #region Constructors


        public AuthorizationMenuViewModel(AppCore appCore, ICommand toMainMenu)
        {
            _appCore = appCore;
            _toMainMenu = toMainMenu;

            Account.AccountAdded += (account) => 
            {
                toMainMenu.Execute(null);
            };

            var toNightWorldForm = new NavigateCommand<ViewModelBase>(_appCore.NavigationStore, () => new NightWorldAuthorizationViewModel(appCore));
            var toMicrosoftForm = new NavigateCommand<ViewModelBase>(_appCore.NavigationStore, () => null);
            var toForm = new NavigateCommand<ViewModelBase>(_appCore.NavigationStore, () => null);

            Model = new AuthorizationMenuModel(toNightWorldForm, toMicrosoftForm, toForm);
        }


        #endregion Constructors
    }
}
