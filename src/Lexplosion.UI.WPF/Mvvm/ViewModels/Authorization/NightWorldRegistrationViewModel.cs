using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Authorization;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Authorization
{
    public class NightWorldRegistrationViewModel : ViewModelBase
    {
        public NightWorldRegistrationModel Model { get; }


        #region Commands


        private RelayCommand _signUpCommand;
        public ICommand SignUpCommand
        {
            get => RelayCommand.GetCommand(ref _signUpCommand, Model.Register);
        }

        public ICommand ToAuthMenu { get; }
        public ICommand ToSignInCommand { get; }

        #endregion Commands


        public NightWorldRegistrationViewModel(AppCore appCore, ICommand toAuthMenu, Action<Type> navigateTo)
        {
            Model = new(appCore);
            ToAuthMenu = toAuthMenu;
            ToSignInCommand = new RelayCommand(((obj) => navigateTo(typeof(NightWorldAuthorizationViewModel))));
        }
    }
}
