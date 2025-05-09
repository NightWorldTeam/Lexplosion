namespace Lexplosion.Logic.Management.Accounts.Auth
{
	class LocalAuth : IAuthHandler
	{
		public IAuthHandler.AuthResult Auth(string login, string accessData)
		{
			return new IAuthHandler.AuthResult
			{
				Login = login,
				UUID = "00000000-0000-0000-0000-000000000000",
				AccessToken = "null",
				SessionToken = null,
				AccessData = null,
				Status = ActivityStatus.Offline
			};
		}

		public IAuthHandler.AuthResult ReAuth(string login, string accessData)
		{
			return Auth(login, accessData);
		}
	}
}
