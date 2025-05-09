using System;
using System.Collections.Generic;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects.Nightworld;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.FileSystem.Installers;
using Lexplosion.Logic.FileSystem.Services;

namespace Lexplosion.Logic.Management.Installers
{
	class NightworldInstallManager : IInstallManager
	{
		private class NwInstancePlatformData : InstancePlatformData
		{
			public bool CustomVersion;
		}

		private NightWorldManifest _nightworldManifest = null;
		private VersionManifest _manifest = null;
		private LastUpdates _updates;
		private Dictionary<string, string> _instanceContent;
		private NightWorldInstaller _installer;
		private CancellationToken _cancelToken;

		private string _instanceId;
		private NwInstancePlatformData _infoData;

		private bool _requiresUpdates = true;
		private bool _onlyBase;
		private int _stagesCount = 0;
		private int _baseFaliseUpdatesCount = 0;
		private int _modpackFilesUpdatesCount = 0;

		private readonly MinecraftInfoService _infoService;
		private readonly NightWorldApi _nightWorldApi;
		private readonly DataFilesManager _dataFilesManager;

		private int _aactualVersion = -1;

		public event Action<string, int, DownloadFileProgress> FileDownloadEvent
		{
			add
			{
				_installer.FileDownloadEvent += value;
			}
			remove
			{
				_installer.FileDownloadEvent -= value;
			}
		}

		public event Action DownloadStarted;

		public NightworldInstallManager(string instanceid, bool onlyBase, INightWorldFileServicesContainer services, CancellationToken cancelToken)
		{
			_instanceId = instanceid;
			_onlyBase = onlyBase;
			_infoService = services.MinecraftService;
			_nightWorldApi = services.NwApi;
			_cancelToken = cancelToken;
			_installer = new NightWorldInstaller(instanceid, services);
			_dataFilesManager = services.DataFilesService;
		}

		private bool _downloadStartedIsCalled = false;
		private void DownloadStartedCall()
		{
			if (!_downloadStartedIsCalled)
				DownloadStarted?.Invoke();
		}

		public InstanceInit Check(out string javaVersionName, string instanceVersion)
		{
			javaVersionName = "";
			_infoData = _dataFilesManager.GetExtendedPlatfromData<NwInstancePlatformData>(_instanceId);

			if (_infoData?.id == null)
			{
				return InstanceInit.NightworldIdError;
			}

			int version = 0;
			if (!_onlyBase)
			{
				version = _nightWorldApi.GetInstanceVersion(_infoData.id);
				_requiresUpdates = version > _infoData.instanceVersion.ToInt32();
				_aactualVersion = version;
			}
			else
			{
				_requiresUpdates = false;
			}

			if (!_requiresUpdates)
			{
				VersionManifest manifest_ = _dataFilesManager.GetManifest(_instanceId, false);
				if (string.IsNullOrWhiteSpace(manifest_?.version?.GameVersion))
				{
					_nightworldManifest = _nightWorldApi.GetInstanceManifest(_infoData.id);
					if (_nightworldManifest == null)
					{
						// TODO: сделать как с локлаьными и курсфорджевкими сборками, чтобы при ошибке сеервера загружался локальный манифест
						return InstanceInit.ServerError;
					}

					if (_nightworldManifest.CustomVersion)
					{
						_manifest = _nightWorldApi.GetVersionManifest(_infoData.id);
						_infoData.CustomVersion = true;
						_dataFilesManager.SavePlatfromData(_instanceId, _infoData);
					}
					else
					{
						bool isNwClient = manifest_?.version?.IsNightWorldClient == true;
						var versionInfo = _nightworldManifest.version;
						_manifest = _infoService.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, isNwClient, versionInfo.modloaderVersion);
					}

					if (_manifest == null)
					{
						return InstanceInit.ServerError;
					}
				}
				else
				{
					var versionInfo = manifest_.version;
					if (_infoData.CustomVersion)
					{
						// TODO: здесь надо учитывать NightWorldClient. Он сейчас всегда выключаться будет
						_manifest = _nightWorldApi.GetVersionManifest(_infoData.id);
					}
					else
					{
						_manifest = _infoService.GetVersionManifest(versionInfo.GameVersion, versionInfo.ModloaderType, versionInfo.IsNightWorldClient, versionInfo.ModloaderVersion);
					}

					if (_manifest == null)
					{
						_nightworldManifest = _nightWorldApi.GetInstanceManifest(_infoData.id);
						if (_nightworldManifest == null)
						{
							return InstanceInit.ServerError;
						}

						if (_nightworldManifest.CustomVersion)
						{
							_infoData.CustomVersion = true;
							_dataFilesManager.SavePlatfromData(_instanceId, _infoData);
							_manifest = _nightWorldApi.GetVersionManifest(_infoData.id);
						}
						else
						{
							var mcVersion = _nightworldManifest.version;
							bool isNwClient = versionInfo.IsNightWorldClient == true;
							_manifest = _infoService.GetVersionManifest(mcVersion.gameVersion, mcVersion.modloaderType, isNwClient, mcVersion.modloaderVersion);
						}

						if (_manifest == null)
						{
							return InstanceInit.ServerError;
						}
					}
				}
			}
			else
			{
				_nightworldManifest = _nightWorldApi.GetInstanceManifest(_infoData.id);
				if (_nightworldManifest?.version == null)
				{
					return InstanceInit.ServerError;
				}

				VersionManifest manifest_ = _dataFilesManager.GetManifest(_instanceId, false);
				bool isNwClient = manifest_?.version?.IsNightWorldClient == true;
				if (_nightworldManifest.CustomVersion)
				{
					// TODO: аналогично подобному месту вышле. Учитывать NightWorldClient
					_infoData.CustomVersion = true;
					_dataFilesManager.SavePlatfromData(_instanceId, _infoData);
					_manifest = _nightWorldApi.GetVersionManifest(_infoData.id);
				}
				else
				{
					var versionInfo = _nightworldManifest.version;
					_manifest = _infoService.GetVersionManifest(versionInfo.gameVersion, versionInfo.modloaderType, isNwClient, versionInfo.modloaderVersion);
				}

				if (_manifest == null)
				{
					return InstanceInit.ServerError;
				}
			}

