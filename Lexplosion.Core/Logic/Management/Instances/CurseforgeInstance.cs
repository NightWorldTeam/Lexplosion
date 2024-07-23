﻿using System;
using System.Collections.Generic;
using System.Net;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Instances
{
    class CurseforgeInstance : PrototypeInstance
    {
        public override bool CheckUpdates(string localId)
        {
            var infoData = DataFilesManager.GetFile<InstancePlatformData>(WithDirectory.InstancesPath + localId + "/instancePlatformData.json");
            if (string.IsNullOrWhiteSpace(infoData?.id))
            {
                return false;
            }

            var content = DataFilesManager.GetFile<InstanceContentFile>(WithDirectory.InstancesPath + localId + "/instanceContent.json");
            if (content != null && !content.FullClient)
            {
                return true;
            }

            if (!Int32.TryParse(infoData.id, out _))
            {
                return true;
            }

            List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetProjectFiles(infoData.id); //получем информацию об этом модпаке

            //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии 
            foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
            {
                if (ver.id > infoData.instanceVersion.ToInt32())
                {
                    return true;
                }
            }

            return false;
        }

        public override InstanceData GetFullInfo(string localId, string externalId)
        {
            var data = CurseforgeApi.GetInstance(externalId);

            if (data == null)
            {
                return null;
            }

            var images = new List<byte[]>();
            if (data.screenshots != null && data.screenshots.Count > 0)
            {
                var perfomer = new TasksPerfomer(3, data.screenshots.Count);
                foreach (var item in data.screenshots)
                {
                    perfomer.ExecuteTask(delegate ()
                    {
                        using (var webClient = new WebClient())
                        {
                            try
                            {
                                webClient.Proxy = null;
                                images.Add(webClient.DownloadData(item.url));
                            }
                            catch { }
                        }
                    });
                }

                perfomer.WaitEnd();
            }

            int? projectFileId = null;
            if (data.latestFilesIndexes?.Count > 0)
            {
                projectFileId = data.latestFilesIndexes[0]?.fileId;
            }

            string date;
            try
            {
                date = (data.dateModified != null) ? DateTime.Parse(data.dateModified).ToString("dd MMM yyyy") : "";
            }
            catch
            {
                date = "";
            }

            return new InstanceData
            {
                Source = InstanceSource.Curseforge,
                Categories = data.categories,
                Description = data.summary,
                Summary = data.summary,
                TotalDownloads = (long)data.downloadCount,
                GameVersion = (data.latestFilesIndexes != null && data.latestFilesIndexes.Count > 0) ? data.latestFilesIndexes[0].gameVersion : "",
                LastUpdate = date,
                Modloader = data.ModloaderType,
                Images = images,
                WebsiteUrl = data.links?.websiteUrl,
                Changelog = (projectFileId != null) ? (CurseforgeApi.GetProjectChangelog(externalId, projectFileId?.ToString()) ?? "") : ""
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

                    Runtime.DebugWrite(file.fileName + " " + file.id);
                }

                return versions;
            }
            else
            {
                return null;
            }
        }
    }
}
