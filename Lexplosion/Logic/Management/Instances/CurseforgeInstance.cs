using System;
using System.Collections.Generic;
using System.Net;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;

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
                Source = InstanceSource.Curseforge,
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

        public override List<InstanceVersion> GetVersions(string externalId)
        {
            List<CurseforgeFileInfo> files = CurseforgeApi.GetProjectFiles(externalId);

            if (files != null)
            {
                var versions = new List<InstanceVersion>();
                foreach (var file in files)
                {
                    ReleaseType status;
                    if (file.releaseType == 1)
                    {
                        status = ReleaseType.Release;
                    }
                    else if (file.releaseType == 2)
                    {
                        status = ReleaseType.Beta;
                    }
                    else
                    {
                        status = ReleaseType.Alpha;
                    }

                    versions.Add(new InstanceVersion
                    {
                        FileName = file.fileName,
                        Id = file.id.ToString(),
                        Status = status,
                        Date = file.fileDate
                    });

                    Console.WriteLine(file.fileName + " " + file.id);
                }

                return versions;
            }
            else
            {
                return null;
            }
        }

        public static List<Info> GetCatalog(int pageSize, int pageIndex, int categoriy, string searchFilter)
        {
            List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(pageSize, pageIndex * pageSize, categoriy, searchFilter);
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
