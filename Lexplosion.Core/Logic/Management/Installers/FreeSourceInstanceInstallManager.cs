using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.FreeSource;

namespace Lexplosion.Logic.Management.Installers
{
    class FreeSourceInstanceInstallManager : ArchiveInstallManager<FreeSourceInstanceInstaller, InstanceManifest, ModpackVersion, FreeSourcePlatformData>
    {
        private SourceMap _urlsMap;

        public FreeSourceInstanceInstallManager(SourceMap urlsMap, string instanceid, bool onlyBase, CancellationToken cancelToken) : base(new FreeSourceInstanceInstaller(instanceid), instanceid, onlyBase, cancelToken)
        {
            _urlsMap = urlsMap;
        }

        public override string ProjectId => ProjectInfo?.ModpackId ?? string.Empty;

        protected override bool ProfectInfoIsValid => !string.IsNullOrWhiteSpace(ProjectInfo?.DownloadUrl);

        protected override string ArchiveDownloadUrl => ProjectInfo.DownloadUrl;

        protected override string ArchiveFileName
        {
            get
            {
                try
                {
                    var url = new Uri(ProjectInfo.DownloadUrl);
                    return Path.GetFileName(url.LocalPath);
                }
                catch
                {
                    return ProjectId + "-archive.zip";
                }
            }
        }

        protected override void DetermineGameType(InstanceManifest manifest, out ClientType clienType, out string modloaderVersion)
        {
            modloaderVersion = "";
            clienType = ClientType.Vanilla;

            modloaderVersion = manifest.ModloaderVersion;
            clienType = manifest.ModloaderType;
        }

        protected override string DetermineGameVersion(InstanceManifest manifest)
        {
            return manifest.GameVersion;
        }

        protected override ModpackVersion GetProjectDefaultInfo(string projectId, string actualInstanceVersion)
        {
            string url = _urlsMap?.GetModpackVersionsListUrl(projectId);
            if (url == null)
            {
                return null;
            }

            string result = ToServer.HttpPost(url);
            if (result == null)
            {
                return null;
            }

            try
            {
                var version = JsonConvert.DeserializeObject<MidpackVersionsList>(result);
                if (string.IsNullOrWhiteSpace(version?.LatestVersion) || version.AllVersions == null || !version.AllVersions.ContainsKey(version.LatestVersion))
                {
                    return null;
                }

                if (version.AllVersions[version.LatestVersion].Version != actualInstanceVersion)
                {
                    return version.AllVersions[version.LatestVersion];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        protected override ModpackVersion GetProjectInfo(string projectId, string projectVersion)
        {
            string url = _urlsMap?.GetModpackVersionManifestUrl(projectId, projectVersion);
            if (url == null)
            {
                return null;
            }

            string result = ToServer.HttpPost(url);
            if (result == null)
            {
                return null;
            }

            try
            {
                var version = JsonConvert.DeserializeObject<ModpackVersion>(result);
                if (string.IsNullOrWhiteSpace(version?.DownloadUrl) || string.IsNullOrWhiteSpace(version.Version) || string.IsNullOrWhiteSpace(version.ModpackId))
                {
                    return null;
                }

                return version;
            }
            catch
            {
                return null;
            }
        }

        protected override string GetProjectVersion(ModpackVersion projectData)
        {
            return projectData?.Version;
        }

        protected override bool LocalInfoIsValid(FreeSourcePlatformData data)
        {
            return data?.IsValid() == true;
        }

        protected override bool ManifestIsValid(InstanceManifest manifest)
        {
            if (manifest == null || string.IsNullOrEmpty(manifest.ModloaderVersion))
            {
                return false;
            }

            if (string.IsNullOrEmpty(manifest.GameVersion))
            {
                if (manifest.GameVersionInfo?.IsNan != false)
                {
                    return false;
                }

                manifest.GameVersion = manifest.GameVersionInfo.Id;
            }

            return Enum.IsDefined(typeof(ClientType), (int)manifest.ModloaderType);
        }
    }
}
