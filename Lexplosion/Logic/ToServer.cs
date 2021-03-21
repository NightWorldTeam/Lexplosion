using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Lexplosion.Objects;
using System.Collections;

namespace Lexplosion.Logic
{

    static class ToServer
    {
        private class FilesList : InstanceFiles //этот класс нужен для декодирования json
        {
            public string code;
            public string str;
        }

        static public bool CheckLauncherUpdates()
        {
            try
            {
                int version = Int32.Parse(HttpPost("windows/launcherVersion.html"));

                if (version > LaunсherSettings.version)
                    return true;

                return false;

            } catch { return false; }

        }

        static public Dictionary<string, string> GetModpaksList()
        {
            try
            {
                string answer = HttpPost("filesList/modpacksList.json");

                Dictionary<string, string> list = JsonConvert.DeserializeObject<Dictionary<string, string>>(answer);

                return list;

            } catch {
                return new Dictionary<string, string>();
            }

        }

        static public InstanceFiles GetFilesList(string instanceId)
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

            SHA1 sha = new SHA1Managed();
            string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

            int d = 32 - key.Length;
            for (int i = 0; i < d; i++)
            {
                key += str2[i];
            }

            List<List<string>> data = new List<List<string>>() { };
            data.Add(new List<string>() { "str", str });
            data.Add(new List<string>() { "str2", str2 });
            data.Add(new List<string>() { "code", Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord))) });

            string answer = "";

            try
            {
                answer = HttpPost("directoryFiles.php?modpack=" + WebUtility.UrlEncode(instanceId), data);

                if (answer != null)
                {
                    answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));
                    FilesList filesData = JsonConvert.DeserializeObject<FilesList>(answer);

                    if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
                    {
                        InstanceFiles ret = new InstanceFiles
                        {
                            data = filesData.data,
                            version = filesData.version,
                            libraries = filesData.libraries,
                            natives = filesData.natives
                        };

                        return ret;

                    } else {
                        return null;
                    }

                } else {
                    return null;
                }

            } catch {
                return null;
            }
        }

        static public Dictionary<string, string> Authorization(string login, string password, string email = "")
        {
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

            SHA1 sha = new SHA1Managed();
            string key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str2 + ":" + LaunсherSettings.secretWord)));

            int d = 32 - key.Length;
            for (int i = 0; i < d; i++)
            {
                key += str2[i];
            }
            //MessageBox.Show(key);

            List<List<string>> data = new List<List<string>>() { };
            data.Add(new List<string>() { "login", login });
            data.Add(new List<string>() { "password", Convert.ToBase64String(AesСryp.Encode(Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(password)) + ":" + str)) + ":" + salt, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)))) });
            data.Add(new List<string>() { "str", str });
            data.Add(new List<string>() { "str2", str2 });
            data.Add(new List<string>() { "code", Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord))) });

            Dictionary<string, string> response = new Dictionary<string, string>();
            string answer = "";

            try
            {
                answer = HttpPost("authorization.php", data);

                if (answer == null)
                {
                    return null;

                } else if(answer == "ERROR:1") {

                    response.Add("status", "ERROR:1");
                    return response;

                } else {

                    answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));
                    Dictionary<string,string> userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(answer);

                    if (userData["code"] == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(userData["str"] + ":" + LaunсherSettings.secretWord))))
                    {
                        if (userData.ContainsKey("login") && userData.ContainsKey("UUID") && userData.ContainsKey("accesToken"))
                        {
                            response.Add("status", "OK");
                            response.Add("login", userData["login"]);
                            response.Add("UUID", userData["UUID"]);
                            response.Add("accesToken", userData["accesToken"]);

                            return response;

                        } else {
                            return null;
                        }

                    } else {
                        return null;
                    }
                }

            } catch {
                return null;
            }

        }

        static public string HttpPost(string url, List<List<string>> data = null, bool outside = false)
        {
            if (!outside)
                url = LaunсherSettings.serverUrl + url;
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                //request.Timeout = 1000;
                string dataS = "";

                if (data != null)
                {
                    foreach (List<string> p in data)
                    {
                        dataS += WebUtility.UrlEncode(p[0]) + "=" + WebUtility.UrlEncode(p[1]) + "&";
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
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        line = reader.ReadToEnd();
                    }
                }
                response.Close();

                return line;

            } catch {
                return null;
            }
        }
    }
}
