using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using Lexplosion.Logic.Management.Instances;
using System.Runtime.CompilerServices;

namespace Lexplosion.Logic.Management.Addons
{
    class CurseforgeAddon : IPrototypeAddon
    {

        private CurseforgeAddonInfo _addonInfo;
        private BaseInstanceData _instanceData;
        private CurseforgeFileInfo _versionInfo;
        private string _projectId;

        public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeAddonInfo addonInfo)
        {
            _addonInfo = addonInfo;
            _instanceData = instanceData;
            _projectId = addonInfo.id;
        }

        private CurseforgeAddon(BaseInstanceData instanceData, string projectId)
        {
            _instanceData = instanceData;
            _projectId = projectId;
        }

        #region Info
        public string ProjectId
        {
            get { return _projectId; }
        }

        public string WebsiteUrl
        {
            get
            {
                if (_addonInfo?.links?.websiteUrl != null)
                {
                    return _addonInfo.links.websiteUrl;
                }

                var addonData = CurseforgeApi.GetAddonInfo(_projectId);
                return addonData?.links?.websiteUrl ?? "";
            }
        }

        public string AuthorName
        {
            get
            {
                return _addonInfo?.GetAuthorName ?? "";
            }
        }

        public string Description
        {
            get
            {
                return _addonInfo?.summary ?? "";
            }
        }

        public string Name
        {
            get
            {
                return _addonInfo?.name ?? "";
            }
        }

        public string LogoUrl
        {
            get
            {
                return _addonInfo?.logo?.url ?? "";
            }
        }

        public List<AddonDependencie> Dependecies
        {
            get
            {
                var list = new List<AddonDependencie>();

                if (_versionInfo?.dependencies != null)
                {
                    foreach (var dependencie in _versionInfo.dependencies)
                    {
                        if (dependencie.ContainsKey("relationType") && dependencie["relationType"] == "3" && dependencie.ContainsKey("modId") && dependencie["modId"] != null)
                        {
                            list.Add(new AddonDependencie(dependencie["modId"], new CurseforgeAddon(_instanceData, dependencie["modId"])));
                        }
                    }
                }

                return list;
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DefaineLatesVersion_()
        {
            if (_addonInfo == null)
            {
                _addonInfo = CurseforgeApi.GetAddonInfo(_projectId);
            }

            _versionInfo = GetLastFile(_instanceData.GameVersion, _addonInfo?.latestFiles, _addonInfo?.latestFilesIndexes, (AddonType)_addonInfo?.classId);
            if (_versionInfo == null)
            {
                _versionInfo = GetLastFile(_instanceData.GameVersion, CurseforgeApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion, _instanceData.Modloader), null, (AddonType)_addonInfo?.classId);
            }
        }

        public void DefineDefaultVersion() => DefaineLatesVersion_();
        public void DefineLatestVersion() => DefaineLatesVersion_();

        private CurseforgeFileInfo GetLastFile(string gameVersion, List<CurseforgeFileInfo> addonInfo, List<CurseforgeAddonInfo.GameVersionAddon> supportAddonInfo, AddonType? addonType)
        {
            CurseforgeFileInfo file = null;
            if (addonInfo != null)
            {
                int maxId = -1;
                bool versionIsFound = false;

                if (supportAddonInfo != null)
                {
                    foreach (var fileInfo in supportAddonInfo)
                    {
                        if (fileInfo.gameVersion == gameVersion && maxId < fileInfo.fileId)
                        {
                            maxId = fileInfo.fileId;
                            versionIsFound = true;
                        }
                    }
                }

                if (versionIsFound)
                {
                    foreach (var fileInfo in addonInfo)
                    {
                        if (maxId == fileInfo.id)
                        {
                            file = fileInfo;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var fileInfo in addonInfo)
                    {
                        if (fileInfo.gameVersions != null && maxId < fileInfo.id && fileInfo.gameVersions.Contains(gameVersion))
                        {
                            file = fileInfo;
                            maxId = fileInfo.id;
                        }
                    }
                }
            }

            return file;
        }

        public SetValues<InstalledAddonInfo, DownloadAddonRes> Install(TaskArgs taskArgs)
        {
            if (_versionInfo == null)
            {
                return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                {
                    Value1 = null,
                    Value2 = DownloadAddonRes.ProjectDataError
                };
            }

            return CurseforgeApi.DownloadAddon(_versionInfo, (AddonType)(_addonInfo.classId ?? 0), "instances/" + _instanceData.LocalId + "/", taskArgs);
        }

        public bool CompareVersions(string addonFileId)
        {
            var lastFile = GetLastFile(_instanceData.GameVersion, _addonInfo.latestFiles, _addonInfo.latestFilesIndexes, (AddonType)(_addonInfo.classId ?? 0));
            return lastFile != null && lastFile.id > addonFileId.ToInt32();
        }
    }
}
