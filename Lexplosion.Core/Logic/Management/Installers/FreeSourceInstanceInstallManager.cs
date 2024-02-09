using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Objects.FreeSource;

namespace Lexplosion.Logic.Management.Installers
{
    class FreeSourceInstanceInstallManager : ArchiveInstallManager<FreeSourceInstanceInstaller, InstanceManifest, ModpackVersion>
    {
        private SourceMap _urlsMap;

        public FreeSourceInstanceInstallManager(SourceMap urlsMap, string instanceid, bool onlyBase, CancellationToken cancelToken) : base(new FreeSourceInstanceInstaller(instanceid, urlsMap?.SourceId), instanceid, onlyBase, cancelToken)
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
            string url = _urlsMap?.ModpackVersionsListUrl?.Replace("${modpackId}", LocalIdData.Load(projectId).Id);
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

                return version.AllVersions[version.LatestVersion];
            }
            catch
            {
                return null;
            }
        }

        protected override ModpackVersion GetProjectInfo(string projectId, string projectVersion)
        {
            string url = _urlsMap?.ModpackVersionManifestUrl;
            if (url == null)
            {
                return null;
            }

            url = url.Replace("${modpackId}", LocalIdData.Load(projectId).Id).Replace("${modpackVersion}", projectVersion);

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

        protected override bool LocalInfoIsValid(InstancePlatformData data)
        {
            var idData = LocalIdData.Load(data?.id);
            return !string.IsNullOrWhiteSpace(idData?.Id) && !string.IsNullOrWhiteSpace(idData?.SourceUrl);
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
