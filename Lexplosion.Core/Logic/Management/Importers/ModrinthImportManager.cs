using System;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.Modrinth;

namespace Lexplosion.Logic.Management.Importers
{
    class ModrinthImportManager : ArchiveImportManager<InstanceManifest>
    {
        public ModrinthImportManager(string fileAddres, Settings globalSettings, CancellationToken cancelToken) : base(fileAddres, new ModrinthInstaller(null), globalSettings, cancelToken)
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
            return manifest.name ?? new Random().GenerateString(15);
        }

        protected override string DetermineSummary(InstanceManifest manifest)
        {
            return manifest.summary;
        }

        protected override string DetermineAthor(InstanceManifest manifest) => null;

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

    }
}
