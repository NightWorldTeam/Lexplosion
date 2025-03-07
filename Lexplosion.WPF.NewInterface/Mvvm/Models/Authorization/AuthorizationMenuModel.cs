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
        }


        #endregion Constructors
    }
}
