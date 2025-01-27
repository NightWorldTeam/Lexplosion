using Lexplosion.Logic.Management.Authentication;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization
{
    public class AccountTypeMenuItem
    {
        public string Name { get; set; }
        public string LogoPath { get; set; }
        public ICommand ToAuthorizationFormCommand { get; set; }

        public AccountTypeMenuItem(string name, string logoPath, ICommand toAuthorizationFormCommand)
        {
            Name = name;
            LogoPath = logoPath;
            ToAuthorizationFormCommand = toAuthorizationFormCommand;
        }
    }


    public sealed class AuthorizationMenuModel
    {
        public IEnumerable<AccountTypeMenuItem> AccountTypes { get; }


        #region Constructors


        public AuthorizationMenuModel(ICommand toNightWorldForm, ICommand toMicrosoftForm, ICommand toOffline)
        {
            AccountTypes = new List<AccountTypeMenuItem>()
            {
                new AccountTypeMenuItem("ViaNightWorld", "pack://Application:,,,/Assets/images/icons/nightworld.png", toNightWorldForm),
                new AccountTypeMenuItem("ViaMicrosoft", "pack://Application:,,,/Assets/images/icons/microsoft.png", toMicrosoftForm),
                new AccountTypeMenuItem("WithoutAccount", "pack://Application:,,,/Assets/images/icons/non_image1.png", toOffline),
            };

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
}
