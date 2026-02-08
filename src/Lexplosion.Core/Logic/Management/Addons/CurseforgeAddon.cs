using System;
using System.Collections.Generic;
using NightWorld.Collections.Concurrent;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Management.Instances;
using System.Runtime.CompilerServices;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.FileSystem.Extensions;

namespace Lexplosion.Logic.Management.Addons
{
	class CurseforgeAddon : IPrototypeAddon
	{

		private CurseforgeAddonInfo _addonInfo;
		private readonly ICurseforgeFileServicesContainer _services;
		private BaseInstanceData _instanceData;
		private CurseforgeFileInfo _versionInfo;
		private string _projectId;

		private ConcurrentHashSet<Modloader> _acceptableModloaders = null;

		public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeAddonInfo addonInfo, ICurseforgeFileServicesContainer services)
		{
			_addonInfo = addonInfo;
			_services = services;
			_instanceData = instanceData;
			_projectId = addonInfo.id;
		}

		public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeFileInfo fileInfo, ICurseforgeFileServicesContainer services)
		{
			_instanceData = instanceData;
			_projectId = fileInfo.modId;
			_versionInfo = fileInfo;
			_services = services;
		}

		public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeAddonInfo addonInfo, CurseforgeFileInfo fileInfo, ICurseforgeFileServicesContainer services)
		{
			_addonInfo = addonInfo;
			_instanceData = instanceData;
			_projectId = addonInfo.id;
			_versionInfo = fileInfo;
			_services = services;
		}

		private CurseforgeAddon(BaseInstanceData instanceData, string projectId, ICurseforgeFileServicesContainer services)
		{
			_instanceData = instanceData;
			_projectId = projectId;
			_services = services;
		}

		#region Info
		public string ProjectId
		{
			get { return _projectId; }
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

		public string FileId
		{
			get => _versionInfo?.id.ToString() ?? "";
		}

		public ProjectSource Source
		{
			get => ProjectSource.Curseforge;
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
							list.Add(new AddonDependencie(dependencie["modId"], new CurseforgeAddon(_instanceData, dependencie["modId"], _services)));
						}
					}
				}

				return list;
			}
		}
		#endregion

		private object _addonInfoLocker = new object();
		private void DefineAddonInfo()
		{
			lock (_addonInfoLocker)
			{
				if (_addonInfo == null)
				{
					_addonInfo = _services.CfApi.GetAddonInfo(_projectId);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DefaineLatesVersion_()
		{
			DefineAddonInfo();

			Modloader? modloader = (_addonInfo.classId == 6) ? (Modloader)_instanceData.Modloader : null; // если это мод (_addonInfo.classId == 6), то передаем модлоадер. Иначе ставим null чтобы модлоадер не учитывался

			var files = _services.CfApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, modloader);
			_versionInfo = GetLastFile(_instanceData.GameVersion.Id, files, null, (AddonType)_addonInfo?.classId);

			var acceptableModloaders = _acceptableModloaders;
			if (_versionInfo == null && modloader != null && acceptableModloaders != null)
			{
				foreach (Modloader newModloader in acceptableModloaders)
				{
					files = _services.CfApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, newModloader);
					_versionInfo = GetLastFile(_instanceData.GameVersion.Id, files, null, (AddonType)_addonInfo?.classId);
					if (_versionInfo != null) break;
				}
			}
		}

		public void DefineDefaultVersion()
		{
			if (_versionInfo != null)
			{
				DefineAddonInfo();
			}
			else
			{
				DefaineLatesVersion_();
			}
		}

		public void DefineLatestVersion() => DefaineLatesVersion_();

		public void DefineSpecificVersion(object versionInfo)
		{
			var version = versionInfo as CurseforgeFileInfo;
			if (version == null)
			{
				DefaineLatesVersion_();
				return;
			}

			DefineAddonInfo();
			_versionInfo = version;
		}

		public IDictionary<string, object> GetAllVersions()
		{
			AddonType type = (AddonType)(_addonInfo.classId ?? 0);

			List<CurseforgeFileInfo> files;
			if (type == AddonType.Mods)
			{
				var modloadersList = new List<Modloader>();
				var acceptableModloaders = _acceptableModloaders;

				modloadersList.Add((Modloader)_instanceData.Modloader);
				if (acceptableModloaders != null)
				{
					modloadersList.AddRange(acceptableModloaders);
				}

				files = _services.CfApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, modloadersList);
			}
			else
			{
				files = _services.CfApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, modloader: null);
			}


			var result = new Dictionary<string, object>();
			foreach (var file in files)
			{
				result[file.displayName] = file;
			}

			return result;
		}

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
			if (_addonInfo == null || _versionInfo == null)
			{
				return new SetValues<InstalledAddonInfo, DownloadAddonRes>
				{
					Value1 = null,
					Value2 = DownloadAddonRes.ProjectDataError
				};
			}

			return _services.CfApi.DownloadAddon(_versionInfo, (AddonType)(_addonInfo.classId ?? 0), "instances/" + _instanceData.LocalId + "/", _services.DirectoryService, taskArgs);
		}

		public void CompareVersions(string addonFileId, Action actionIfTrue)
		{
			if (_addonInfo == null) return;

			int currenId = addonFileId.ToInt32();
			if (_addonInfo?.latestFilesIndexes == null) return;

			foreach (var file in _addonInfo.latestFilesIndexes)
			{
				if (file == null) continue;

				//md будет true, если тип аддона НЕ мод, если клиент без модлоадера или же тип модлоадера клиента совпадает с типом модлоадера мода.
				bool md = (_addonInfo.classId != 6 || _instanceData.Modloader == ClientType.Vanilla || file.ModloaderType == _instanceData.Modloader);
				if (file.gameVersion == _instanceData.GameVersion.Id && md && file.fileId > currenId)
				{
					actionIfTrue();
					return;
				}
			}
		}

		public IEnumerable<CategoryBase> LoadCategories()
		{
			DefineAddonInfo();

			return _addonInfo?.categories ?? new List<CurseforgeCategory>();

		}

		public string GetFullDescription()
		{
			return _services.CfApi.GetProjectDescription(_addonInfo?.id);
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
			if (_addonInfo?.links?.websiteUrl != null)
			{
				return _addonInfo.links.websiteUrl;
			}

			var addonData = _services.CfApi.GetAddonInfo(_projectId);
			return addonData?.links?.websiteUrl ?? "";
		}

		public event Action OnInfoUpdated;
	}
}
