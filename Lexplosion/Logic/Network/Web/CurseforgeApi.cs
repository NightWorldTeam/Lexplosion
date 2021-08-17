using Lexplosion.Logic.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Network
{
    static class CurseforgeApi
    {
        public static List<CurseforgeInstanceInfo> GetInstances(int pageSize, int index, ModpacksCategories categoriy, string searchFilter = "")
        {
            try
            {
                if (pageSize > 50)
                {
                    pageSize = 50;
                }

                string url;
                if (categoriy == ModpacksCategories.All)
                {
                    url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=4471&pageSize=" + pageSize + "&index=" + index + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
                }
                else
                {
                    url = "https://addons-ecs.forgesvc.net/api/v2/addon/search?gameId=432&sectionId=4471&pageSize=" + pageSize + "&index=" + index + "&categoryId=" + ((int)categoriy) + "&searchFilter=" + WebUtility.UrlEncode(searchFilter);
                }

                string answer;

                WebRequest req = WebRequest.Create(url);
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<List<CurseforgeInstanceInfo>>(answer);
                }

                return new List<CurseforgeInstanceInfo>();
            }
            catch
            {
                return new List<CurseforgeInstanceInfo>();
            }

        }

        public static List<CurseforgeFileInfo> GetInstanceInfo(string id)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create("https://addons-ecs.forgesvc.net/api/v2/addon/" + id + "/files");
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<List<CurseforgeFileInfo>>(answer);
                }
                else
                {
                    return new List<CurseforgeFileInfo>();
                }
            }
            catch
            {
                return new List<CurseforgeFileInfo>();
            }
        }

        public static CurseforgeInstanceInfo GetInstance(string id)
        {
            try
            {
                string answer;

                WebRequest req = WebRequest.Create("https://addons-ecs.forgesvc.net/api/v2/addon/" + id + "/");
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

                if (answer != null)
                {
                    return JsonConvert.DeserializeObject<CurseforgeInstanceInfo>(answer);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
