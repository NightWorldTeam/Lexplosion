namespace Lexplosion.Logic.Management.Accounts.Auth
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
		AuthResult Auth(string login, string accessData);

		/// <summary>
		/// Проверяет сохраненные данных аунтефикации.
		/// </summary>
		/// <param name="login">Логин</param>
		/// <param name="accessData">Данные для доступа. Так же эта переменная изменится, в нее будут помещены данные доступа, которые можно сохранять</param>
		/// <param name="code">Результат выполнения.</param>
		/// <returns></returns>
		AuthResult ReAuth(string login, string accessData);

		struct AuthResult
		{
			public AuthCode Code;
			public string Login;
			public string UUID;
			public string AccessData;
			public string AccessToken;
			public string SessionToken;
			public object AdditionalInfo;
			public ActivityStatus Status;
		}
	}
}
