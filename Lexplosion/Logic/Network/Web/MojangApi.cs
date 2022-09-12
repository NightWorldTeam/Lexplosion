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
                    string accessToken = DecodeToken(data.accessToken);

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

        private class MicrosoftData
        {
            public string uhs;
            public string xsts_token;
        }

        private class MicrosoftAuthRes
        {
            public string access_token;
        }

        private class MojangProfile
        {
            public string id;
            public string name;
        }

        /// <summary>
        /// Возвращает токен для майнрафта.
        /// </summary>
        /// <param name="microsoftData">json массив microsoft данных (uhs и xsts_token)</param>
        /// <returns>Возвращает токен, который можно использовать для получения аккаунта.</returns>
        public static string GetToken(string microsoftData)
        {
            var mcfData = JsonConvert.DeserializeObject<MicrosoftData>(microsoftData);

            WebRequest req = WebRequest.Create("https://api.minecraftservices.com/authentication/login_with_xbox");
            req.Method = "POST";
            req.ContentType = "application/json";

            byte[] byteArray = Encoding.UTF8.GetBytes(
                "{" +
                    "\"identityToken\" : \"XBL3.0 x=" + mcfData.uhs + ";" + mcfData.xsts_token + "\"," +
                    "\"ensureLegacyEnabled\" : true" +
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

            Console.WriteLine("1 " + answer);

            return JsonConvert.DeserializeObject<MicrosoftAuthRes>(answer).access_token;
        }

        /// <summary>
        /// олучаем из моджанговского токена аксесс токен для майкрафта.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Акссес токен</returns>
        private static string DecodeToken(string token)
        {
            string accessToken = null;
            int i = 0;
            string ednPart = "";
            // по сути ебанный костыль. Я не собираюсь юзать либо для декодирования jwt, она весит больше товарного состава блять и тащит за собой пару миллиардов других dll'ников
            while (i < 3)
            {
                try
                {
                    var a = token.Split('.')[1] + ednPart;
                    var b = Convert.FromBase64String(a);
                    var c = Encoding.UTF8.GetString(b);
                    var obj = JsonConvert.DeserializeObject<AccsessTokenData>(c);
                    accessToken = obj.yggt;

                    break;
                }
                catch
                {
                    ednPart += "=";
                }

                i++;
            }

            Console.WriteLine("TOKEN " + i + " " + accessToken);

            return accessToken;
            
        }

        /// <summary>
        /// Авторизация токеном.
        /// </summary>
        /// <param name="token">Сам токен. Получить можно в GetToken.</param>
        /// <returns>Результат.</returns>
        public static AuthResult AuthFromToken(string token)
        {
            //try
            {
                string accessToken = token;
                string answer = ToServer.HttpGet("https://api.minecraftservices.com/minecraft/profile", new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>
                    (
                        "Authorization",
                        "Bearer " + token
                    )
                });

                Console.WriteLine("2 " + answer);

                var profile = JsonConvert.DeserializeObject<MojangProfile>(answer);

                return new AuthResult
                {
                    Status = AuthCode.Successfully,
                    Login = profile.name,
                    UUID = profile.id,
                    AccesToken = accessToken,
                    SessionToken = null
                };
            }
            //catch { }


            return new AuthResult
            {
                Status = AuthCode.DataError
            };
        }
    }
}
