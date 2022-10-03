using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Lexplosion.Global;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Authentication
{
    internal class NightWorldAuth : IAuthHandler
    {
        public User Auth(ref string login, ref string accessData, out AuthCode code)
        {
            accessData = "{\"type\":\"password\",\"data\":\"" + Sha256(accessData + login) + "\"}";
            return Execute(login, ref accessData, out code);      
        }

        public User ReAuth(ref string login, ref string accessData, out AuthCode code)
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

            code = AuthCode.NoConnect;
            return null;
        }

        private string Sha256(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
