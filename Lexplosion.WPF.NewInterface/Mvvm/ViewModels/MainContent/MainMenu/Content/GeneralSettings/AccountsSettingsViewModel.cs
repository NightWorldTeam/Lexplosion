using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.MainMenu.GeneralSettings;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AccountsSettingsViewModel : ViewModelBase
    {
        private ModalNavigationStore _modalNavigationStore;

        public AccountsSettingsModel Model { get; }


        #region Commands


        private RelayCommand _activateAccountCommand;
        public ICommand ActivateAccountCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _activateAccountCommand, (acc) =>
            {
                if (!acc.IsAuthed)
                {
                    Runtime.TaskRun(() =>
                    {
                        var authResult = acc.Auth();
                        if (authResult == AuthCode.Successfully)
                        {
                            acc.IsActive = true;
                            Account.SaveAll();
                        }

                        Runtime.DebugWrite(acc.IsLaunch);
                    });

                    return;
                }

                acc.IsActive = true;
                Account.SaveAll();
            });
        }


        private RelayCommand _doAccountLauncherCommand;
        public ICommand DoAccountLauncherCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _doAccountLauncherCommand, (acc) =>
            {
                if (!acc.IsAuthed)
                {
                    Runtime.TaskRun(() => 
                    {
                        var authResult = acc.Auth();
                        if (authResult == AuthCode.Successfully) 
                        {
                            acc.IsLaunch = true;
                            Account.SaveAll();
                        }
                        Runtime.DebugWrite(acc.IsLaunch);
                    });

                    return;
                }

                acc.IsLaunch = true;
                Account.SaveAll();
            });
        }


        private RelayCommand _singOutCommand;
        public ICommand SingOutCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _singOutCommand, (acc) =>
            {
                Runtime.DebugWrite($"{acc.AccountType} {acc.Login} executed.");

                _modalNavigationStore.Open(new ConfirmActionViewModel("Удаление аккаунта", "ха-ха-ха", 
                    (obj) => 
                    {
                        Model.RemoveAccount(acc);
                        Account.SaveAll();
                    }));
            });
        }


        private RelayCommand _reauthAccountCommand;
        public ICommand ReauthAccountCommand
        {
            get => RelayCommand.GetCommand<Account>(ref _reauthAccountCommand, (acc) => 
            {
                Runtime.TaskRun(() =>
                {
                    var authResult = acc.Auth();
                    if (authResult == AuthCode.Successfully)
                        Account.SaveAll();
                    else if (acc.AccountType == AccountType.Microsoft && (authResult == AuthCode.TokenError || authResult == AuthCode.SessionExpired)) 
                    {
                        System.Diagnostics.Process.Start("https://login.live.com/oauth20_authorize.srf?client_id=ed0f84c7-4bf4-4a97-96c7-8c82b1e4ea0b&response_type=code&redirect_uri=https://night-world.org/requestProcessing/microsoftOAuth.php&scope=XboxLive.signin%20offline_access&state=NOT_NEEDED");
                    }
                    else
                    {
                        // TODO: Notification
                        // TODO: Error Handler
                    }
                });
            }); 
        }


        private RelayCommand _addAccountCommand;
        public ICommand OpenAccountFactoryCommand {
            get => RelayCommand.GetCommand(ref _addAccountCommand, () => 
            {
                _modalNavigationStore.Open(new AccountFactoryViewModel());
            });
        }


        #endregion Commands


        public AccountsSettingsViewModel(ModalNavigationStore modalNavigationStore)
        {
            _modalNavigationStore = modalNavigationStore;
            Model = new AccountsSettingsModel();
        }
    }
}


