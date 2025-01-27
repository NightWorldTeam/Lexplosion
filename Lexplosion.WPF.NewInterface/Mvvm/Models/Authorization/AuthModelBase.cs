using Lexplosion.Core.Tools.Notification;
using Lexplosion.Logic.Management.Accounts;
using Lexplosion.WPF.NewInterface.Core;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization
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
                        // TODO: Translate
                        _appCore.MessageService.Error("Ошибка авторизации");
                        break;
                    }
                case AuthCode.NoConnect:
                    {
                        // TODO: Translate
                        _appCore.MessageService.Error("Не удалось соединиться с сервером");
                        break;
                    }
                case AuthCode.TokenError:
                    {
                        // TODO: Translate
                        _appCore.MessageService.Error("Ошибка токена");
                        break;
                    }
                case AuthCode.SessionExpired:
                    {
                        // TODO: Translate
                        _appCore.MessageService.Error("Сессия истекла");
                        break;
                    }
                default:
                    {
                        // TODO: Translate
                        _appCore.MessageService.Error("Unknown error");
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
