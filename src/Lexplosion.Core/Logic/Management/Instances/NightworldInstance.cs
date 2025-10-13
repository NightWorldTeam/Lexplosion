using Lexplosion.Global;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Net;

namespace Lexplosion.Logic.Management.Instances
{
    class NightworldInstance : PrototypeInstance
    {
        private readonly INightWorldFileServicesContainer _services;

        public NightworldInstance(INightWorldFileServicesContainer services)
        {
            _services = services;
        }

        public override bool CheckUpdates(string localId)
        {
            var infoData = _services.DataFilesService.GetPlatfromData(localId);
            if (string.IsNullOrWhiteSpace(infoData?.id))
            {
                return false;
            }

            int version = _services.NwApi.GetInstanceVersion(infoData.id);
            return (infoData.instanceVersion.ToInt32() < version);
        }

        public override InstanceData GetFullInfo(string localId, string externalId)
        {
            var data = _services.NwApi.GetInstanceInfo(externalId);
            var images = new List<byte[]>();

            if (data != null)
            {
                if (data.Images != null)
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Proxy = null;
                        foreach (var item in data.Images)
                        {
                            try
                            {
                                images.Add(webClient.DownloadData(item));
                            }
                            catch { }
                        }
                    }
                }

                return new InstanceData
                {
                    Source = InstanceSource.Nightworld,
                    Categories = data.Categories,
                    Description = data.Description,
                    Summary = data.Summary,
                    TotalDownloads = data.DownloadCounts,
                    GameVersion = data.GameVersion,
                    LastUpdate = (new DateTime(1970, 1, 1).AddSeconds(data.LastUpdate)).ToString("dd MMM yyyy"),
                    Modloader = data.Modloader,
                    Images = images,
                    WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + externalId,
                    Changelog = ""
                };
            }
            else
            {
                return null;
            }
        }

        public override List<InstanceVersion> GetVersions(string externalId)
        {
            return null;
        }

        //public static List<Info> GetCatalog(int pageSize, int pageIndex)
        //{
        //	Dictionary<string, NightWorldApi.InstanceInfo> nwInstances = _services.NwApi.GetInstancesList();
        //	var result = new List<Info>();

        //	var i = 0;
        //	foreach (string nwModpack in nwInstances.Keys)
        //	{
        //		if (i < pageSize * (pageIndex + 1))
        //		{
        //			// проверяем версию игры
        //			if (nwInstances[nwModpack].GameVersion != null)
        //			{
        //				result.Add(new Info()
        //				{
        //					Name = nwInstances[nwModpack].Name,
        //					Author = nwInstances[nwModpack].Author,
        //					Categories = nwInstances[nwModpack].Categories,
        //					Summary = nwInstances[nwModpack].Summary,
        //					Description = nwInstances[nwModpack].Description,
        //					GameVersion = nwInstances[nwModpack].GameVersion,
        //					WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + nwModpack,
        //					LogoUrl = nwInstances[nwModpack].LogoUrl,
        //					ExternalId = nwModpack
        //				});
        //			}
        //		}

        //		i++;
        //	}

        //	return result;
        //}

        public static Info GetInstance(string instanceId, NightWorldApi api)
        {
            Dictionary<string, NightWorldApi.InstanceInfo> nwInstances = api.GetInstancesList();
            var result = new List<Info>();

            NightWorldApi.InstanceInfo info = null;
            foreach (string id in nwInstances.Keys)
            {
                if (id == instanceId)
                {
                    info = nwInstances[id];
                }
            }

            if (info == null)
            {
                return null;
            }

            return new Info()
            {
                Name = info.Name,
                Author = info.Author,
                Categories = info.Categories,
                Summary = info.Summary,
                Description = info.Description,
                GameVersion = info.GameVersion,
                WebsiteUrl = LaunсherSettings.URL.Base + "modpacks/" + instanceId,
                LogoUrl = info.LogoUrl,
                ExternalId = instanceId
            };
        }
    }
}
