using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Net;

namespace Lexplosion.Logic.Management.Instances
{
    class ModrinthInstance : PrototypeInstance
    {
        public override bool CheckUpdates(InstancePlatformData infoData, string localId)
        {
            ModrinthProjectInfo info = ModrinthApi.GetProject(infoData.id);

            if (info.Versions == null || info.Versions.Count == 0) return false;
            return info.Versions[info.Versions.Count - 1] != infoData.instanceVersion;
        }

        private static List<ModrinthCategory> ParseCategories(List<string> data)
        {
            var categories = new List<ModrinthCategory>();
            if (data != null)
            {
                foreach (string category in data)
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        categories.Add(new ModrinthCategory
                        {
                            Id = category
                        });
                    }
                }
            }

            return categories;
        }

        public override InstanceData GetFullInfo(string localId, string externalId)
        {
            var data = ModrinthApi.GetProject(externalId);

            if (data == null)
            {
                return null;
            }

            var images = new List<byte[]>();
            if (data.Images != null && data.Images.Count > 0)
            {
                var perfomer = new TasksPerfomer(3, data.Images.Count);
                foreach (var item in data.Images)
                {
                    perfomer.ExecuteTask(delegate ()
                    {
                        using (var webClient = new WebClient())
                        {
                            if (item.ContainsKey("url"))
                            {
                                try
                                {
                                    images.Add(webClient.DownloadData(item["url"]));
                                }
                                catch { }
                            }
                        }
                    });
                }

                perfomer.WaitEnd();
            }

            ClientType clientType = ClientType.Vanilla;
            if (data.Loaders.Contains("fabric"))
            {
                clientType = ClientType.Fabric;
            }
            else if (data.Loaders.Contains("forge"))
            {
                clientType = ClientType.Forge;
            }
            else if (data.Loaders.Contains("quilt"))
            {
                clientType = ClientType.Quilt;
            }

            var categories = ParseCategories(data.Categories);

            string date;
            try
            {
                date = (data.Updated != null) ? DateTime.Parse(data.Updated).ToString("dd MMM yyyy") : "";
            }
            catch
            {
                date = "";
            }

            return new InstanceData
            {
                Source = InstanceSource.Modrinth,
                Categories = categories,
                Description = data.Summary,
                Summary = data.Summary,
                TotalDownloads = data.Downloads,
                GameVersion = data.GameVersions[data.GameVersions.Count - 1],
                LastUpdate = date,
                Modloader = clientType,
                Images = images,
                WebsiteUrl = data.WebsiteUrl,
                Changelog = ""
            };
        }

        public override List<InstanceVersion> GetVersions(string externalId)
        {
            List<ModrinthProjectFile> versions = ModrinthApi.GetProjectFiles(externalId);
            var data = new List<InstanceVersion>();
            foreach (ModrinthProjectFile file in versions)
            {
                string date;
                try
                {
                    date = (file.Date != null) ? DateTime.Parse(file.Date).ToString("dd MMM yyyy") : "";
                }
                catch
                {
                    date = "";
                }

                data.Add(new InstanceVersion
                {
                    FileName = file.Name,
                    Date = date,
                    Id = file.FileId,
                    Status = (file.Status == "release") ? ReleaseType.Release : ((file.Status == "beta") ? ReleaseType.Beta : ReleaseType.Alpha)
                });
            }

            return data;
        }

        public static List<Info> GetCatalog(int pageSize, int pageIndex, IProjectCategory categoriy, string searchFilter, string sortField, string gameVersion)
        {
            sortField = ModrinthApi.SearchFilters.Relevance;
            List<ModrinthCtalogUnit> curseforgeInstances = ModrinthApi.GetInstances(pageSize, pageIndex, categoriy, sortField, searchFilter, gameVersion);
            var result = new List<Info>();

            foreach (var instance in curseforgeInstances)
            {
                var categories = ParseCategories(instance.Categories);

                result.Add(new Info()
                {
                    Name = instance.Title,
                    Author = instance.Author,
                    Categories = categories,
                    Summary = instance.Summary,
                    Description = instance.Summary,
                    GameVersion = instance.GameVersions[instance.GameVersions.Count - 1],
                    WebsiteUrl = "https://modrinth.com/modpack/" + instance.Slug,
                    LogoUrl = instance.LogoUrl,
                    ExternalId = instance.ProjectId
                });
            }

            return result;
        }
    }
}
