using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization.BasicAuthorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
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
