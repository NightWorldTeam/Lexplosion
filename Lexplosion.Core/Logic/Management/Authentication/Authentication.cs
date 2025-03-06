﻿using System;
using System.Text;
using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Authentication
{
    public class Authentication
    {
        public static Authentication Instance { get; } = new Authentication();


        private OldAcccountsFormat _accounts;


        private Authentication()
        {
			Runtime.DebugWrite("OLD AUTH");
            _accounts = DataFilesManager.GetFile<OldAcccountsFormat>(LaunсherSettings.LauncherDataPath + "/account.json");
        }

        /// <summary>
        /// Получает сохраненный аккаунт.
        /// </summary>
        /// <param name="login">Сюда будет помещен логин.</param>
        /// <param name="accountType">Сюда передавать тип аккаунта, который нужно получить. Передавать null, если нужно получить использованный в последний раз аккаунт.</param>
        /// <returns>
        /// Возвращает тип полученного аккаунта, он может отличаться от параметра accountType, 
        /// ведь если accountType будет null, то будет возвращен тип аккаунта, использованного в последний раз.
        /// </returns>
        public AccountType GetAccount(AccountType? accountType, out string login)
        {
            if (_accounts != null && _accounts.Profiles != null && _accounts.Profiles.Count > 0)
            {
                AccountType selectedAccount = accountType ?? _accounts.SelectedProfile;

                if (_accounts.Profiles.ContainsKey(selectedAccount))
                {
                    OldAcccountsFormat.Profile profile = _accounts.Profiles[selectedAccount];

                    if (!string.IsNullOrWhiteSpace(profile.Login) && !string.IsNullOrWhiteSpace(profile.AccessData))
                    {
                        login = profile.Login;
                    }
                    else
                    {
                        login = null;
                    }

                    return selectedAccount;
                }
            }

            login = null;
            return accountType ?? AccountType.NightWorld;
        }

        /// <summary>
        /// Производит авторизацию.
        /// </summary>
        /// <param name="accountType">Тип аккаунта.</param>
        /// <param name="newLogin">Если нужно обновить аккаунт, то сюда пихать новый логин. В ином случае null. Если этот парметр не равен null, то newPassword тоже не должен быть равен null.</param>
        /// <param name="newPassword">Если нужно обновить пароль аккаунта, то сюда пихать новый пароль.</param>
        /// <param name="saveUser">Сохранять ли аккаунт.</param>
        public AuthCode Auth(AccountType accountType, string newLogin, string newPassword, bool saveUser)
        {
            IAuthHandler authHandler;

            switch (accountType)
            {
                case AccountType.NightWorld:
                    authHandler = new NightWorldAuth();
                    break;
                case AccountType.Microsoft:
                    authHandler = new MicrosoftAuth();
                    break;
                case AccountType.NoAuth:
                    authHandler = new LocalAuth();
                    break;
                default:
                    authHandler = null;
                    break;
            }

            AuthCode result;
            User user = null;
            string login;
            string accessData;

            if (newLogin == null && newPassword == null)
            {
                var account = _accounts.Profiles[accountType];

                login = account.Login;
                byte[] key = Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey);
                byte[] IV = Encoding.UTF8.GetBytes(LaunсherSettings.passwordKey.Substring(0, 16));
                byte[] decripted = Cryptography.AesDecode(Convert.FromBase64String(account.AccessData), key, IV);
                accessData = Encoding.UTF8.GetString(decripted);

                user = authHandler.ReAuth(ref login, ref accessData, out result);
            }
            else
            {
                if (newLogin == null)
                {
                    newLogin = _accounts.Profiles[accountType].Login;
                }

                login = newLogin;
                accessData = newPassword;

                user = authHandler.Auth(ref login, ref accessData, out result);
            }

            if (result == AuthCode.Successfully)
            {
                if (saveUser)
                {
                    DataFilesManager.SaveAccount(login, accessData, accountType);
                }
            }

            return result;
        }
    }
}
