using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Lexplosion.Logic.Network.Web
{
    public class MojangApi
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

        public MojangApi(ToServer toServer)
        {
            _toServer = toServer;
        }

        private Random _random = new Random();
        private readonly ToServer _toServer;

        public MojangAuthResult Auth(string username, string password)
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
                        "\"clientToken\": \"" + _random.GenerateString(20) + "\"" +
                    "}";

                string answer = _toServer.HttpPostJson("https://authserver.mojang.com/authenticate", payload, out HttpStatusCode? statusCode);

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

        public MojangAuthResult Refresh(string username, string accessToken, string clientToken)
        {
            try
            {
                string payload =
                    "{" +
                        "\"accessToken\": \"" + accessToken + "\"," +
                        "\"clientToken\": \"" + clientToken + "\"" +
                    "}";

                string answer = _toServer.HttpPostJson("https://authserver.mojang.com/refresh", payload, out HttpStatusCode? statusCode);

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

        /// <summary>
        /// Возвращает токен для майнрафта.
        /// </summary>
        /// <param name="microsoftData">json массив microsoft данных (uhs и xsts_token)</param>
        /// <returns>Возвращает токен, который можно использовать для получения аккаунта.</returns>
        public string GetToken(string microsoftData)
        {
            try
            {

                var mcfData = JsonConvert.DeserializeObject<MicrosoftData>(microsoftData);
                string payload =
                    "{" +
                        "\"identityToken\" : \"XBL3.0 x=" + mcfData.uhs + ";" + mcfData.xsts_token + "\"," +
                        "\"ensureLegacyEnabled\" : true" +
                    "}";

                string answer = _toServer.HttpPostJson("https://api.minecraftservices.com/authentication/login_with_xbox", payload, out _);

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
        public MojangAuthResult AuthFromToken(string token)
        {
            try
            {
                string answer = _toServer.HttpGet("https://api.minecraftservices.com/minecraft/profile", new Dictionary<string, string>()
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
