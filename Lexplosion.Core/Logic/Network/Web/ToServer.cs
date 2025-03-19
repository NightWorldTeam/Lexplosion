﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Network
{
    public static class ToServer
    {
        /// <summary>
        /// Проверяет есть ли на сервере новая версия лаунчера
        /// </summary>
        /// <returns>Возвращает версию нового лаунчера. -1 если новых версий нет.</returns>
        public static int CheckLauncherUpdates()
        {
            try
            {
                int version = Int32.Parse(HttpPost(LaunсherSettings.URL.LauncherParts + "launcherVersion.html"));

                if (version > LaunсherSettings.version)
                {
                    return version;
                }

                return -1;

            }
            catch
            {
                return -1;
            }
        }

        public static bool ServerIsOnline()
        {
            return HttpPost(LaunсherSettings.URL.Base + "api/onlineStatus") == "online";
        }

        public static async Task<int> GetMcServerOnline(MinecraftServerInstance server)
        {
            int count = -1;
            await Task.Run(() =>
            {
                string result = ToServer.HttpGet($"https://api.mcsrvstat.us/3/{server.Address}");
                if (result == null) return;

                try
                {
                    var data = JsonConvert.DeserializeObject<McServerOnlineData>(result);
                    if (data?.Players != null)
                    {
                        count = data.Players.Online;
                    }
                }
                catch { }
            });

            return count;
        }

        public static List<MinecraftServerInstance> GetMinecraftServersList()
        {
            string result = HttpGet(LaunсherSettings.URL.Base + "/api/minecraftServers/list");

            if (result == null) return new List<MinecraftServerInstance>();

            try
            {
                var data = JsonConvert.DeserializeObject<List<MinecraftServerInstance>>(result);

                for (int i = 0; i < data.Count; i++)
                {
                    MinecraftServerInstance element = data[i];
                    if (!element.IsValid()) data.Remove(element);
                }

                Random rand = new Random();
                rand.Shuffle(data);
                return data;
            }
            catch
            {
                return new List<MinecraftServerInstance>();
            }
        }

        public static T ProtectedRequest<T>(string url) where T : ProtectedManifest
        {
            Random rnd = new Random();

            string str = rnd.GenerateString(32);
            string str2 = rnd.GenerateString(32);

            using (SHA1 sha = new SHA1Managed())
            {
                string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

                int d = 32 - key.Length;
                for (int i = 0; i < d; i++)
                {
                    key += str2[i];
                }

                Dictionary<string, string> data = new Dictionary<string, string>()
                {
                    ["str"] = str,
                    ["str2"] = str2,
                    ["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
                };

                try
                {
                    Runtime.DebugWrite(url);
                    string answer = HttpPost(url, data);

                    if (answer != null && answer != "")
                    {
                        byte[] IV = Encoding.UTF8.GetBytes(str.Substring(0, 16));
                        byte[] decripted = Cryptography.AesDecode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), IV);
                        answer = Encoding.UTF8.GetString(decripted);

                        T filesData = JsonConvert.DeserializeObject<T>(answer);
                        if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
                        {
                            return filesData;
                        }
                        else
                        {
                            return default;
                        }
                    }
                    else
                    {
                        return default;
                    }
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite(ex);
                    return default;
                }
            }
        }

        /// <summary>
        /// Получает маниест для майкрафт версии
        /// </summary>
        /// <param name="version">Версия майнкрафта</param>
        /// <param name="clientType">тип клиента</param>
        /// <param name="isNwClient">Включен ли NightWorldClient</param>
        /// <param name="modloaderVersion">Версия модлоадера, если он передан в clientType</param>
        /// <param name="optifineVersion">Версия оптифайна, если он не нужен то null</param>
        public static VersionManifest GetVersionManifest(string version, ClientType clientType, bool isNwClient, string modloaderVersion = null, string optifineVersion = null)
        {
            try
            {
                string modloaderUrl = "";
                if (!string.IsNullOrWhiteSpace(modloaderVersion))
                {
                    if (clientType != ClientType.Vanilla)
                    {
                        modloaderUrl = "/" + clientType.ToString().ToLower() + "/";
                        modloaderUrl += modloaderVersion;
                    }
                }

                var filesData = ProtectedRequest<ProtectedVersionManifest>(LaunсherSettings.URL.VersionsData + WebUtility.UrlEncode(version) + modloaderUrl);
                if (filesData?.version != null)
                {
                    if (isNwClient && filesData.version.NightWorldClientData != null)
                    {
                        filesData.version.IsNightWorldClient = true;
                    }

                    Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
                    foreach (string lib in filesData.libraries.Keys)
                    {
                        if (filesData.libraries[lib].os == null || filesData.libraries[lib].os.Contains("windows"))
                        {
                            libraries[lib] = filesData.libraries[lib].GetLibInfo;
                        }
                    }

                    VersionManifest manifest = new VersionManifest
                    {
                        version = filesData.version,
                        libraries = libraries
                    };

                    if (optifineVersion != null)
                    {
                        var optifineData = ProtectedRequest<ProtectedInstallerManifest>(LaunсherSettings.URL.InstallersData + WebUtility.UrlEncode(version) + "/optifine/" + optifineVersion);
                        if (optifineData == null) return null;

                        foreach (string lib in optifineData.libraries.Keys)
                        {
                            if (optifineData.libraries[lib].os == null || optifineData.libraries[lib].os.Contains("windows"))
                            {
                                libraries[lib] = optifineData.libraries[lib].GetLibInfo;
                                libraries[lib].additionalInstallerType = AdditionalInstallerType.Optifine;
                            }
                        }

                        optifineData.version.installerVersion = optifineVersion;
                        manifest.version.AdditionalInstaller = optifineData.version;
                    }

                    return manifest;
                }
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Exception " + ex);
            }

            return null;
        }

        /// <summary>
        /// Защищенный запрос для передачи данных пользователя (регистрация, авторизация).
        /// </summary>
        /// <typeparam name="T">Возвращаемый тип</typeparam>
        /// <param name="url">url</param>
        /// <param name="data">Данные</param>
        /// <param name="errorResult">Если сервер вернул ошибку, то ее код будет помещен сюда. В случае успеха будет null </param>
        /// <returns>Вернет базовое значение при ошибке, поместив код ошибки в errorResult (если он есть).</returns>
        public static T ProtectedUserRequest<T>(string url, string data, out string errorResult) where T : ProtectedManifest
        {
            Random rnd = new Random();

            string str = rnd.GenerateString(32);
            string str2 = rnd.GenerateString(32);
            string salt = rnd.GenerateString(32);

            using (SHA1 sha = new SHA1Managed())
            {
                string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

                int d = 32 - key.Length;
                for (int i = 0; i < d; i++)
                {
                    key += str2[i];
                }

                data = Convert.ToBase64String(Encoding.UTF8.GetBytes(data)) + ":" + str;
                string planText = Convert.ToBase64String(Encoding.UTF8.GetBytes(data)) + ":" + salt;
                byte[] encrypted = Cryptography.AesEncode(planText, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));

                var fullData = new Dictionary<string, string>()
                {
                    ["data"] = Convert.ToBase64String(encrypted),
                    ["str"] = str,
                    ["str2"] = str2,
                    ["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
                };

                string answer;
                try
                {
                    answer = HttpPost(url, fullData);
                    Runtime.DebugWrite(answer);

                    if (answer == null)
                    {
                        errorResult = null;
                        return default;
                    }
                    else if (answer.StartsWith("ERROR:")) // если результат является кодом ошибки, то возращаем его не декодируя
                    {
                        errorResult = answer;
                        return default;
                    }
                    else
                    {
                        byte[] IV = Encoding.UTF8.GetBytes(str.Substring(0, 16));
                        byte[] decripted = Cryptography.AesDecode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), IV);
                        answer = Encoding.UTF8.GetString(decripted);
                        T answerData = JsonConvert.DeserializeObject<T>(answer);

                        if (answerData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(answerData.str + ":" + LaunсherSettings.secretWord))))
                        {
                            errorResult = null;
                            return answerData;
                        }
                        else
                        {
                            errorResult = null;
                            return default;
                        }
                    }
                }
                catch
                {
                    errorResult = null;
                    return default;
                }
            }
        }

        public static string HttpPost(string url, Dictionary<string, string> data = null)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                string dataS = "";

                if (data != null)
                {
                    foreach (var value in data)
                    {
                        dataS += WebUtility.UrlEncode(value.Key) + "=" + WebUtility.UrlEncode(value.Value) + "&";
                    }
                }

                byte[] byteArray = Encoding.UTF8.GetBytes(dataS);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                string line;
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            line = reader.ReadToEnd();
                        }
                    }
                    response.Close();
                }

                return line;
            }
            catch
            {
                return null;
            }
        }

        public static string HttpGet(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create(url);
				((HttpWebRequest)req).UserAgent = "Mozilla/5.0";
				if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        req.Headers.Add(header.Key, header.Value);
                    }
                }

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

                return answer;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite($"url: {url}, Exception: {ex}, stack trace: {new System.Diagnostics.StackTrace()}");
                return null;
            }
        }

        public static bool IsHtmlPage(string url)
        {
            var request = HttpWebRequest.Create(url);
            request.Method = "HEAD";

            return (request.GetResponse().ContentType.StartsWith("text/html"));
        }

        public static string HttpPostJson(string url, string data, out HttpStatusCode? httpStatus, Dictionary<string, string> headers = null)
        {
            httpStatus = null;

            try
            {
                WebRequest req = WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/json";

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        req.Headers.Add(header.Key, header.Value);
                    }
                }

                byte[] byteArray = Encoding.UTF8.GetBytes(data);

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

                httpStatus = HttpStatusCode.OK;
                return answer;
            }
            catch (WebException ex)
            {
                WebExceptionStatus status = ex.Status;

                if (status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
                    httpStatus = httpResponse.StatusCode;
                }
            }
            catch { }

            return null;
        }
    }
}
