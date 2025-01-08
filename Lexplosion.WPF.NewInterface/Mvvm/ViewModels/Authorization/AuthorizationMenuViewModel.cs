using Lexplosion.Logic.Management.Accounts;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public sealed class AuthorizationMenuViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenu;

        private readonly ViewModelBase _microsoft;
        private readonly ViewModelBase _nightWorld;
        private readonly ViewModelBase _withoutAccount;

        public AuthorizationMenuModel Model { get; }


        #region Constructors


        public AuthorizationMenuViewModel(INavigationStore navigationStore, ICommand toMainMenu)
        {
            _navigationStore = navigationStore;
            _toMainMenu = toMainMenu;

            Account.AccountAdded += (account) => 
            {
                toMainMenu.Execute(null);
            };

            var toNightWorldForm = new NavigateCommand<ViewModelBase>(navigationStore, () => new NightWorldAuthorizationViewModel());
            var toMicrosoftForm = new NavigateCommand<ViewModelBase>(navigationStore, () => null);
            var toForm = new NavigateCommand<ViewModelBase>(navigationStore, () => new NightWorldAuthorizationViewModel());

            Model = new AuthorizationMenuModel(toNightWorldForm, toMicrosoftForm, toForm);
        }


        #endregion Constructors
    }
}
