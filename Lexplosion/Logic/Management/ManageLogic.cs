using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Global;

namespace Lexplosion.Logic.Management
{
    static class ManageLogic
    {
        public static AuthCode Auth(string login, string password, bool saveUser)
        {
            Dictionary<string, string> response = ToServer.Authorization(login, password);

            if (response != null)
            {
                if (response["status"] == "OK")
                {
                    UserData.Login = response["login"];
                    UserData.UUID = response["UUID"];
                    UserData.AccessToken = response["accesToken"];

                    if (saveUser)
                    {
                        DataFilesManager.SaveAccount(login, password);
                    }

                    UserData.IsAuthorized = true;
                    UserStatusSetter.SetBaseStatus(UserStatusSetter.Statuses.Online);

                    return AuthCode.Successfully;
                }
                else
                {
                    return AuthCode.DataError;
                }
            }
            else
            {
                return AuthCode.NoConnect;
            }
        }
    }
}
