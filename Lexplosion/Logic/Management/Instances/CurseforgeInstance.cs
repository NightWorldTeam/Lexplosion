using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Instances
{
    class CurseforgeInstance : PrototypeInstance
    {
        public override bool CheckUpdates(InstancePlatformData infoData)
        {
            if (!Int32.TryParse(infoData.id, out _))
            {
                return true;
            }

            List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetProjectFiles(infoData.id); //получем информацию об этом модпаке

            //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии 
            foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
            {
                if (ver.id > infoData.instanceVersion)
                {
                    return true;
                
                }
            }

            return false;
        }

        public override InstanceData GetFullInfo(string localId, string externalId)
        {
            var data = CurseforgeApi.GetInstance(externalId);
            var images = new List<byte[]>();
            if (data.screenshots != null)
            {
                using (var webClient = new WebClient())
                {
                    foreach (var item in data.screenshots)
                    {
                        try
                        {
                            images.Add(webClient.DownloadData(item.url));
                        }
                        catch { }
                    }
                }
            }

            var projectFileId = data.latestFilesIndexes?[0]?.fileId;

            return new InstanceData
            {
                Categories = data.categories,
                Description = data.summary,
                Summary = data.summary,
                TotalDownloads = (long)data.downloadCount,
                GameVersion = (data.latestFilesIndexes != null && data.latestFilesIndexes.Count > 0) ? data.latestFilesIndexes[0].gameVersion : "",
                LastUpdate = (data.dateModified != null) ? DateTime.Parse(data.dateModified).ToString("dd MMM yyyy") : "",
                Modloader = data.ModloaderType,
                Images = images,
                WebsiteUrl = data.links?.websiteUrl,
                Changelog = (projectFileId != null) ? (CurseforgeApi.GetProjectChangelog(externalId, projectFileId.ToString()) ?? "") : ""
            };
        }

        public static List<Info> GetCatalog(int pageSize, int pageIndex, ModpacksCategories categoriy, string searchFilter)
        {
            List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, ModpacksCategories.All, searchFilter);
            var result = new List<Info>();

            foreach (var instance in curseforgeInstances)
            {
                // проверяем версию игры
                if (instance.latestFilesIndexes != null && instance.latestFilesIndexes.Count > 0 && instance.latestFilesIndexes[0].gameVersion != null)
                {
                    string author = null;
                    if (instance.authors != null && instance.authors.Count > 0)
                    {
                        author = instance.authors[0].name;
                    }

                    result.Add(new Info()
                    {
                        Name = instance.name,
                        Author = author,
                        Categories = instance.categories,
                        Summary = instance.summary,
                        Description = instance.summary,
                        GameVersion = instance.latestFilesIndexes[0].gameVersion,
                        WebsiteUrl = instance.links?.websiteUrl,
                        LogoUrl = instance.logo?.url,
                        ExternalId = instance.id.ToString()
                    });
                }
            }

            return result;
        }
    }
}
