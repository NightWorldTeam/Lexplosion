using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Lexplosion.Logic.Network.Web
{
    static class MojangApi
    {
        class AuthAnswer
        {
            public class SelectedProfile 
            {
                public string name;
                public string id;
            }

            public string accessToken;
            public SelectedProfile selectedProfile;
        }

        class AccsessTokenData
        {
            public string yggt;
        }

        public static AuthResult Authorization(string username, string password)
        {
            try
            {
                WebRequest req = WebRequest.Create("https://authserver.mojang.com/authenticate");
                req.Method = "POST";
                req.ContentType = "application/json";

                byte[] byteArray = Encoding.UTF8.GetBytes(
                    "{" +
                        "\"agent\" : {" +
                            "\"name\": \"Minecraft\", " +
                            "\"version\": 1" +
                        "}," +
                        "\"username\": \"" + username + "\"," +
                        "\"password\": \"" + password + "\"" +
                    "}"
                    );

                req.ContentLength = byteArray.Length;

                using (Stream dataStream = req.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                string answer;
                using (WebResponse resp = req.GetResponse())
                {
                    using (Stream stream = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            answer = sr.ReadToEnd();
                        }
                    }
                }

                var data = JsonConvert.DeserializeObject<AuthAnswer>(answer);

                if (data != null && !string.IsNullOrEmpty(data.accessToken) && data.selectedProfile != null
                    && !string.IsNullOrEmpty(data.selectedProfile.id) && !string.IsNullOrEmpty(data.selectedProfile.name))
                {
                    string accessToken = JsonConvert.DeserializeObject<AccsessTokenData>(Encoding.UTF8.GetString(Convert.FromBase64String(data.accessToken.Split('.')[1] + "=="))).yggt;

                    return new AuthResult 
                    { 
                        Status = AuthCode.Successfully,
                        Login = data.selectedProfile.name,
                        UUID = data.selectedProfile.id,
                        AccesToken = accessToken,
                        SessionToken = null
                    };
                }
            }
            catch { }


            return new AuthResult
            {
                Status = AuthCode.DataError
            };
        }
    }
}
