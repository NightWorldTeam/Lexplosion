using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.Management.Accounts.Auth
{
	class MicrosoftAuth : IAuthHandler
	{
		public IAuthHandler.AuthResult Auth(string login, string accessData)
		{
			string token = MojangApi.GetToken(accessData);
			if (token == null)
			{
				return new IAuthHandler.AuthResult
				{
					Code = AuthCode.SessionExpired
				};
			}

			MojangAuthResult response = MojangApi.AuthFromToken(token);
			if (response.Status == AuthCode.Successfully)
			{
				return new IAuthHandler.AuthResult
				{
					Login = response.Login,
					UUID = response.UUID,
					AccessToken = response.AccesToken,
					SessionToken = null,
					AccessData = accessData,
					Status = ActivityStatus.Online,
					Code = AuthCode.Successfully
				};
			}

			Runtime.DebugWrite("MicrosoftAuth code: " + response?.Status);

			return new IAuthHandler.AuthResult
			{
				Code = AuthCode.NoConnect
			};
		}

		public IAuthHandler.AuthResult ReAuth(string login, string accessData)
		{
			return Auth(login, accessData);
		}
	}
}
