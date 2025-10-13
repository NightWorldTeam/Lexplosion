using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Authorization;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Authorization
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
