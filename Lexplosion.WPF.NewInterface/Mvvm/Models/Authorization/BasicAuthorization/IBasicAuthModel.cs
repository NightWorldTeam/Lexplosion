namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization
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
        /// <summary>
        /// Идет процесс авторизации
        /// </summary>
        bool IsLoading { get; set; }
    }
}
