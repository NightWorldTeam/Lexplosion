using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Network
{
    static class ToServer
    {
        public static List<JavaVersion> GetJavaVersions()
        {
            try
            {
                string answer = HttpGet(LaunсherSettings.URL.JavaData);
                return JsonConvert.DeserializeObject<List<JavaVersion>>(answer);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Проверяет есть ли на сервере новая версия лаунчера
        /// </summary>
        /// <returns>Возвращает версию нового лаунчера. И -1, если новых версий нет.</returns>
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

        public static List<MCVersionInfo> GetVersionsList()
        {
            try
            {
                string answer = HttpGet(LaunсherSettings.URL.VersionsData);
                if (answer != null)
                {
                    List<MCVersionInfo> data = JsonConvert.DeserializeObject<List<MCVersionInfo>>(answer);
                    return data ?? new List<MCVersionInfo>();
                }
                else
                {
                    return new List<MCVersionInfo>();
                }
            }
            catch
            {
                return new List<MCVersionInfo>();
            }

        }

        public static bool ServerIsOnline()
        {
            return HttpPost(LaunсherSettings.URL.Base + "/api/onlineStatus") == "online";
        }

        public static List<string> GetModloadersList(string gameVersion, ModloaderType modloaderType)
        {
            string modloader;
            if (modloaderType != ModloaderType.Vanilla)
            {
                modloader = "/" + modloaderType.ToString().ToLower() + "/";
            }
            else
            {
                return new List<string>();
            }

            try
            {
                string answer = HttpGet(LaunсherSettings.URL.VersionsData + gameVersion + modloader);
                if (answer != null)
                {
                    List<string> data = JsonConvert.DeserializeObject<List<string>>(answer);
                    return data ?? new List<string>();
                }
                else
                {
                    return new List<string>();
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        public static List<string> GetOptifineVersions(string gameVersion)
        {
            try
            {
                string answer = HttpGet(LaunсherSettings.URL.InstallersData + gameVersion + "/optifine");
                if (answer != null)
                {
                    List<string> data = JsonConvert.DeserializeObject<List<string>>(answer);
                    return data ?? new List<string>();
                }
                else
                {
                    return new List<string>();
                }
            }
            catch
            {
                return new List<string>();
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
                        answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));

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
                catch
                {
                    return default;
                }
            }
        }

        //функция получает манифест для майкрафт версии
        public static VersionManifest GetVersionManifest(string version, ModloaderType modloader, string modloaderVersion = null, string optifineVersion = null)
        {
            try
            {
                string modloaderUrl = "";
                if (!string.IsNullOrEmpty(modloaderVersion))
                {
                    if (modloader != ModloaderType.Vanilla)
                    {
                        modloaderUrl = "/" + modloader.ToString().ToLower() + "/";
                        modloaderUrl += modloaderVersion;
                    }
                }

                var filesData = ProtectedRequest<ProtectedVersionManifest>(LaunсherSettings.URL.VersionsData + WebUtility.UrlEncode(version) + modloaderUrl);
                if (filesData != null)
                {
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
                        if (optifineData != null)
                        {
                            foreach (string lib in optifineData.libraries.Keys)
                            {
                                if (optifineData.libraries[lib].os == null || optifineData.libraries[lib].os.Contains("windows"))
                                {
                                    libraries[lib] = optifineData.libraries[lib].GetLibInfo;
                                    libraries[lib].additionalInstallerType = AdditionalInstallerType.Optifine;
                                }
                            }

                            optifineData.version.installerVersion = optifineVersion;
                            manifest.version.additionalInstaller = optifineData.version;
                        }
                    }

                    return manifest;
                }
            }
            catch { }

            return null;
        }

        public static AuthResult Authorization(string login, string accessData, out int baseStatus)
        {
            baseStatus = 0;

            string str = "";
            string str2 = "";
            string salt = "";
            Random rnd = new Random();

            for (int i = 0; i < 32; i++)
            {
                str += rnd.GenerateString(1);
                str2 += rnd.GenerateString(1);
                salt += rnd.GenerateString(1);
            }

            using (SHA1 sha = new SHA1Managed())
            {
                string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

                int d = 32 - key.Length;
                for (int i = 0; i < d; i++)
                {
                    key += str2[i];
                }

                accessData = Convert.ToBase64String(Encoding.UTF8.GetBytes(accessData)) + ":" + str;
                string planText = Convert.ToBase64String(Encoding.UTF8.GetBytes(accessData)) + ":" + salt;
                byte[] encrypted = AesСryp.Encode(planText, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));

                Dictionary<string, string> data = new Dictionary<string, string>()
                {
                    ["login"] = login,
                    ["accessData"] = Convert.ToBase64String(encrypted),
                    ["str"] = str,
                    ["str2"] = str2,
                    ["code"] = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord)))
                };

                AuthResult response = new AuthResult();
                string answer;

                try
                {
                    answer = HttpPost(LaunсherSettings.URL.Account + "auth", data);
                    Runtime.DebugWrite(answer);

                    if (answer == null)
                    {
                        return null;

                    }
                    else if (answer == "ERROR:0")
                    {
                        response.Status = AuthCode.NoConnect;
                        return response;
                    }
                    else if (answer == "ERROR:1")
                    {
                        response.Status = AuthCode.DataError;
                        return response;
                    }
                    else if (answer == "ERROR:2")
                    {
                        response.Status = AuthCode.SessionExpired;
                        return response;
                    }
                    else
                    {
                        answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));
                        Dictionary<string, string> userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(answer);

                        if (userData["code"] == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(userData["str"] + ":" + LaunсherSettings.secretWord))))
                        {
                            if (userData.ContainsKey("login") && userData.ContainsKey("UUID") && userData.ContainsKey("accesToken"))
                            {
                                response.Status = AuthCode.Successfully;
                                response.Login = userData["login"];
                                response.UUID = userData["UUID"];
                                response.AccesToken = userData["accesToken"];
                                response.SessionToken = userData["sessionToken"];
                                response.AccessID = userData["accessID"];

                                Int32.TryParse(userData["baseStatus"], out baseStatus);

                                return response;
                            }
                            else
                            {
                                return null;
                            }

                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string HttpPost(string url, Dictionary<string, string> data = null) // TODO: List<string> заменить на массив
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                //request.Timeout = 1000;
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

        public static string HttpGet(string url, List<KeyValuePair<string, string>> headers = null)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create(url);
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
            catch
            {
                return null;
            }
        }

        public static string HttpPostJson(string url, string data, out HttpStatusCode? httpStatus)
        {
            httpStatus = null;

            try
            {
                WebRequest req = WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/json";

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
