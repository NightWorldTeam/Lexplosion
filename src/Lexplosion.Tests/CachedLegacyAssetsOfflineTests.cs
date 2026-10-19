using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Installers;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Localization;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Objects.CommonClientData;
using Newtonsoft.Json;

namespace Lexplosion.Tests
{
    // Offline regression tests for cached legacy assets.
    [TestFixture]
    public sealed class CachedLegacyAssetsOfflineTests
    {
        private const string InstanceId = "offline-fixture";
        private const string AssetName = "sounds/random/click.ogg";
        private string _root;
        private WithDirectory _directories;
        private DataFilesManager _files;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "lexplosion-offline-" + Guid.NewGuid().ToString("N"));
            // No HTTP service is available in this fixture. Any dependency on it is a bug
            // in the fully cached scenario, not a reason to access real Minecraft servers.
            _directories = new WithDirectory(null);
            _directories.Create(_root);
            _files = new DataFilesManager(_directories);
            string version = Path.Combine(_directories.GetInstancePath(InstanceId), "version");
            Directory.CreateDirectory(version);
            File.WriteAllText(Path.Combine(version, "client.jar"), "locally cached jar");
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null && Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        [Test]
        [TestCase("legacy", false, false, true, false)]
        [TestCase("pre-1.6", false, false, false, true)]
        [TestCase("custom", true, true, true, true)]
        public void FullyCachedAssetsAreRestoredWithNoNetworkService(
            string indexName, bool isVirtual, bool mapToResources,
            bool expectVirtual, bool expectResources)
        {
            string hash = StoreObject("locally cached sound");
            WriteIndex(indexName, isVirtual, mapToResources, hash);
            var setup = CreateInstaller(indexName);

            // With version JAR, asset index and objects already present, no downloads are needed.
            int needed = setup.Installer.CheckBaseFiles(in setup.Manifest, ref setup.Updates);
            Assert.That(needed, Is.Zero, "A complete local cache must not trigger downloads.");

            var errors = setup.Installer.UpdateBaseFiles(
                in setup.Manifest, ref setup.Updates, "", CancellationToken.None);
            Assert.That(errors, Is.Empty, string.Join("; ", errors));

            string virtualPath = Path.Combine(_root, "assets", "virtual", "legacy", "sounds", "random", "click.ogg");
            string resourcesPath = Path.Combine(_directories.GetInstancePath(InstanceId),
                "resources", "sounds", "random", "click.ogg");
            Assert.That(File.Exists(virtualPath), Is.EqualTo(expectVirtual), "virtual/legacy mapping mismatch");
            Assert.That(File.Exists(resourcesPath), Is.EqualTo(expectResources), "resources mapping mismatch");
            if (expectVirtual) Assert.That(File.ReadAllText(virtualPath), Is.EqualTo("locally cached sound"));
            if (expectResources) Assert.That(File.ReadAllText(resourcesPath), Is.EqualTo("locally cached sound"));
        }

        [Test]
        public void MissingLocalIndexCannotBeReconstructedFromHashedObjectsOffline()
        {
            StoreObject("locally cached sound");
            // Deliberately omit the index JSON and provide no HTTP service.
            var setup = CreateInstaller("legacy");
            int needed = setup.Installer.CheckBaseFiles(in setup.Manifest, ref setup.Updates);

            // Documents the current limitation: a missing index forces a network attempt
            // and the hash-only objects do not provide original resource names.
            Assert.That(needed, Is.GreaterThan(0), "Missing index must be reported as incomplete cache.");
            Assert.That(File.Exists(Path.Combine(_root, "assets", "indexes", "legacy.json")), Is.False);
        }

        private string StoreObject(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            string hash;
            using (var sha = SHA1.Create())
                hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            string dir = Path.Combine(_root, "assets", "objects", hash.Substring(0, 2));
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, hash), bytes);
            return hash;
        }

        private void WriteIndex(string indexName, bool isVirtual, bool mapToResources, string hash)
        {
            var index = new InstanceInstaller.Assets
            {
                Virtual = isVirtual,
                MapToResources = mapToResources,
                objects = new Dictionary<string, InstanceInstaller.Assets.AssetFile>
                {
                    [AssetName] = new InstanceInstaller.Assets.AssetFile { hash = hash }
                }
            };
            string dir = Path.Combine(_root, "assets", "indexes");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, indexName + ".json"), JsonConvert.SerializeObject(index));
        }

        private (InstanceInstaller Installer, VersionManifest Manifest, LastUpdates Updates)
            CreateInstaller(string indexName)
        {
            var manifest = new VersionManifest
            {
                version = new VersionInfo
                {
                    GameVersion = "1.5.2",
                    AssetsVersion = indexName,
                    MinecraftJar = new Lexplosion.Logic.Objects.CommonClientData.FileInfo
                    {
                        name = "client.jar", lastUpdate = 42L
                    },
                    LibrariesLastUpdate = 0
                },
                libraries = new Dictionary<string, LibInfo>()
            };
            var updates = new LastUpdates { ["version"] = 42L };
            var services = new OfflineServices(_directories, _files);
            return (new InstanceInstaller(InstanceId, services), manifest, updates);
        }

        private sealed class OfflineServices : IFileServicesContainer
        {
            public OfflineServices(WithDirectory dir, DataFilesManager files)
            {
                DirectoryService = dir;
                DataFilesService = files;
            }
            public WithDirectory DirectoryService { get; }
            public DataFilesManager DataFilesService { get; }
            public ToServer WebService => null;
            public MinecraftInfoService MinecraftService => null;
            public ILocalizationService LocalizationService => null;
        }
    }
}
