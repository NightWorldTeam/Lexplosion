using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Tools;
using System.Collections.Generic;
using System.Linq;
using Lexplosion.Logic.Management.Instances;
using System.Runtime.CompilerServices;
using System;
using System.Threading;

namespace Lexplosion.Logic.Management.Addons
{
    class ModrinthAddon : IPrototypeAddon
    {
        private ModrinthProjectFile _versionInfo;
        private ModrinthProjectInfo _addonInfo;
        private BaseInstanceData _instanceData;

        private string _projectId;
        private string _fileId;

        public ModrinthAddon(BaseInstanceData instanceData, ModrinthProjectInfo addonInfo)
        {
            _addonInfo = addonInfo;
            _instanceData = instanceData;
            _projectId = addonInfo.ProjectId;

            SetAuthor();
        }

        public ModrinthAddon(BaseInstanceData instanceData, ModrinthProjectFile addonFileInfo)
        {
            _instanceData = instanceData;
            _projectId = addonFileInfo.ProjectId;
            _fileId = addonFileInfo.FileId;
        }

        public ModrinthAddon(BaseInstanceData instanceData, ModrinthProjectInfo addonInfo, ModrinthProjectFile addonFileInfo)
        {
            _instanceData = instanceData;
            _projectId = addonFileInfo.ProjectId;
            _addonInfo = addonInfo;
            _fileId = addonFileInfo.FileId;

            SetAuthor();
        }

        private ModrinthAddon(BaseInstanceData instanceData, string projectId)
        {
            _instanceData = instanceData;
            _projectId = projectId;
        }

        private ModrinthAddon(BaseInstanceData instanceData, string projectId, string fileId)
        {
            _instanceData = instanceData;
            _projectId = projectId;
            _fileId = fileId;
        }

        #region Info
        public string ProjectId
        {
            get
            {
                return _projectId;
            }
        }

        public string WebsiteUrl
        {
            get
            {
                return _addonInfo?.WebsiteUrl ?? "";
            }
        }

        public string AuthorName
        {
            get; private set;
        }

        public string Description
        {
            get
            {
                return _addonInfo?.Summary ?? "";
            }
        }

        public string Name
        {
            get
            {
                return _addonInfo?.Title ?? "";
            }
        }

        public string LogoUrl
        {
            get
            {
                return _addonInfo?.LogoUrl ?? "";
            }
        }

        public string FileId
        {
            get => _fileId;
        }

        public ProjectSource Source
        {
            get => ProjectSource.Modrinth;
        }
        #endregion

        public List<AddonDependencie> Dependecies
        {
            get
            {
                var list = new List<AddonDependencie>();

                if (_versionInfo?.Dependencies != null)
                {
                    foreach (var dependencie in _versionInfo.Dependencies)
                    {
                        if (dependencie?.ProjectId != null)
                        {
                            if (dependencie.VersionId != null)
                            {
                                list.Add(new AddonDependencie(dependencie.ProjectId, new ModrinthAddon(_instanceData, dependencie.ProjectId, dependencie.VersionId)));
                            }
                            else
                            {
                                list.Add(new AddonDependencie(dependencie.ProjectId, new ModrinthAddon(_instanceData, dependencie.ProjectId)));
                            }
                        }
                    }
                }

                return list;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAuthor()
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                List<ModrinthTeam> teamsData = ModrinthApi.GetTeam(_addonInfo.Team);
                if (teamsData.Count > 0)
                {
                    AuthorName = teamsData[0]?.User?.Username;
                    OnInfoUpdated?.Invoke();
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DefaineLatesVersion_()
        {
            var files = ModrinthApi.GetProjectFiles(_projectId, _instanceData.Modloader, _instanceData.GameVersion);
            if (files.Count > 0)
            {
                _versionInfo = files[files.Count - 1];
                _fileId = _versionInfo.FileId;
            }
        }

        public void DefineDefaultVersion()
        {
            if (_addonInfo == null)
            {
                _addonInfo = ModrinthApi.GetProject(_projectId);
                SetAuthor();
            }

            if (_fileId != null)
            {
                if (_versionInfo == null)
                {
                    _versionInfo = ModrinthApi.GetProjectFile(_fileId);
                }
            }
            else
            {
                DefaineLatesVersion_();
            }
        }

        public void DefineLatestVersion()
        {
            if (_addonInfo == null)
            {
                _addonInfo = ModrinthApi.GetProject(_projectId);
                SetAuthor();
            }

            DefaineLatesVersion_();
        }

        public SetValues<InstalledAddonInfo, DownloadAddonRes> Install(TaskArgs taskArgs)
        {
            if (_addonInfo == null || _versionInfo == null)
            {
                return new SetValues<InstalledAddonInfo, DownloadAddonRes>
                {
                    Value1 = null,
                    Value2 = DownloadAddonRes.ProjectDataError
                };
            }

            return ModrinthApi.DownloadAddon(_addonInfo, _versionInfo.FileId, "instances/" + _instanceData.LocalId + "/", taskArgs);
        }

        public bool CompareVersions(string addonFileId)
        {
            return _addonInfo.Versions[_addonInfo.Versions.Count - 1] != addonFileId;
        }

        public event Action OnInfoUpdated;
    }
}
