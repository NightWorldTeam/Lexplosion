using Lexplosion.Global;
using Lexplosion.Logic.Management.Authentication;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.ViewModels;
using System;

namespace Lexplosion.WPF.NewInterface.Models.Authorization
{
    public abstract class AuthModelBase : VMBase
    {
        private readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };
        
        protected AuthModelBase(DoNotificationCallback doNotification)
        {
            _doNotification = doNotification;
        }

        /// <summary>
        /// Обрабатывает результат авторизации
        /// </summary>
        /// <param name="accountType">Тип аккаунта авторизации</param>
        /// <param name="authCode">Код результата авторизации</param>
        protected void PerformAuthCode(AccountType accountType, AuthCode authCode, bool isOAuth2 = false)
        {
            switch (authCode)
            {
                case AuthCode.Successfully:
                    {
                        SuccessfulAuthorization(GlobalData.User.Login, accountType);
                        break;
                    }
                case AuthCode.DataError:
                    {
                        _doNotification(ResourceGetter.GetString("authError"), ResourceGetter.GetString("wrongLoginOrPassword"), 8, NotificationType.Error);
                        break;
                    }
                case AuthCode.NoConnect:
                    {
                        _doNotification(ResourceGetter.GetString("authError"), ResourceGetter.GetString("noConnectionsToTheServer"), 8, NotificationType.Error);
                        break;
                    }
                case AuthCode.TokenError:
                    {
                        _doNotification(ResourceGetter.GetString("authError"), ResourceGetter.GetString("tokenError"), 8, NotificationType.Error);
                        break;
                    }
                case AuthCode.SessionExpired:
                    {
                        _doNotification(ResourceGetter.GetString("loginFailed"), ResourceGetter.GetString("sessionExpiredPleaseTryAgainFillPassword"), 8, NotificationType.Error);
                        break;
                    }
                default:
                    {
                        _doNotification(ResourceGetter.GetString("someError"), authCode.ToString(), 8, NotificationType.Error);
                        break;
                    }
            }
        }

        /// <summary>
        /// Заполняет UserData, при успешной авторизации пользователя.
        /// </summary>
        /// <param name="name">Никнейм игрока</param>
        /// <param name="accountType">Тип аккаунта которым пользователь авторизировался</param>
        private void SuccessfulAuthorization(string name, AccountType accountType) 
        {
            UserData.Instance.Nickname = name;
            UserData.Instance.IsAuthrized = true;
            UserData.Instance.CurrentAccountType = accountType;
        }
    }
}
