using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Web;

namespace Lexplosion.Logic.Management.Authentication
{
    class MicrosoftAuth : IAuthHandler
    {
        public User Auth(ref string login, ref string accessData, out AuthCode code)
        {
            string token = MojangApi.GetToken(accessData);
            if (token == null)
            {
                code = AuthCode.SessionExpired;
                return null;
            }

            MojangAuthResult response = MojangApi.AuthFromToken(token);
            if (response.Status == AuthCode.Successfully)
            {
                code = AuthCode.Successfully;
                login = response.Login;
                return new User(response.Login, response.UUID, response.AccesToken, null, AccountType.Microsoft, ActivityStatus.Online);
            }

            code = AuthCode.NoConnect;
            return null;
        }

        public User ReAuth(ref string login, ref string accessData, out AuthCode code)
        {
            return Auth(ref login, ref accessData, out code);
        }
    }
}
