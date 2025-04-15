using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public sealed class AuthorizationMenuViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;
        private readonly Dictionary<Type, Action> _navigationByType = new();
        
     
        public AuthorizationMenuModel Model { get; }


        #region Commands


        public ICommand ToRegisterCommand { get; }


        #endregion Commands


        #region Constructors


        public AuthorizationMenuViewModel(AppCore appCore, ICommand toMainMenu)
        {
            _appCore = appCore;

            Account.AccountAdded += (account) => 
            {
                toMainMenu.Execute(null);
            };

            var backCommand = _appCore.BuildNavigationCommand(this);

            ToRegisterCommand = _appCore.BuildNavigationCommand(new NightWorldRegistrationViewModel(appCore, backCommand, NavigateTo));

            var toNightWorldForm = _appCore.BuildNavigationCommand(new NightWorldAuthorizationViewModel(appCore, NavigateTo, ToRegisterCommand));
            var toMicrosoftForm = _appCore.BuildNavigationCommand(new MicrosoftAuthorizationViewModel(appCore, NavigateTo, backCommand), (vm) => vm.Model.LogIn());
            var toNoAccountForm = _appCore.BuildNavigationCommand(new NoAccountAuthorizationViewModel(appCore, NavigateTo));

            _navigationByType[typeof(NightWorldAuthorizationViewModel)] = () => toNightWorldForm?.Execute(null);
            _navigationByType[typeof(MicrosoftAuthorizationViewModel)] = () => toMicrosoftForm?.Execute(null);
            _navigationByType[typeof(NoAccountAuthorizationViewModel)] = () => toNoAccountForm?.Execute(null);

            Model = new AuthorizationMenuModel(toNightWorldForm, toMicrosoftForm, toNoAccountForm);
        }


        #endregion Constructors


        void NavigateTo(Type type) 
        {
            if (_navigationByType.TryGetValue(type, out var navigate)) 
            {
                navigate();
            }
        }
    }
}