			if (_manifest != null)
			{
				_updates = _dataFilesManager.GetLastUpdates(_instanceId);
				_instanceContent = _installer.GetInstanceContent();

				_requiresUpdates = (_requiresUpdates || _updates.Count == 0);

				_baseFaliseUpdatesCount = _installer.CheckBaseFiles(_manifest, ref _updates); // проверяем основные файлы клиента на обновление
				if (_baseFaliseUpdatesCount == -1)
				{
					return InstanceInit.GuardError;
				}

				if (_baseFaliseUpdatesCount > 0)
				{
					DownloadStartedCall();
					_stagesCount++;
				}

				if (_requiresUpdates || _installer.InvalidStruct(_updates, _instanceContent))
				{
					if (_nightworldManifest == null)
					{
						_nightworldManifest = _nightWorldApi.GetInstanceManifest(_infoData.id);
						if (_nightworldManifest == null)
						{
							return InstanceInit.ServerError;
						}
					}

					_modpackFilesUpdatesCount = _installer.CheckInstance(_nightworldManifest, ref _updates, _instanceContent); // проверяем дополнительные файлы клиента (моды и прочее)
					if (_modpackFilesUpdatesCount == -1)
					{
						return InstanceInit.GuardError;
					}

					if (_modpackFilesUpdatesCount > 0)
					{
						DownloadStartedCall();
						_stagesCount++;
					}

					_requiresUpdates = true;
				}

				javaVersionName = _manifest.version.JavaVersionName;

				if (_aactualVersion == -1)
				{
					int version_ = _nightWorldApi.GetInstanceVersion(_infoData.id);
					if (version_ > 0)
						_aactualVersion = version_;
				}

				return InstanceInit.Successful;
			}
			else
			{
				return InstanceInit.ServerError;
			}
		}

		public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
		{
			Runtime.DebugWrite("NightWorld Update " + _requiresUpdates);

			Action<string, int, DownloadFileProgress> singleDownloadMethod = null;

			if (_stagesCount > 0)
			{
				if (_baseFaliseUpdatesCount > 1)
				{
					_installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = _stagesCount,
							Stage = 1,
							Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
							TotalFilesCount = totalDataCount,
							FilesCount = nowDataCount
						});
					};
				}
				else
				{
					singleDownloadMethod = delegate (string file, int pr, DownloadFileProgress stage_)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = _stagesCount,
							Stage = 1,
							Procents = pr,
							TotalFilesCount = 1,
							FilesCount = 0
						});
					};

					_installer.FileDownloadEvent += singleDownloadMethod;
				}
			}

			if (_baseFaliseUpdatesCount > 0)
			{
				progressHandler(StageType.Client, new ProgressHandlerArguments()
				{
					StagesCount = _stagesCount,
					Stage = 1,
					Procents = 0
				});
			}

			if (_cancelToken.IsCancellationRequested)
			{
				return new InitData
				{
					InitResult = InstanceInit.IsCancelled
				};
			}

			List<string> errors_ = _installer.UpdateBaseFiles(_manifest, ref _updates, javaPath, _cancelToken);
			List<string> errors = null;

			if (_cancelToken.IsCancellationRequested)
			{
				return new InitData
				{
					InitResult = InstanceInit.IsCancelled
				};
			}

			if (_requiresUpdates)
			{
				int stage;
				if (_baseFaliseUpdatesCount > 0)
				{
					stage = 2;
				}
				else
				{
					stage = 1;
				}

				progressHandler(StageType.Client, new ProgressHandlerArguments()
				{
					StagesCount = _stagesCount,
					Stage = stage,
					Procents = 0
				});

				if (_modpackFilesUpdatesCount > 1)
				{
					if (singleDownloadMethod != null)
					{
						_installer.FileDownloadEvent -= singleDownloadMethod;
					}

					_installer.FilesDownloadEvent += delegate (int totalDataCount, int nowDataCount)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = _stagesCount,
							Stage = stage,
							Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
							TotalFilesCount = totalDataCount,
							FilesCount = nowDataCount
						});
					};
				}
				else if (singleDownloadMethod == null)
				{
					_installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = _stagesCount,
							Stage = stage,
							Procents = pr,
							TotalFilesCount = 1,
							FilesCount = 0
						});
					};
				}

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				errors = _installer.UpdateInstance(_nightworldManifest, _infoData.id, ref _updates, _instanceContent, _cancelToken);

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}
			}

			_dataFilesManager.SaveManifest(_instanceId, _manifest);

			if (errors != null)
			{
				foreach (string error in errors_)
				{
					errors.Add(error);
				}
			}
			else
			{
				errors = errors_;
			}

			InstanceInit result;
			if (errors.Count > 0)
			{
				result = InstanceInit.DownloadFilesError;
			}
			else
			{
				result = InstanceInit.Successful;
				if (_aactualVersion != -1)
				{
					_infoData.instanceVersion = _aactualVersion.ToString();
				}

				_dataFilesManager.SavePlatfromData(_instanceId, _infoData);
			}

			return new InitData
			{
				InitResult = result,
				DownloadErrors = errors,
				VersionFile = _manifest.version,
				Libraries = _manifest.libraries,
				UpdatesAvailable = (result != InstanceInit.Successful)
			};
		}
	}
}
