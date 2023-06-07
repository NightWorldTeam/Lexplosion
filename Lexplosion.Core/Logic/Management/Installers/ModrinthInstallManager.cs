using System.Threading;
using Lexplosion.Tools;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.Modrinth;

namespace Lexplosion.Logic.Management.Installers
{
    class ModrinthInstallManager : ArchiveInstallManager<ModrinthInstaller, InstanceManifest, ModrinthProjectFile>
    {
        public ModrinthInstallManager(string instanceid, bool onlyBase, CancellationToken cancelToken) : base(new ModrinthInstaller(instanceid), instanceid, onlyBase, cancelToken)
        { }
        protected override ModrinthProjectFile GetProjectInfo(string projectId, string projectVersion)
        {
            return ModrinthApi.GetProjectFile(projectVersion);
        }

        protected override bool LocalInfoIsValid(InstancePlatformData data)
        {
            return data?.id != null;
        }

        protected override ModrinthProjectFile GetProjectDefaultInfo(string projectId, string actualInstanceVersion)
        {
            ModrinthProjectInfo instanceInfo = ModrinthApi.GetProject(projectId); //получем информацию об этом модпаке

            //проверяем полученные данные на валидность и определяем последнюю версию клиента (она будет последняя в спике)
            string lastVersion = instanceInfo.Versions.GetLastElement();
            if (lastVersion != null && lastVersion != actualInstanceVersion)
            {
                return ModrinthApi.GetProjectFile(lastVersion);
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

            if (manifest.dependencies.ContainsKey("forge"))
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

        public override string ProjectId { get => ProjectInfo?.ProjectId ?? ""; }

        protected override bool ProfectInfoIsValid
        {
            get => ProjectInfo?.Files != null && ProjectInfo.Files.Count > 0 && ProjectInfo.Files[0].Filename != null && ProjectInfo.Files[0].Url != null;
        }

        protected override string ArchiveDownloadUrl
        {
            get => ProjectInfo.Files[0].Url;
        }

        protected override string ArchiveFileName
        {
            get => ProjectInfo.Files[0].Filename;
        }
    }
}
