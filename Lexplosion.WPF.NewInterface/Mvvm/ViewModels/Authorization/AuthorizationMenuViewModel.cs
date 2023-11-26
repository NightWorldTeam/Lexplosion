using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Authorization
{
    public class AccountTypeMenuItem
    {
        public string Name { get; set; }
        public byte[] Logo { get; set; }
    }


    public sealed class AuthorizationMenuModel
    {
        #region Constructors


        public AuthorizationMenuModel()
        {
            var loadedNWAccount = LoadSavedAccount(AccountType.NightWorld);
            var loadedMSAccount = LoadSavedAccount(AccountType.Microsoft);
        }


        #endregion Constructors


        #region Public & Protected Methods


        /// <summary>
        /// Возвращает тип аккаунта, логин, и ответ на вопрос существует ли не пустой логин.
        /// </summary>
        /// <param name="accountType"></param>
        /// <returns>AccountType, string, bool</returns>
        protected Tuple<AccountType, string, bool> LoadSavedAccount(AccountType? accountType)
        {
            AccountType type = Authentication.Instance.GetAccount(accountType, out string _loadedLogin);
            return new Tuple<AccountType, string, bool>(type, _loadedLogin, string.IsNullOrEmpty(_loadedLogin));
        }


        #endregion Public & Protected Methods
    }

    public sealed class AuthorizationMenuViewModel : ViewModelBase
    {
        private readonly INavigationStore _navigationStore;
        private readonly ICommand _toMainMenu;

        private readonly ViewModelBase _microsoft;
        private readonly ViewModelBase _nightWorld;
        private readonly ViewModelBase _withoutAccount;


        #region Commands


        private RelayCommand _openAccountAuthFormCommand;
        public ICommand OpenAccountAuthFormCommand
        {
            get => RelayCommand.GetCommand(ref _openAccountAuthFormCommand, () => 
            {
                _navigationStore.CurrentViewModel = new NightWorldAuthorizationViewModel();
            });
        }

        private RelayCommand _toNightWorldCommand;
        public ICommand ToNightWorldCommand 
        {
            get => RelayCommand.GetCommand(ref _toNightWorldCommand, () => { });
        }



        #endregion Commands


        #region Constructors


        public AuthorizationMenuViewModel(INavigationStore navigationStore, ICommand toMainMenu)
        {
            _navigationStore = navigationStore;
            _toMainMenu = toMainMenu;
        }


        #endregion Constructors
    }
}
