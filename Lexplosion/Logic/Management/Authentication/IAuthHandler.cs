using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Authentication
{
    interface IAuthHandler
    {
        /// <summary>
        /// Авторизирует в первый раз.
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="accessData">Данные для доступа. Так же эта переменная изменится, в нее будут помещены данные доступа, которые можно сохранять</param>
        /// <param name="code">Результат выполнения.</param>
        /// <returns></returns>
        User Auth(string login, ref string accessData, out AuthCode code);

        /// <summary>
        /// Проверяет сохраненные данных аунтефикации.
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="accessData">Данные для доступа. Так же эта переменная изменится, в нее будут помещены данные доступа, которые можно сохранять</param>
        /// <param name="code">Результат выполнения.</param>
        /// <returns></returns>
        User ReAuth(string login, ref string accessData, out AuthCode code);
    }
}
