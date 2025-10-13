using Lexplosion.Logic.FileSystem.Installers;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Modrinth;
using System.Collections.Generic;
using System.Threading;

namespace Lexplosion.Logic.Management.Installers
{
    class ModrinthInstallManager : ArchiveInstallManager<ModrinthInstaller, InstanceManifest, ModrinthProjectFile, InstancePlatformData>
    {
        private ModrinthApi _modrinthApi;

        public ModrinthInstallManager(string instanceid, bool onlyBase, IModrinthFileServicesContainer services, CancellationToken cancelToken) : base(new ModrinthInstaller(instanceid, services), instanceid, onlyBase, services, cancelToken)
        {
            _modrinthApi = services.MdApi;
        }

        protected override ModrinthProjectFile GetProjectInfo(string projectId, string projectVersion)
        {
            var data = _modrinthApi.GetProjectFile(projectVersion);

            if (string.IsNullOrWhiteSpace(data.FileId))
            {
                return null;
            }

            return data;
        }

        protected override bool LocalInfoIsValid(InstancePlatformData data)
        {
            return data?.id != null;
        }

        protected override ModrinthProjectFile GetProjectDefaultInfo(string projectId, string actualInstanceVersion)
        {
            ModrinthProjectInfo instanceInfo = _modrinthApi.GetProject(projectId); //получем информацию об этом модпаке

            //проверяем полученные данные на валидность и определяем последнюю версию клиента (она будет последняя в спике)
            string lastVersion = instanceInfo.Versions.GetLastElement();
            if (lastVersion != null && lastVersion != actualInstanceVersion)
            {
                return _modrinthApi.GetProjectFile(lastVersion);
            }

            return null;
        }

        protected override string GetProjectVersion(ModrinthProjectFile projectData)
        {
            return projectData?.FileId;
        }

        protected override bool ManifestIsValid(InstanceManifest manifest)
        {
            return manifest?.files != null && manifest.dependencies != null && manifest.dependencies.ContainsKey("minecraft");
        }

        protected override void DetermineGameType(InstanceManifest manifest, out ClientType clienType, out string modloaderVersion)
        {
            modloaderVersion = "";
            clienType = ClientType.Vanilla;

            if (manifest.dependencies.ContainsKey("neoforge"))
            {
                modloaderVersion = manifest.dependencies["neoforge"] ?? "";
                clienType = ClientType.NeoForge;
            }
            else if (manifest.dependencies.ContainsKey("neoforge-loader"))
            {
                modloaderVersion = manifest.dependencies["neoforge-loader"] ?? "";
                clienType = ClientType.NeoForge;
            }
            else if (manifest.dependencies.ContainsKey("forge"))
            {
                modloaderVersion = manifest.dependencies["forge"] ?? "";
                clienType = ClientType.Forge;
            }
            else if (manifest.dependencies.ContainsKey("forge-loader"))
            {
                modloaderVersion = manifest.dependencies["forge-loader"] ?? "";
                clienType = ClientType.Forge;
            }
            else if (manifest.dependencies.ContainsKey("quilt-loader"))
            {
                modloaderVersion = manifest.dependencies["quilt-loader"] ?? "";
                clienType = ClientType.Quilt;
            }
            else if (manifest.dependencies.ContainsKey("fabric-loader"))
            {
                modloaderVersion = manifest.dependencies["fabric-loader"] ?? "";
                clienType = ClientType.Fabric;
            }
        }

        protected override string DetermineGameVersion(InstanceManifest manifest)
        {
            return manifest.dependencies["minecraft"] ?? "";
        }

        public override string ProjectId { get => projectInfo?.ProjectId ?? string.Empty; }

        protected override bool ProfectInfoIsValid
        {
            get => projectInfo?.Files != null && projectInfo.Files.Count > 0 && projectInfo.Files[0].Filename != null && projectInfo.Files[0].Url != null;
        }

        protected override string ArchiveDownloadUrl
        {
            get => projectInfo.Files[0].Url;
        }

        protected override string ArchiveFileName
        {
            get => projectInfo.Files[0].Filename;
        }
    }
}
