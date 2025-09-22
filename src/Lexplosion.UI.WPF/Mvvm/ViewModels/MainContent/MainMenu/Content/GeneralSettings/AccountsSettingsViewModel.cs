using Lexplosion.Logic.Management.Accounts;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.MainMenu.GeneralSettings;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AccountsSettingsViewModel : ViewModelBase
    {
        private readonly AppCore _appCore;


        public AccountsSettingsModel Model { get; }


        #region Commands


        private RelayCommand _activateAccountCommand;
        public ICommand ActivateAccountCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _activateAccountCommand, Model.ActivateAccount);
        }


        private RelayCommand _doAccountLauncherCommand;
        public ICommand DoAccountLauncherCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _doAccountLauncherCommand, Model.DoAccountLauncherCommand);
        }


        private RelayCommand _singOutCommand;
        public ICommand SingOutCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _singOutCommand, Model.SignOut);
        }

        private RelayCommand _singOutAllCommand;
        public ICommand SingOutAllCommand
        {
            get => RelayCommand.GetCommand(ref _singOutAllCommand, Model.SingOutAllAccounts);
        }


        private RelayCommand _reauthAccountCommand;
        public ICommand ReauthAccountCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _reauthAccountCommand, Model.ReauthAccount); 
        }


        private RelayCommand _addAccountCommand;
        public ICommand OpenAccountFactoryCommand {
            get => RelayCommand.GetCommand(ref _addAccountCommand, () => 
            {
                _appCore.ModalNavigationStore.Open(new AccountFactoryViewModel(_appCore));
            });
        }


        #endregion Commands


        public AccountsSettingsViewModel(AppCore appCore)
        {
            _appCore = appCore;
            Model = new AccountsSettingsModel(appCore);
        }
    }
}


