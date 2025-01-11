﻿using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization;
using Lexplosion.WPF.NewInterface.Stores;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public sealed class NightWorldAuthorizationViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenu;

        public IBasicAuthModel Model { get; }


        #region Commands


        private RelayCommand authorizationCommand;
        public ICommand AuthorizationCommand
        {
            get => authorizationCommand ?? (authorizationCommand = new RelayCommand(obj =>
            {
                Model.LogIn();
                _toMainMenu?.Execute(null);
            }));
        }


        private RelayCommand _changeAuthorizationFormCommand;
        public ICommand ChangeAuthorizationFormCommand
        {
            get => _changeAuthorizationFormCommand ?? (_changeAuthorizationFormCommand = new RelayCommand(obj =>
            {

            }));
        }


        private RelayCommand _passwordResetCommand;
        public ICommand PasswordResetCommand
        {
            get => _passwordResetCommand ?? (_passwordResetCommand = new RelayCommand(obj =>
            {
                _navigationStore.CurrentViewModel = new PasswordResetViewModel(_navigationStore);
            }));
        }


        #endregion Commands


        #region Constructors


        public NightWorldAuthorizationViewModel(AppCore appCore, string loadedLogin = "")
        {
            _navigationStore = appCore.NavigationStore;
            Model = new NightWorldAuthorizationModel(appCore, loadedLogin);
        }


        #endregion Constructors
    }
}
