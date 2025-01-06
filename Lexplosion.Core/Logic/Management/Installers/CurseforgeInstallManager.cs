﻿using System;
using System.Collections.Generic;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Curseforge;

namespace Lexplosion.Logic.Management.Installers
{
    class CurseforgeInstallManager : ArchiveInstallManager<CurseforgeInstaller, InstanceManifest, CurseforgeFileInfo, InstancePlatformData>
    {
        public CurseforgeInstallManager(string instanceid, bool onlyBase, CancellationToken cancelToken) : base(new CurseforgeInstaller(instanceid), instanceid, onlyBase, cancelToken)
        { }

        protected override CurseforgeFileInfo GetProjectInfo(string projectId, string projectVersion)
        {
            var data = CurseforgeApi.GetProjectFile(projectId, projectVersion);

            if (data.id < 1)
            {
                return null;
            }

            return data;
        }

        protected override bool LocalInfoIsValid(InstancePlatformData data)
        {
            return data?.id != null && Int32.TryParse(data.id, out _);
        }

        protected override CurseforgeFileInfo GetProjectDefaultInfo(string projectId, string actualInstanceVersion)
        {
            int actualVersion = actualInstanceVersion?.ToInt32() ?? 0;
            List<CurseforgeFileInfo> instanceVersionsInfo = CurseforgeApi.GetProjectFiles(projectId); //получем информацию об этом модпаке

            CurseforgeFileInfo result = null;

            //проходимся по каждой версии модпака, ищем самый большой id. Это будет последняя версия. Причем этот id должен быть больше, чем id уже установленной версии
            foreach (CurseforgeFileInfo ver in instanceVersionsInfo)
            {
                if (ver.id > actualVersion)
                {
                    result = ver;
                    actualVersion = ver.id;
                }
            }

            return result;
        }

        protected override string GetProjectVersion(CurseforgeFileInfo projectData)
        {
            return projectData?.id.ToString();
        }

        protected override bool ManifestIsValid(InstanceManifest manifest)
        {
            return manifest?.minecraft != null && manifest.minecraft.modLoaders != null && manifest.minecraft.version != null;
        }

        protected override void DetermineGameType(InstanceManifest manifest, out ClientType clienType, out string modloaderVersion)
        {
            modloaderVersion = "";
            clienType = ClientType.Vanilla;
            foreach (var loader in manifest.minecraft.modLoaders)
            {
                if (loader.primary)
                {
                    modloaderVersion = loader.id;
                    break;
                }
            }

            if (modloaderVersion != "")
            {
                if (modloaderVersion.Contains("neoforge-"))
                {
                    clienType = ClientType.NeoForge;
                    modloaderVersion = modloaderVersion.Replace("neoforge-", "");
                }
                else if(modloaderVersion.Contains("forge-"))
                {
                    clienType = ClientType.Forge;
                    modloaderVersion = modloaderVersion.Replace("forge-", "");
                }
                else if (modloaderVersion.Contains("fabric-"))
                {
                    clienType = ClientType.Fabric;
                    modloaderVersion = modloaderVersion.Replace("fabric-", "");
                }
                else if (modloaderVersion.Contains("fabric-"))
                {
                    clienType = ClientType.Quilt;
                    modloaderVersion = modloaderVersion.Replace("quilt-", "");
                }
            }
        }

        protected override string DetermineGameVersion(InstanceManifest manifest)
        {
            return manifest.minecraft?.version ?? "";
        }

        public override string ProjectId { get => ProjectInfo?.id.ToString() ?? ""; }

        protected override bool ProfectInfoIsValid
        {
            get => !string.IsNullOrWhiteSpace(ProjectInfo?.downloadUrl) && !string.IsNullOrWhiteSpace(ProjectInfo.fileName);
        }

        protected override string ArchiveDownloadUrl
        {
            get => ProjectInfo.downloadUrl;
        }

        protected override string ArchiveFileName
        {
            get => ProjectInfo.fileName;
        }
    }
}
