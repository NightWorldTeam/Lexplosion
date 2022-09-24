using System.Runtime.CompilerServices;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Authentication
{
    internal class NightWorldAuth : IAuthHandler
    {
        public User Auth(string login, ref string accessData, out AuthCode code)
        {
            accessData = "{\"type\":\"password\",\"data\":\"" + accessData + "\"}";
            return Execute(login, ref accessData, out code);      
        }

        public User ReAuth(string login, ref string accessData, out AuthCode code)
        {
            accessData = "{\"type\":\"accessID\",\"data\":\"" + accessData + "\"}";
            return Execute(login, ref accessData, out code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private User Execute(string login, ref string accessData, out AuthCode code)
        {
            ToServer.AuthResult response = ToServer.Authorization(login, accessData, out int baseStatus);
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

            if (response != null)
            {
                if (response.Status == AuthCode.Successfully)
                {
                    User user = new User(response.Login, 
                        response.UUID, 
                        response.AccesToken, 
                        response.SessionToken, 
                        AccountType.NightWorld, 
                        status);

                    LaunchGame.GameStartEvent += user.GameStart;
                    LaunchGame.GameStopEvent += user.GameStop;
                    Lexplosion.Run.ExitEvent += user.Exit;

                    code = AuthCode.Successfully;
                    return user;
                }

                code = response.Status;
                return null;
            }

            code = AuthCode.NoConnect;
            return null;
        }
    }
}
