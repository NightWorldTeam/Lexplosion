using Lexplosion.Core.Tools.Notification;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Mvvm.Models.Authorization
{
    public delegate void DoNotificationCallback(string titleKey, string messageKey, uint time, NotificationType type);

    public abstract class AuthModelBase : ViewModelBase
    {
        protected readonly AppCore _appCore;

        protected AuthModelBase(AppCore appCore)
        {
            _appCore = appCore;
        }

        /// <summary>
        /// Обрабатывает результат авторизации NightWorld
        /// </summary>
        /// <param name="accountType">Тип аккаунта авторизации</param>
        /// <param name="authCode">Код результата авторизации</param>
        protected virtual void PerformNightWorldAuthCode(Account account, AuthCode authCode, bool isOAuth2 = false)
        {
            switch (authCode)
            {
                case AuthCode.Successfully:
                    {
                        SuccessfulAuthorization(account);
                        break;
                    }
                case AuthCode.DataError:
                    {
                        _appCore.MessageService.Error("WrongLoginOrPassword", true);
                        break;
                    }
                case AuthCode.NoConnect:
                    {
                        _appCore.MessageService.Error("NoConnectionToTheServer", true);
                        break;
                    }
                case AuthCode.TokenError:
                    {
                        _appCore.MessageService.Error("TokenErrorTryAuthAgain", true);
                        break;
                    }
                case AuthCode.SessionExpired:
                    {
                        _appCore.MessageService.Error("SessionExpiredTryAuthAgain", true);
                        break;
                    }
                default:
                    {
                        _appCore.MessageService.Error("UnknownError", true);
                        break;
                    }
            }
        }

        /// <summary>
        /// Заполняет UserData, при успешной авторизации пользователя.
        /// </summary>
        /// <param name="name">Никнейм игрока</param>
        /// <param name="accountType">Тип аккаунта которым пользователь авторизировался</param>
        private void SuccessfulAuthorization(Account account)
        {
            account.IsActive = true;
            account.IsLaunch = true;
            account.Save();
        }
    }
}
