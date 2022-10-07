using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;

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
            public string clientToken;
            public SelectedProfile selectedProfile;
        }

        public class AuthResult
        {
            public AuthCode Status;
            public string Login;
            public string UUID;
            public string AccesToken;
            public string ClientToken;
        }

        private static Random random = new Random();

        public static AuthResult Auth(string username, string password)
        {
            try
            {
                string payload = 
                    "{" +
                        "\"agent\" : {" +
                            "\"name\": \"Minecraft\", " +
                            "\"version\": 1" +
                        "}," +
                        "\"username\": \"" + username + "\"," +
                        "\"password\": \"" + password + "\"," +
                        "\"clientToken\": \"" + random.GenerateString(20) + "\"" +
                    "}";

                string answer = ToServer.HttpPostJson("https://authserver.mojang.com/authenticate", payload, out HttpStatusCode? statusCode);

                if (answer == null)
                {
                    if (statusCode == null)
                    {
                        return new AuthResult
                        {
                            Status = AuthCode.NoConnect
                        };
                    }

                    if (statusCode == HttpStatusCode.Gone)
                    {
                        return new AuthResult
                        {
                            Status = AuthCode.NeedMicrosoftAuth
                        };

                    }
                    else if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                    {
                        return new AuthResult
                        {
                            Status = AuthCode.DataError
                        };
                    }
                }

                var data = JsonConvert.DeserializeObject<AuthAnswer>(answer);
                Runtime.DebugWrite("Mojang Auth " + data.accessToken);

                if (data != null && !string.IsNullOrEmpty(data.accessToken) && data.selectedProfile != null
                    && !string.IsNullOrEmpty(data.selectedProfile.id) && !string.IsNullOrEmpty(data.selectedProfile.name))
                {
                    return new AuthResult 
                    { 
                        Status = AuthCode.Successfully,
                        Login = data.selectedProfile.name,
                        UUID = data.selectedProfile.id,
                        AccesToken = data.accessToken,
                        ClientToken = data.clientToken
                    };
                }
            }
            catch { }

            return new AuthResult
            {
                Status = AuthCode.NoConnect
            };
        }

        public static AuthResult Refresh(string username, string accessToken, string clientToken)
        {
            try
            {
                string payload =
                    "{" +
                        "\"accessToken\": \"" + accessToken + "\"," +
                        "\"clientToken\": \"" + clientToken + "\"" +
                    "}";

                string answer = ToServer.HttpPostJson("https://authserver.mojang.com/refresh", payload, out HttpStatusCode? statusCode);

                if (answer == null)
                {
                    if (statusCode == null)
                    {
                        return new AuthResult
                        {
                            Status = AuthCode.NoConnect
                        };
                    }

                    if (statusCode == HttpStatusCode.Gone)
                    {
                        return new AuthResult
                        {
                            Status = AuthCode.NeedMicrosoftAuth
                        };

                    }
                    else if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                    {
                        return new AuthResult
                        {
                            Status = AuthCode.SessionExpired
                        };
                    }
                }

                var data = JsonConvert.DeserializeObject<AuthAnswer>(answer);
                Runtime.DebugWrite("Mojang Refresh " + data.accessToken);

                if (data != null && !string.IsNullOrEmpty(data.accessToken) && !string.IsNullOrEmpty(data.clientToken) 
                    && data.selectedProfile != null && !string.IsNullOrEmpty(data.selectedProfile.id) && !string.IsNullOrEmpty(data.selectedProfile.name))
                {
                    return new AuthResult
                    {
                        Status = AuthCode.Successfully,
                        Login = data.selectedProfile.name,
                        UUID = data.selectedProfile.id,
                        AccesToken = data.accessToken,
                        ClientToken = data.clientToken
                    };
                }
            }
            catch { }

            return new AuthResult
            {
                Status = AuthCode.SessionExpired
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
            try
            {

                var mcfData = JsonConvert.DeserializeObject<MicrosoftData>(microsoftData);
                string payload =
                    "{" +
                        "\"identityToken\" : \"XBL3.0 x=" + mcfData.uhs + ";" + mcfData.xsts_token + "\"," +
                        "\"ensureLegacyEnabled\" : true" +
                    "}";

                string answer = ToServer.HttpPostJson("https://api.minecraftservices.com/authentication/login_with_xbox", payload, out _);
                
                if(answer == null)
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<MicrosoftAuthRes>(answer).access_token;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Авторизация токеном.
        /// </summary>
        /// <param name="token">Сам токен. Получить можно в GetToken.</param>
        /// <returns>Результат.</returns>
        public static AuthResult AuthFromToken(string token)
        {
            try
            {
                string answer = ToServer.HttpGet("https://api.minecraftservices.com/minecraft/profile", new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>
                    (
                        "Authorization",
                        "Bearer " + token
                    )
                });

                if (answer == null)
                {
                    return new AuthResult
                    {
                        Status = AuthCode.DataError
                    };
                }

                var profile = JsonConvert.DeserializeObject<MojangProfile>(answer);

                return new AuthResult
                {
                    Status = AuthCode.Successfully,
                    Login = profile.name,
                    UUID = profile.id,
                    AccesToken = token
                };
            }
            catch { }


            return new AuthResult
            {
                Status = AuthCode.DataError
            };
        }
    }
}
