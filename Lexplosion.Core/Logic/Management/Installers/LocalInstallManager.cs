using System;
using System.Collections.Generic;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.FileSystem.Installers;
using Lexplosion.Logic.FileSystem.Services;
using Lexplosion.Logic.Network.Services;
using Lexplosion.Logic.Objects.CommonClientData;

namespace Lexplosion.Logic.Management.Installers
{
	class LocalInstallManager : IInstallManager
	{
		private VersionManifest Manifest;
		private LastUpdates Updates;
		private InstanceInstaller installer;
		private CancellationToken _cancelToken;

		private string InstanceId;
		private int stagesCount = 0;
		private int updatesCount = 0;

		private readonly MinecraftInfoService _infoService;
		private readonly WithDirectory _withDirectory;
		private readonly DataFilesManager _dataFilesManager;

		public event Action<string, int, DownloadFileProgress> FileDownloadEvent
		{
			add
			{
				installer.FileDownloadEvent += value;
			}
			remove
			{
				installer.FileDownloadEvent -= value;
			}
		}

		public event Action DownloadStarted;

		public LocalInstallManager(string instanceid, IFileServicesContainer services, CancellationToken cancelToken)
		{
			InstanceId = instanceid;
			_infoService = services.MinecraftService;
			_cancelToken = cancelToken;
			installer = new InstanceInstaller(instanceid, services);
			_withDirectory = services.DirectoryService;
			_dataFilesManager = services.DataFilesService;
		}

		public InstanceInit Check(out string javaVersionName, string instanceVersion)
		{
			javaVersionName = string.Empty;

			//модпак локальный. получем его версию, отправляем её в ToServer.GetFilesList. Метод ToServer.GetFilesList получит список именно для этой версии, а не для модпака
			Manifest = _dataFilesManager.GetManifest(InstanceId, false);

			if (string.IsNullOrWhiteSpace(Manifest?.version?.GameVersion))
			{
				Runtime.DebugWrite("Manifest is null (" + (Manifest == null) + ", " + (Manifest?.version == null) + ")");
				return InstanceInit.VersionError;
			}

			var gameVersion = Manifest.version.GameVersion;
			var modloaderType = Manifest.version.ModloaderType;
			var modloaderVersion = Manifest.version.ModloaderVersion;
			var optifineVersion = Manifest.version.AdditionalInstaller?.installerVersion;
			var isNwClient = Manifest.version.IsNightWorldClient;

			Manifest = _infoService.GetVersionManifest(gameVersion, modloaderType, isNwClient, modloaderVersion, optifineVersion);

			if (Manifest != null)
			{
				Updates = _dataFilesManager.GetLastUpdates(InstanceId);
				updatesCount = installer.CheckBaseFiles(Manifest, ref Updates); // проверяем основные файлы клиента на обновление

				if (updatesCount == -1)
				{
					return InstanceInit.GuardError;
				}

				if (updatesCount > 0)
				{
					stagesCount = 1;
					DownloadStarted?.Invoke();
				}

				javaVersionName = Manifest.version.JavaVersionName;
				return InstanceInit.Successful;
			}
			else
			{
				Runtime.DebugWrite("Manifest from server is null. Load local manifest");
				Manifest = _dataFilesManager.GetManifest(InstanceId, true);

				if (string.IsNullOrWhiteSpace(Manifest?.version?.GameVersion))
				{
					Runtime.DebugWrite("Local manifest is null (" + (Manifest == null) + ", " + (Manifest?.version == null) + ")");
					return InstanceInit.VersionError;
				}

				javaVersionName = Manifest.version.JavaVersionName ?? string.Empty;
				return InstanceInit.Successful;
			}
		}

		public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
		{
			if (stagesCount == 1)
			{
				progressHandler(StateType.DownloadClient, new ProgressHandlerArguments()
				{
					StagesCount = 1,
					Stage = 1,
					Procents = 0
				});

				if (updatesCount > 1)
				{
					installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
					{
						progressHandler(StateType.DownloadClient, new ProgressHandlerArguments()
						{
							StagesCount = 1,
							Stage = 1,
							Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
							TotalFilesCount = totalDataCount,
							FilesCount = nowDataCount
						});
					};
				}
				else
				{
					installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
					{
						progressHandler(StateType.DownloadClient, new ProgressHandlerArguments()
						{
							StagesCount = 1,
							Stage = 1,
							Procents = pr,
							TotalFilesCount = 1,
							FilesCount = 0
						});
					};
				}
			}

			if (_cancelToken.IsCancellationRequested)
			{
				return new InitData
				{
					InitResult = InstanceInit.IsCancelled
				};
			}

			List<string> errors = null;

			if (Updates != null)
			{
				errors = installer.UpdateBaseFiles(Manifest, ref Updates, javaPath, _cancelToken);
				_dataFilesManager.SaveManifest(InstanceId, Manifest);
			}
			else
			{
				errors = new List<string>();
			}


			if (_cancelToken.IsCancellationRequested)
			{
				return new InitData
				{
					InitResult = InstanceInit.IsCancelled
				};
			}

			InstanceInit result = InstanceInit.Successful;
			if (errors.Count > 0)
			{
				result = InstanceInit.DownloadFilesError;
			}

			if (Manifest?.version?.MinecraftJar == null)
			{
				return new InitData
				{
					InitResult = InstanceInit.ManifestError
				};
			}

			return new InitData
			{
				InitResult = result,
				DownloadErrors = errors,
				VersionFile = Manifest.version,
				Libraries = Manifest.libraries,
				UpdatesAvailable = (result != InstanceInit.Successful)
			};
		}
	}
}
