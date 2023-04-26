using System.Runtime.CompilerServices;
using Lexplosion.Logic.Network;

namespace Lexplosion.Logic.Management.Authentication
{
    using AuthData = NightWorldApi.AuthData;

    internal class NightWorldAuth : IAuthHandler
    {
        public User Auth(ref string login, ref string accessData, out AuthCode code)
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
            return Execute(data, ref accessData, out code);
        }

        public User ReAuth(ref string login, ref string accessData, out AuthCode code)
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
            return Execute(data, ref accessData, out code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private User Execute(AuthData authData, ref string accessData, out AuthCode code)
        {
            NwAuthResult response = NightWorldApi.Authorization(authData);
            accessData = response.AccessID;

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

                User user = new User(response.Login,
                    response.UUID,
                    response.AccesToken,
                    response.SessionToken,
                    AccountType.NightWorld,
                    status);

                LaunchGame.OnGameProcessStarted += delegate (LaunchGame gameManager)
                {
                    user.GameStart(gameManager.GameClientName);
                };

                LaunchGame.OnGameStoped += delegate (LaunchGame gameManager)
                {
                    user.GameStop(gameManager.GameClientName);
                };

                Lexplosion.Runtime.OnExitEvent += user.Exit;

                code = AuthCode.Successfully;
                return user;
            }

            code = response.Status;
            return null;
        }
    }
}
