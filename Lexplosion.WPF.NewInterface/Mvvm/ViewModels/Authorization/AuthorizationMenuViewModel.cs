using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Stores;
using System;

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

    public sealed class AuthorizationMenuViewModel : VMBase
    {
        private readonly INavigationStore _navigationStore;


        #region Commands


        private RelayCommand _openAccountAuthFormCommand;
        public RelayCommand OpenAccountAuthFormCommand
        {
            get => _openAccountAuthFormCommand ?? (_openAccountAuthFormCommand = new RelayCommand(obj =>
            {
                _navigationStore.CurrentViewModel = new NightWorldAuthorizationViewModel(_navigationStore);
            }));
        }

        #endregion Commands


        #region Constructors


        public AuthorizationMenuViewModel(INavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }


        #endregion Constructors
    }
}
