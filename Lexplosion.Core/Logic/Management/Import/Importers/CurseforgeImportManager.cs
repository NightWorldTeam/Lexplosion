using System;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.Curseforge;

namespace Lexplosion.Logic.Management.Import.Importers
{
    internal class CurseforgeImportManager : ArchiveImportManager<InstanceManifest>
    {
        public CurseforgeImportManager(string fileAddres, Settings globalSettings, CancellationToken cancelToken) : base(fileAddres, new CurseforgeInstaller(null), globalSettings, cancelToken)
        {
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
                else if (modloaderVersion.Contains("forge-"))
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

        protected override string DetermineInstanceName(InstanceManifest manifest)
        {
            return manifest.name ?? new Random().GenerateString(15);
        }

        protected override string DetermineSummary(InstanceManifest manifest) => null;

        protected override bool ManifestIsValid(InstanceManifest manifest)
        {
            return manifest?.minecraft != null && manifest.minecraft.modLoaders != null && manifest.minecraft.version != null;
        }

        protected override string DetermineAthor(InstanceManifest manifest)
        {
            return manifest?.author;
        }
    }
}
