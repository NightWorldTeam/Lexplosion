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

            var lastElem = info.Versions.GetLastElement();
            return lastElem != null && lastElem != infoData.instanceVersion;
        }

        private static List<ModrinthCategory> ParseCategories(List<string> data)
        {
            var categories = new List<ModrinthCategory>();
            if (data != null)
            {
                foreach (string category in data)
                {
                    if (!string.IsNullOrWhiteSpace(category))
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
                    if (item == null || !item.ContainsKey("url")) continue;

                    perfomer.ExecuteTask(delegate ()
                    {
                        using (var webClient = new WebClient())
                        {
                            try
                            {
                                images.Add(webClient.DownloadData(item["url"]));
                            }
                            catch { }
                        }
                    });
                }

                perfomer.WaitEnd();
            }

            ClientType clientType = ClientType.Vanilla;
            if (data.Loaders != null)
            {
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

            var gameVer = (data.GameVersions != null && data.GameVersions.Count > 0) ? data.GameVersions[data.GameVersions.Count - 1] : "";

            return new InstanceData
            {
                Source = InstanceSource.Modrinth,
                Categories = categories,
                Description = data.Summary,
                Summary = data.Summary,
                TotalDownloads = data.Downloads,
                GameVersion = gameVer,
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
                if (file == null) continue;

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
    }
}
