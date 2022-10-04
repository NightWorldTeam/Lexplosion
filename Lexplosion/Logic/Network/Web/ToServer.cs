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

        public static bool CheckLauncherUpdates()
        {
            try
            {
                int version = Int32.Parse(HttpPost(LaunсherSettings.URL.LauncherParts + "launcherVersion.html"));
                return version > LaunсherSettings.version;

            }
            catch { return false; }
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

        public static List<string> GetModloadersList(string gameVersion, ModloaderType modloaderType)
        {
            string modloader;
            if (modloaderType != ModloaderType.Vanilla)
            {
                modloader = "/"+ modloaderType.ToString().ToLower() + "/";
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

        //функция получает манифест для майкрафт версии
        public static VersionManifest GetVersionManifest(string version, ModloaderType modloader, string modloaderVersion = "")
        {
            string[] chars = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string str = "";
            string str2 = "";
            Random rnd = new Random();

            for (int i = 0; i < 32; i++)
            {
                str += chars[rnd.Next(0, chars.Length)];
                str2 += chars[rnd.Next(0, chars.Length)];
            }

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
                    string modloaderUrl = "";
                    if (!string.IsNullOrEmpty(modloaderVersion))
                    {
                        if (modloader != ModloaderType.Vanilla)
                        {
                            modloaderUrl = "/" + modloader.ToString().ToLower() + "/";
                            modloaderUrl += modloaderVersion;
                        }
                    }

                    Console.WriteLine("URL " + LaunсherSettings.URL.VersionsData + WebUtility.UrlEncode(version) + modloaderUrl);
                    string answer = HttpPost(LaunсherSettings.URL.VersionsData + WebUtility.UrlEncode(version) + modloaderUrl, data);

                    if (answer != null && answer != "")
                    {
                        answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));

                        DataVersionManifest filesData = JsonConvert.DeserializeObject<DataVersionManifest>(answer);
                        if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
                        {
                            Dictionary<string, LibInfo> libraries = new Dictionary<string, LibInfo>();
                            foreach (string lib in filesData.libraries.Keys)
                            {
                                if (filesData.libraries[lib].os == "all" || filesData.libraries[lib].os == "windows")
                                {
                                    libraries[lib] = new LibInfo
                                    {
                                        notArchived = filesData.libraries[lib].notArchived,
                                        url = filesData.libraries[lib].url,
                                        obtainingMethod = filesData.libraries[lib].obtainingMethod,
                                        isNative = filesData.libraries[lib].isNative,
                                        activationConditions = filesData.libraries[lib].activationConditions,
                                        notLaunch = filesData.libraries[lib].notLaunch
                                    };
                                }
                            }

                            VersionManifest ret = new VersionManifest
                            {
                                version = filesData.version,
                                libraries = libraries
                            };

                            return ret;
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
                catch
                {
                    return null;
                }
            }
        }

        public class AuthResult
        {
            public AuthCode Status;
            public string Login;
            public string UUID;
            public string AccesToken;
            public string SessionToken;
            public string AccessID;
        }

        public static AuthResult Authorization(string login, string accessData, out int baseStatus)
        {
            baseStatus = 0;

            string[] chars = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string str = "";
            string str2 = "";
            string salt = "";
            Random rnd = new Random();

            for (int i = 0; i < 32; i++)
            {
                str += chars[rnd.Next(0, chars.Length)];
                str2 += chars[rnd.Next(0, chars.Length)];
                salt += chars[rnd.Next(0, chars.Length)];
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

                //try
                {
                    answer = HttpPost(LaunсherSettings.URL.Account + "auth", data);
                    Console.WriteLine(answer);

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
                //catch
                //{
                //    return null;
                //}
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
            //try 
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
            //catch (Exception e)
            //{
            //    Console.WriteLine(url + " " + e);
            //    return null;
            //}
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

                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("ERROR " + reader.ReadToEnd());
                }
            }
            catch { }

            return null;
        }
    }
}
