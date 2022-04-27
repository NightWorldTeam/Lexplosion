using Lexplosion.Global;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Logic.Network
{
    static class NightWorldApi
    {
        private class DataNInstanceManifest : NightWorldManifest //этот класс нужен для декодирования json в GetInstanceManifest
        {
            public string code;
            public string str;
        }

        public static Dictionary<string, NWInstanceInfo> GetInstancesList()
        {
            try
            {
                string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData);
                Dictionary<string, NWInstanceInfo> list = JsonConvert.DeserializeObject<Dictionary<string, NWInstanceInfo>>(answer);

                return list;
            }
            catch
            {
                return new Dictionary<string, NWInstanceInfo>();
            }
        }

        public static int GetInstanceVersion(string id)
        {
            try
            {
                string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(id) + "/version");
                Int32.TryParse(answer, out int idInt);

                return idInt;
            }
            catch
            {
                return 0;
            }
        }

        // Функция получает манифест для NightWorld модпаков
        public static NightWorldManifest GetInstanceManifest(string instanceId) // TODO: одинаковые блоки кода в этих двух функция вынести в другую функцию
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

                List<List<string>> data = new List<List<string>>() { };
                data.Add(new List<string>() { "str", str });
                data.Add(new List<string>() { "str2", str2 });
                data.Add(new List<string>() { "code", Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(str + ":" + LaunсherSettings.secretWord))) });

                try
                {
                    string answer = ToServer.HttpPost(LaunсherSettings.URL.ModpacksData + WebUtility.UrlEncode(instanceId) + "/manifest", data);

                    if (answer != null)
                    {
                        answer = AesСryp.Decode(Convert.FromBase64String(answer), Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(str.Substring(0, 16)));
                        DataNInstanceManifest filesData = JsonConvert.DeserializeObject<DataNInstanceManifest>(answer);

                        if (filesData.code == Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(filesData.str + ":" + LaunсherSettings.secretWord))))
                        {

                            NightWorldManifest ret = new NightWorldManifest
                            {
                                data = filesData.data,
                                version = filesData.version
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
    }
}
