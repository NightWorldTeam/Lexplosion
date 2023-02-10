using System.Runtime.CompilerServices;
using Lexplosion.Logic.Network;

namespace Lexplosion.Logic.Management.Authentication
{
    using AuthData = NightWorldApi.AuthData;
    internal class NightWorldAuth : IAuthHandler
    {
        public User Auth(ref string login, ref string accessData, out AuthCode code)
        {
            //accessData = "{\"type\":\"password\",\"data\":\"" + Сryptography.Sha256(accessData + login) + "\"}";
            var data = new AuthData
            {
                login = login,
                accessData = new AuthData.AccessData
                {
                    type = "password",
                    data = Сryptography.Sha256(accessData + login)
                }
            };
            return Execute(data, ref accessData, out code);
        }

        public User ReAuth(ref string login, ref string accessData, out AuthCode code)
        {
            //accessData = "{\"type\":\"accessID\",\"data\":\"" + accessData + "\"}";
            var data = new AuthData
            {
                login = login,
                accessData = new AuthData.AccessData
                {
                    type = "accessID",
                    data = accessData
                }
            };
            return Execute(data, ref accessData, out code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private User Execute(AuthData authData, ref string accessData, out AuthCode code)
        {
            AuthResult response = NightWorldApi.Authorization(authData, out int baseStatus);
            accessData = response.AccessID;

            ActivityStatus status = ActivityStatus.Online;
            if (baseStatus == 1)
            {
                status = ActivityStatus.Offline;
            }
            else if (baseStatus == 2)
            {
                status = ActivityStatus.NotDisturb;
            }

            if (response.Status == AuthCode.Successfully)
            {
                User user = new User(response.Login,
                    response.UUID,
                    response.AccesToken,
                    response.SessionToken,
                    AccountType.NightWorld,
                    status);

                LaunchGame.GameStartEvent += delegate (LaunchGame gameManager)
                {
                    user.GameStart(gameManager.GameClientName);
                };

                LaunchGame.GameStopEvent += delegate (LaunchGame gameManager)
                {
                    user.GameStop(gameManager.GameClientName);
                };

                Lexplosion.Runtime.ExitEvent += user.Exit;

                code = AuthCode.Successfully;
                return user;
            }

            code = response.Status;
            return null;
        }
    }
}
