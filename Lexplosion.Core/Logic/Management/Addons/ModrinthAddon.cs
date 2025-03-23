using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Tools;
using System.Collections.Generic;
using Lexplosion.Logic.Management.Instances;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using static Lexplosion.Logic.Objects.Curseforge.InstanceManifest;
using NightWorld.Collections.Concurrent;

namespace Lexplosion.Logic.Management.Addons
{
	class ModrinthAddon : IPrototypeAddon
	{
		private ModrinthProjectFile _versionInfo;
		private ModrinthProjectInfo _addonInfo;
		private BaseInstanceData _instanceData;

		private string _projectId;
		private string _fileId;

		private ConcurrentHashSet<Modloader> _acceptableModloaders = new();

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
			if (!string.IsNullOrWhiteSpace(_addonInfo.Author))
			{
				AuthorName = _addonInfo.Author;
			}
			else if (_addonInfo.Team != null)
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
				var files = ModrinthApi.GetProjectFiles(_projectId, (Modloader)_instanceData.Modloader, _instanceData.GameVersion.Id);
				if (files.Count > 0 && files[0] != null)
				{
					_versionInfo = files[0];
					_fileId = _versionInfo.FileId;
				}
				else
				{
					foreach (var modloader in _acceptableModloaders)
					{
						files = ModrinthApi.GetProjectFiles(_projectId, modloader, _instanceData.GameVersion.Id);
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
				var files = ModrinthApi.GetProjectFiles(_projectId, modloaders: null, _instanceData.GameVersion.Id);

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
					_addonInfo = ModrinthApi.GetProject(_projectId);
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
				var modloaders = new Modloader[_acceptableModloaders.Count + 1];
				modloaders[0] = (Modloader)_instanceData.Modloader;
				_acceptableModloaders.CopyTo(modloaders, 1);
				files = ModrinthApi.GetProjectFiles(_projectId, (Modloader)_instanceData.Modloader, _instanceData.GameVersion.Id);
			}
			else
			{
				files = ModrinthApi.GetProjectFiles(_projectId, modloaders: null, _instanceData.GameVersion.Id);
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

			return ModrinthApi.DownloadAddon(_addonInfo, _versionInfo.FileId, "instances/" + _instanceData.LocalId + "/", taskArgs);
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
						var files = ModrinthApi.GetProjectFiles(ProjectId, modloader, _instanceData?.GameVersion?.Id ?? "");

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

			var resutl = CategoriesManager.FindAddonsCategoriesById(ProjectSource.Modrinth, _addonInfo.GetAddonType, _addonInfo.Categories);
			return resutl ?? new List<CategoryBase>();
		}

		public string GetFullDescription()
		{
			return string.Empty;
		}

		public void SetAcceptableModloader(Modloader modloader)
		{
			_acceptableModloaders.Add(modloader);
		}

		public void RemoveAcceptableModloader(Modloader modloader)
		{
			_acceptableModloaders.Remove(modloader);
		}

		public event Action OnInfoUpdated;
    }
}
