using Lexplosion.Logic.FileSystem.Extensions;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Tools;
using NightWorld.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion.Logic.Management.Addons
{
    class ModrinthAddon : IPrototypeAddon
    {
        private ModrinthProjectFile _versionInfo;
        private ModrinthProjectInfo _addonInfo;
        private readonly IModrinthFileServicesContainer _services;
        private readonly Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> _categoriesGetter;
        private BaseInstanceData _instanceData;

        private string _projectId;
        private string _fileId;

        private ConcurrentHashSet<Modloader> _acceptableModloaders = null;

        public ModrinthAddon(BaseInstanceData instanceData, ModrinthProjectInfo addonInfo, IModrinthFileServicesContainer services, Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> categoriesGetter)
        {
            _addonInfo = addonInfo;
            _services = services;
            _categoriesGetter = categoriesGetter;
            _instanceData = instanceData;
            _projectId = addonInfo.ProjectId;

            SetAuthor();
        }

        public ModrinthAddon(BaseInstanceData instanceData, ModrinthProjectFile addonFileInfo, IModrinthFileServicesContainer services, Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> categoriesGetter)
        {
            _instanceData = instanceData;
            _projectId = addonFileInfo.ProjectId;
            _fileId = addonFileInfo.FileId;
            _services = services;
            _categoriesGetter = categoriesGetter;
        }

        public ModrinthAddon(BaseInstanceData instanceData, ModrinthProjectInfo addonInfo, ModrinthProjectFile addonFileInfo, IModrinthFileServicesContainer services, Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> categoriesGetter)
        {
            _instanceData = instanceData;
            _projectId = addonFileInfo.ProjectId;
            _addonInfo = addonInfo;
            _fileId = addonFileInfo.FileId;
            _services = services;
            _categoriesGetter = categoriesGetter;
            SetAuthor();
        }

        private ModrinthAddon(BaseInstanceData instanceData, string projectId, IModrinthFileServicesContainer services, Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> categoriesGetter)
        {
            _instanceData = instanceData;
            _projectId = projectId;
            _services = services;
            _categoriesGetter = categoriesGetter;
        }

        private ModrinthAddon(BaseInstanceData instanceData, string projectId, string fileId, IModrinthFileServicesContainer services, Func<AddonType, IEnumerable<string>, IEnumerable<CategoryBase>> categoriesGetter)
        {
            _instanceData = instanceData;
            _projectId = projectId;
            _fileId = fileId;
            _services = services;
            _categoriesGetter = categoriesGetter;
        }

        #region Info
        public string ProjectId
        {
            get
            {
                return _projectId;
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
                        if (dependencie?.ProjectId != null && dependencie.DependencyType == ModrinthProjectFile.Dependencie.Dependency.Required)
                        {
                            if (dependencie.VersionId != null)
                            {
                                list.Add(new AddonDependencie(dependencie.ProjectId, new ModrinthAddon(_instanceData, dependencie.ProjectId, dependencie.VersionId, _services, _categoriesGetter)));
                            }
                            else
                            {
                                list.Add(new AddonDependencie(dependencie.ProjectId, new ModrinthAddon(_instanceData, dependencie.ProjectId, _services, _categoriesGetter)));
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
            if (!string.IsNullOrWhiteSpace(_addonInfo.Author))
            {
                AuthorName = _addonInfo.Author;
            }
            else if (_addonInfo.Team != null)
            {
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    List<ModrinthTeam> teamsData = _services.MdApi.GetTeam(_addonInfo.Team);
                    if (teamsData.Count > 0)
                    {
                        AuthorName = teamsData[0]?.User?.Username;
                        OnInfoUpdated?.Invoke();
                    }
                });
            }
            else
            {
                AuthorName = "Unknown author";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DefaineLatesVersion_()
        {
            if (_addonInfo.Type == "mod")
            {
                Modloader mainMoaloder = (Modloader)_instanceData.Modloader;
                var files = _services.MdApi.GetProjectFiles(_projectId, mainMoaloder, _instanceData.GameVersion.Id);
                if (files.Count > 0 && files[0] != null)
                {
                    _versionInfo = files[0];
                    _fileId = _versionInfo.FileId;
                }
                else if (_acceptableModloaders != null)
                {
                    foreach (var modloader in _acceptableModloaders)
                    {
                        files = _services.MdApi.GetProjectFiles(_projectId, modloader, _instanceData.GameVersion.Id);
                        if (files.Count > 0 && files[0] != null)
                        {
                            _versionInfo = files[0];
                            _fileId = _versionInfo.FileId;
                            break;
                        }
                    }
                }
            }
            else
            {
                var files = _services.MdApi.GetProjectFiles(_projectId, modloaders: null, _instanceData.GameVersion.Id);

                if (files.Count > 0 && files[0] != null)
                {
                    _versionInfo = files[0];
                    _fileId = _versionInfo.FileId;
                }
            }
        }

        private object _addonInfoLocker = new object();
        private void DefineAddonInfo()
        {
            lock (_addonInfoLocker)
            {
                if (_addonInfo == null)
                {
                    _addonInfo = _services.MdApi.GetProject(_projectId);
                    SetAuthor();
                }
            }
        }

        public void DefineDefaultVersion()
        {
            DefineAddonInfo();

            if (_fileId != null)
            {
                if (_versionInfo == null)
                {
                    _versionInfo = _services.MdApi.GetProjectFile(_fileId);
                }
            }
            else
            {
                DefaineLatesVersion_();
            }
        }

        public void DefineLatestVersion()
        {
            DefineAddonInfo();
            DefaineLatesVersion_();
        }

        public void DefineSpecificVersion(object versionInfo)
        {
            DefineAddonInfo();
            var version = versionInfo as ModrinthProjectFile;
            if (version == null)
            {
                DefaineLatesVersion_();
                return;
            }

            _versionInfo = version;
        }

        public IDictionary<string, object> GetAllVersions()
        {
            var result = new Dictionary<string, object>();

            List<ModrinthProjectFile> files;
            if (_addonInfo.Type == "mod")
            {
                Modloader mainModloader = (Modloader)_instanceData.Modloader;

                var modloaders = new List<Modloader>();
                var acceptableModloaders = _acceptableModloaders;

                modloaders.Add(mainModloader);
                if (acceptableModloaders != null)
                {
                    modloaders.AddRange(acceptableModloaders);
                }

                files = _services.MdApi.GetProjectFiles(_projectId, modloaders, _instanceData.GameVersion.Id);
            }
            else
            {
                files = _services.MdApi.GetProjectFiles(_projectId, modloaders: null, _instanceData.GameVersion.Id);
            }

            foreach (var file in files)
            {
                result[file.Name] = file;
            }

            return result;
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

            return _services.MdApi.DownloadAddon(_addonInfo, _versionInfo.FileId, "instances/" + _instanceData.LocalId + "/", _services.DirectoryService, taskArgs);
        }


        public void CompareVersions(string addonFileId, Action actionIfTrue)
        {
            var addonInfo = _addonInfo;
            if (addonInfo == null) return;

            var lastEelem = addonInfo.Versions.GetLastElement();
            if (lastEelem != addonFileId)
            {
                if (lastEelem == null || addonInfo.GameVersions?.Count > 1 || addonInfo.Loaders?.Count > 1)
                {
                    //неизвестно для каокго модлоадера и для какой версии игры предназначена последняя версия аддона, поэтому делаем дополнительный запрос
                    ThreadPool.QueueUserWorkItem((object o) =>
                    {
                        Modloader? modloader = (addonInfo.Type == "mod") ? (Modloader?)_instanceData?.Modloader : null;
                        var files = _services.MdApi.GetProjectFiles(ProjectId, modloader, _instanceData?.GameVersion?.Id ?? "");

                        if (files.Count > 0 && files[0] != null && files[0].FileId != addonFileId)
                        {
                            actionIfTrue();
                        }
                    });
                }
                else
                {
                    // у аддона есть только 1 версия игры и 1 модлоадер (или их вовсе  нет), значит последняя версия там точно подходит
                    actionIfTrue();
                }
            }
        }

        public IEnumerable<CategoryBase> LoadCategories()
        {
            DefineAddonInfo();
            if (_addonInfo?.Categories == null) return new List<CategoryBase>();

            var resutl = _categoriesGetter(_addonInfo.GetAddonType, _addonInfo.Categories);
            return resutl ?? new List<CategoryBase>();
        }

        public string GetFullDescription()
        {
            return string.Empty;
        }

        public void SetAcceptableModloaders(IEnumerable<Modloader> modloaders)
        {
            if (modloaders == null)
            {
                _acceptableModloaders = null;
                return;
            }

            _acceptableModloaders = new ConcurrentHashSet<Modloader>(modloaders);
        }

        public string LoadWebsiteUrl()
        {
            return _addonInfo?.WebsiteUrl ?? "";
        }

        public event Action OnInfoUpdated;
    }
}
