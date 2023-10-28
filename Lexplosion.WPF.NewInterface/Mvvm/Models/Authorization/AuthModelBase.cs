using Lexplosion.Global;
using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization
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
                        _doNotification("authError", "wrongLoginOrPassword", 8, NotificationType.Error);
                        break;
                    }
                case AuthCode.NoConnect:
                    {
                        _doNotification("authError", "noConnectionsToTheServer", 8, NotificationType.Error);
                        break;
                    }
                case AuthCode.TokenError:
                    {
                        _doNotification("authError", "tokenError", 8, NotificationType.Error);
                        break;
                    }
                case AuthCode.SessionExpired:
                    {
                        _doNotification("loginFailed", "sessionExpiredPleaseTryAgainFillPassword", 8, NotificationType.Error);
                        break;
                    }
                default:
                    {
                        _doNotification("someError", authCode.ToString(), 8, NotificationType.Error);
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
