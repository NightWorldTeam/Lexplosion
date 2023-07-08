using System.Security;

namespace Lexplosion.WPF.NewInterface.Models.Authorization
{
    public interface IBasicAuthModel : IAuthModel
    {
        /// <summary>
        /// Логин от учётной записи пользователя.
        /// </summary>
        string Login { get; set; }
        /// <summary>
        /// Пароль от учётной записи пользователя.
        /// </summary>
        string Password { get; set; }
        /// <summary>
        /// Будет ли сохранения аккаунта после авторизации.
        /// </summary>
        bool IsRememberMe { get; set; }
    }
}
