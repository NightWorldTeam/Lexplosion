using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Authorization.OAuth2;
using System;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Authorization
{
    public sealed class MicrosoftAuthorizationViewModel : ViewModelBase
    {
        public MicrosoftAuthorizationModel Model { get; }

        public ICommand ToNightWorldCommand { get; }
        public ICommand ToNoAccountCommand { get; }

        public ICommand ManualInputCommand { get; }

        public ICommand CancelCommand { get; }


        public MicrosoftAuthorizationViewModel(AppCore appCore, Action<Type> navigateTo, ICommand backToMenu) 
        {
            ToNightWorldCommand = new RelayCommand((obj) => navigateTo(typeof(NightWorldAuthorizationViewModel)));
            ToNoAccountCommand = new RelayCommand((obj) => navigateTo(typeof(NoAccountAuthorizationViewModel)));

            CancelCommand = new RelayCommand((obj) =>
            {
                Model.Cancel();
                backToMenu.Execute(null);
            });

            Model = new(appCore);

            ManualInputCommand = new RelayCommand((obj) =>
            {
                var manualInputModal = new MicrosoftManualInputViewModel(appCore);
                manualInputModal.TokenEntered += Model.ManualInput;
                appCore.ModalNavigationStore.Open(manualInputModal);
            });
        }
    }
}
