using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Network.Web
{
    static class MojangApi
    {
        private class AuthAnswer
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

        private static Random random = new Random();

        public static MojangAuthResult Auth(string username, string password)
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
                        return new MojangAuthResult
                        {
                            Status = AuthCode.NoConnect
                        };
                    }

                    if (statusCode == HttpStatusCode.Gone)
                    {
                        return new MojangAuthResult
                        {
                            Status = AuthCode.NeedMicrosoftAuth
                        };

                    }
                    else if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                    {
                        return new MojangAuthResult
                        {
                            Status = AuthCode.DataError
                        };
                    }
                }

                var data = JsonConvert.DeserializeObject<AuthAnswer>(answer);
                Runtime.DebugWrite("Mojang Auth " + data.accessToken);

                if (data != null && !string.IsNullOrWhiteSpace(data.accessToken) && data.selectedProfile != null
                    && !string.IsNullOrWhiteSpace(data.selectedProfile.id) && !string.IsNullOrWhiteSpace(data.selectedProfile.name))
                {
                    return new MojangAuthResult
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

            return new MojangAuthResult
            {
                Status = AuthCode.NoConnect
            };
        }

        public static MojangAuthResult Refresh(string username, string accessToken, string clientToken)
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
                        return new MojangAuthResult
                        {
                            Status = AuthCode.NoConnect
                        };
                    }

                    if (statusCode == HttpStatusCode.Gone)
                    {
                        return new MojangAuthResult
                        {
                            Status = AuthCode.NeedMicrosoftAuth
                        };

                    }
                    else if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
                    {
                        return new MojangAuthResult
                        {
                            Status = AuthCode.SessionExpired
                        };
                    }
                }

                var data = JsonConvert.DeserializeObject<AuthAnswer>(answer);
                Runtime.DebugWrite("Mojang Refresh " + data.accessToken);

                if (data != null && !string.IsNullOrWhiteSpace(data.accessToken) && !string.IsNullOrWhiteSpace(data.clientToken)
                    && data.selectedProfile != null && !string.IsNullOrWhiteSpace(data.selectedProfile.id) && !string.IsNullOrWhiteSpace(data.selectedProfile.name))
                {
                    return new MojangAuthResult
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

            return new MojangAuthResult
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

                if (answer == null)
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
        public static MojangAuthResult AuthFromToken(string token)
        {
            try
            {
                string answer = ToServer.HttpGet("https://api.minecraftservices.com/minecraft/profile", new Dictionary<string, string>()
                {
                    ["Authorization"] = "Bearer " + token
                });

                if (answer == null)
                {
                    return new MojangAuthResult
                    {
                        Status = AuthCode.DataError
                    };
                }

                var profile = JsonConvert.DeserializeObject<MojangProfile>(answer);

                return new MojangAuthResult
                {
                    Status = AuthCode.Successfully,
                    Login = profile.name,
                    UUID = profile.id,
                    AccesToken = token
                };
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite(ex);
            }


            return new MojangAuthResult
            {
                Status = AuthCode.DataError
            };
        }
    }
}
