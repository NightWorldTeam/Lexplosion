﻿using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using Lexplosion.Logic.Management.Instances;
using System.Runtime.CompilerServices;
using System;

namespace Lexplosion.Logic.Management.Addons
{
	class CurseforgeAddon : IPrototypeAddon
	{

		private CurseforgeAddonInfo _addonInfo;
		private BaseInstanceData _instanceData;
		private CurseforgeFileInfo _versionInfo;
		private string _projectId;

		private HashSet<Modloader> _acceptableModloaders = new();

		public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeAddonInfo addonInfo)
		{
			_addonInfo = addonInfo;
			_instanceData = instanceData;
			_projectId = addonInfo.id;
		}

		public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeFileInfo fileInfo)
		{
			_instanceData = instanceData;
			_projectId = fileInfo.modId;
			_versionInfo = fileInfo;
		}

		public CurseforgeAddon(BaseInstanceData instanceData, CurseforgeAddonInfo addonInfo, CurseforgeFileInfo fileInfo)
		{
			_addonInfo = addonInfo;
			_instanceData = instanceData;
			_projectId = addonInfo.id;
			_versionInfo = fileInfo;
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
							list.Add(new AddonDependencie(dependencie["modId"], new CurseforgeAddon(_instanceData, dependencie["modId"])));
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
					_addonInfo = CurseforgeApi.GetAddonInfo(_projectId);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DefaineLatesVersion_()
		{
			DefineAddonInfo();

			//_versionInfo = GetLastFile(_instanceData.GameVersion, _addonInfo?.latestFiles, _addonInfo?.latestFilesIndexes, (AddonType)_addonInfo?.classId);
			//if (_versionInfo == null)
			//{
			//    _versionInfo = GetLastFile(_instanceData.GameVersion, CurseforgeApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion, _instanceData.Modloader), null, (AddonType)_addonInfo?.classId);
			//}

			var modloader = (_addonInfo.classId == 6) ? _instanceData.Modloader : ClientType.Vanilla; // если это мод (_addonInfo.classId == 6), то передаем модлоадер. Иначе ставим Vanilla

			var files = CurseforgeApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, (int)modloader);
			_versionInfo = GetLastFile(_instanceData.GameVersion.Id, files, null, (AddonType)_addonInfo?.classId);

			if (_versionInfo == null && modloader != ClientType.Vanilla)
			{
				foreach (var newModloader in _acceptableModloaders)
				{
					files = CurseforgeApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, (int)newModloader);
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
			var modloader = (_addonInfo.classId == 6) ? _instanceData.Modloader : ClientType.Vanilla;
			var files = CurseforgeApi.GetProjectFiles(_addonInfo.id, _instanceData.GameVersion.Id, (int)modloader);

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

			return CurseforgeApi.DownloadAddon(_versionInfo, (AddonType)(_addonInfo.classId ?? 0), "instances/" + _instanceData.LocalId + "/", taskArgs);
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
			return CurseforgeApi.GetProjectDescription(_addonInfo?.id);
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
