using Lexplosion.Logic.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Management.Accounts.Auth
{
    using AuthData = NightWorldApi.AuthData;

    internal class NightWorldAuth : IAuthHandler
    {
        public IAuthHandler.AuthResult Auth(string login, string accessData)
        {
            var data = new AuthData
            {
                login = login,
                accessData = new AuthData.AccessData
                {
                    type = "password",
                    data = Cryptography.Sha256(accessData + login)
                }
            };

            return Execute(data);
        }

        public IAuthHandler.AuthResult ReAuth(string login, string accessData)
        {
            var data = new AuthData
            {
                login = login,
                accessData = new AuthData.AccessData
                {
                    type = "accessID",
                    data = accessData
                }
            };

            return Execute(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAuthHandler.AuthResult Execute(AuthData authData)
        {
            NwAuthResult response = NightWorldApi.Authorization(authData);

            ActivityStatus status = ActivityStatus.Online;

            if (response.Status == AuthCode.Successfully)
            {
                if (response.BaseStatus == 1)
                {
                    status = ActivityStatus.Offline;
                }
                else if (response.BaseStatus == 2)
                {
                    status = ActivityStatus.NotDisturb;
                }

                var result = new IAuthHandler.AuthResult
                {
                    Login = response.Login,
                    UUID = response.UUID,
                    AccessToken = response.AccesToken,
                    SessionToken = response.SessionToken,
                    AccessData = response.AccessID,
                    Status = status,
                    Code = AuthCode.Successfully
                };

                return result;
            }

            return new IAuthHandler.AuthResult { Code = response.Status };
        }
    }
}
