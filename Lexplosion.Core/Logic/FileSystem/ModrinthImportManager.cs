using System.Threading;
using Lexplosion.Logic.Management.Installers;
using Lexplosion.Logic.Objects.Modrinth;

namespace Lexplosion.Logic.FileSystem
{
    class ModrinthImportManager : ArchiveImportManager<InstanceManifest>
    {
        public ModrinthImportManager(string fileAddres, bool isLocalPath, Settings globalSettings, CancellationToken cancelToken) : base(fileAddres, isLocalPath, new ModrinthInstaller(GenerateTempId()), globalSettings, cancelToken)
        {
        }

        protected override bool ManifestIsValid(InstanceManifest manifest)
        {
            return manifest?.files != null && manifest.dependencies != null && manifest.dependencies.ContainsKey("minecraft");
        }

        protected override string DetermineGameVersion(InstanceManifest manifest)
        {
            return manifest.dependencies["minecraft"] ?? "";
        }

        protected override string DetermineInstanceName(InstanceManifest manifest)
        {
            return manifest.name;
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

    }
}
