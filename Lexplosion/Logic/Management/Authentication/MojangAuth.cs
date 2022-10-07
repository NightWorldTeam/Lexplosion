using Newtonsoft.Json;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Authentication
{
    class MojangAuth : IAuthHandler
    {
        private class AccessDataFormat
        {
            public string ClientToken;
            public string AccessToken;
        }

        public User Auth(ref string login, ref string accessData, out AuthCode code)
        {
            MojangApi.AuthResult response = MojangApi.Auth(login, accessData);

            if (response != null)
            {
                if (response.Status == AuthCode.Successfully)
                {
                    accessData = JsonConvert.SerializeObject(new AccessDataFormat
                    {
                        ClientToken = response.ClientToken,
                        AccessToken = response.AccesToken
                    });
                    Runtime.DebugWrite(accessData);

                    code = AuthCode.Successfully;
                    return new User(response.Login, response.UUID, response.AccesToken, null, AccountType.Mojang, ActivityStatus.Online);
                }

                code = response.Status;
                return null;
            }

            code = AuthCode.NoConnect;
            return null;
        }

        public User ReAuth(ref string login, ref string accessData, out AuthCode code)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<AccessDataFormat>(accessData);

                if (data != null && !string.IsNullOrEmpty(data.AccessToken) && !string.IsNullOrEmpty(data.ClientToken))
                {
                    Runtime.DebugWrite(accessData);
                    MojangApi.AuthResult response = MojangApi.Refresh(login, data.AccessToken, data.ClientToken);

                    if (response.Status == AuthCode.Successfully)
                    {
                        code = AuthCode.Successfully;

                        accessData = JsonConvert.SerializeObject(new AccessDataFormat
                        {
                            ClientToken = response.ClientToken,
                            AccessToken = response.AccesToken
                        });

                        return new User(response.Login, response.UUID, response.AccesToken, null, AccountType.Mojang, ActivityStatus.Online);
                    }
                    else
                    {
                        code = response.Status;
                    }
                }
            }
            catch {}

            code = AuthCode.SessionExpired;
            return null;
        }
    }
}
