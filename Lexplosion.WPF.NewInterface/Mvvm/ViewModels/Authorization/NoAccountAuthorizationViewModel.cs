using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public class NoAccountAuthorizationViewModel : ViewModelBase
    {
        public NoAccountAuthorizationModel Model { get; }

        public ICommand ToNightWorldCommand { get; }
        public ICommand ToMicrosoftCommand { get; }

        public ICommand LoginCommand { get; }

        public NoAccountAuthorizationViewModel(AppCore appCore, Action<Type> navigateTo) 
        {
            ToNightWorldCommand = new RelayCommand(obj => navigateTo(typeof(NightWorldAuthorizationViewModel)));
            ToMicrosoftCommand = new RelayCommand(obj => navigateTo(typeof(MicrosoftAuthorizationViewModel)));

            Model = new();

            LoginCommand = new RelayCommand(obj => Model.LogIn());
        }
    }
}
