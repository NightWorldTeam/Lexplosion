using System;
using System.Collections.Generic;
using System.Threading;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Tools;
using Lexplosion.Logic.Network.Services;

namespace Lexplosion.Logic.Management.Installers
{
	/// <summary>
	/// Описывает менеджер установки для инсталлеров, работающими со сборками в формате архивов.
	/// В таких сборках основные данные (модлоадер, версия игры и тп) хранятся в манифесете, который находится внутри архива. 
	/// И чтобы эти данные определить, нужно сначала скачать архив.
	/// </summary>
	/// <typeparam name="TInstaller">Тип инсталлера сборки</typeparam>
	/// <typeparam name="UManifest">Тип манифеста, который содержится в архиве</typeparam>
	/// <typeparam name="BProjectInfo">Тип, описывающий информацию о сборки, получаемую с сервера</typeparam>
	/// <typeparam name="CPlatformData">Тип, описывающий данные, которые будут храниться в файле instancePlatformData.json</typeparam>
	abstract class ArchiveInstallManager<TInstaller, UManifest, BProjectInfo, CPlatformData> : IInstallManager where TInstaller : InstanceInstaller, IArchivedInstanceInstaller<UManifest> where CPlatformData : InstancePlatformData
	{
		private VersionManifest _manifest;
		private LastUpdates _updates;
		private TInstaller _installer;

		private string _instanceId;
		private CPlatformData _infoData;
		protected BProjectInfo projectInfo;

		private bool _baseFilesIsCheckd = false;
		private bool _onlyBase;
		private readonly MinecraftInfoService _infoService;
		private CancellationToken _cancelToken;

		private int _updatesCount = 0;

		private bool _serverManifestIsNull = false;

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

		public ArchiveInstallManager(TInstaller installer, string instanceid, bool onlyBase, MinecraftInfoService infoService, CancellationToken cancelToken)
		{
			_instanceId = instanceid;
			_onlyBase = onlyBase;
			_infoService = infoService;
			_cancelToken = cancelToken;
			_installer = installer;
		}

		/// <summary>
		/// Получает информацию о проекте.
		/// </summary>
		/// <param name="projectId">ID проекта</param>
		/// <param name="projectVersion">Версия проекта</param>
		/// <returns>
		/// Объект, содержащий нужную инфу о пректе. Если ничего не найдено, то null.
		/// </returns>
		protected abstract BProjectInfo GetProjectInfo(string projectId, string projectVersion);

		/// <summary>
		/// Аналогично GetProjectInfo, за исключением того, что он должнн возвращать версию по умочланию (обычно последнюю)
		/// </summary>
		/// /// <returns>
		/// Объект, содержащий нужную инфу о пректе. Если ничего не найдено, то null.
		/// </returns>
		protected abstract BProjectInfo GetProjectDefaultInfo(string projectId, string actualInstanceVersion);

		/// <summary>
		/// Должен возвращать версию проекта из его объъекта информации (объект, что возвращают GetProjectInfo и GetProjectDefaultInfo)
		/// </summary>
		protected abstract string GetProjectVersion(BProjectInfo projectData);

		protected abstract bool ManifestIsValid(UManifest manifest);

		/// <summary>
		/// Должен определять тип майкрафта
		/// </summary>
		/// <param name="clienType">Тип клиента</param>
		/// <param name="modloaderVersion">Версия модлоадера (если его нет, то должна возвращаться пустая строка)</param>
		protected abstract void DetermineGameType(UManifest manifest, out ClientType clienType, out string modloaderVersion);

		protected abstract string DetermineGameVersion(UManifest manifest);

		protected abstract bool LocalInfoIsValid(CPlatformData data);

		public abstract string ProjectId { get; }
		protected abstract bool ProfectInfoIsValid { get; }
		protected abstract string ArchiveDownloadUrl { get; }
		protected abstract string ArchiveFileName { get; }

		private bool _downloadStartedIsCalled = false;
		private void DownloadStartedCall()
		{
			if (!_downloadStartedIsCalled)
				DownloadStarted?.Invoke();
		}

		public InstanceInit Check(out string javaVersionName, string instanceVersion)
		{
			javaVersionName = string.Empty;

			_manifest = DataFilesManager.GetManifest(_instanceId, false);
			_infoData = DataFilesManager.GetExtendedPlatfromData<CPlatformData>(_instanceId);

			if (!LocalInfoIsValid(_infoData))
			{
				return InstanceInit.CurseforgeIdError;
			}

			// TODO: думаю, если манифест равен null, вполне можно продолжить работу скачав всё заново 
			if (string.IsNullOrWhiteSpace(_manifest?.version?.GameVersion))
			{
				return InstanceInit.VersionError;
			}

			if (!string.IsNullOrWhiteSpace(_manifest.version.ModloaderVersion) && _manifest.version.ModloaderType != ClientType.Vanilla)
			{
				_baseFilesIsCheckd = true;

				var ver = _manifest.version;
				_manifest = _infoService.GetVersionManifest(ver.GameVersion, ver.ModloaderType, ver.IsNightWorldClient, ver.ModloaderVersion);

				if (_manifest != null)
				{
					_updates = DataFilesManager.GetLastUpdates(_instanceId);
					_updatesCount = _installer.CheckBaseFiles(_manifest, ref _updates); // проверяем основные файлы клиента на обновление

					if (_updatesCount == -1)
					{
						return InstanceInit.GuardError;
					}
					else if (_updatesCount > 0)
					{
						DownloadStartedCall();
					}
				}
				else
				{
					Runtime.DebugWrite("Manifest from server is null. Load local manifest");
					_serverManifestIsNull = true;
					_manifest = DataFilesManager.GetManifest(_instanceId, true);

					if (string.IsNullOrWhiteSpace(_manifest?.version?.GameVersion))
					{
						Runtime.DebugWrite("Local manifest is null (" + (_manifest == null) + ", " + (_manifest?.version == null) + ")");
						return InstanceInit.VersionError;
					}

					javaVersionName = _manifest.version.JavaVersionName ?? string.Empty;
					return InstanceInit.Successful;
				}
			}

			// если версия сборки определена, то получаем инфу о ней
			BProjectInfo info = default;
			if (instanceVersion != null)
			{
				info = GetProjectInfo(_infoData.id, instanceVersion);
			}
			else if (!_onlyBase) //версия сборки не определена. Получаем версию по умолчанию
			{
				info = GetProjectDefaultInfo(_infoData.id, _infoData.instanceVersion);
			}

			if (info != null)
			{
				projectInfo = info;
				_infoData.instanceVersion = GetProjectVersion(info);
			}

			DataFilesManager.SavePlatfromData(_instanceId, _infoData);

			javaVersionName = _manifest.version.JavaVersionName ?? string.Empty;
			return InstanceInit.Successful;
		}

		public InitData Update(string javaPath, ProgressHandlerCallback progressHandler)
		{
			if (_serverManifestIsNull)
			{
				return new InitData
				{
					InitResult = InstanceInit.Successful,
					VersionFile = _manifest.version,
					Libraries = _manifest.libraries,
					UpdatesAvailable = false,
					ClientVersion = ProjectId ?? string.Empty
				};
			}

			var localFiles = _installer.GetInstanceContent(); //получем список всех файлов модпака

			//нашелся id, который больше id установленной версии. Значит доступно обновление. Или же отсуствуют некоторые файлы модпака. Обновляем
			if (projectInfo != null || _installer.InvalidStruct(localFiles))
			{
				Runtime.DebugWrite("ProjectInfo != null " + (projectInfo != null));
				DownloadStartedCall();

				if (projectInfo == null)
				{
					projectInfo = GetProjectInfo(_infoData.id, _infoData.instanceVersion); //получем информацию об этом модпаке

					if (!ProfectInfoIsValid)
					{
						// возможно эта версия была удалена на сервере. Пробуем получить версию по умолчанию
						projectInfo = GetProjectDefaultInfo(_infoData.id, _infoData.instanceVersion);
						if (ProfectInfoIsValid)
						{
							_infoData.instanceVersion = GetProjectVersion(projectInfo);
							DataFilesManager.SavePlatfromData(_instanceId, _infoData);
						}
						else
						{
							return new InitData
							{
								InitResult = InstanceInit.CurseforgeIdError,
							};
						}
					}
				}

				_installer.MainFileDownload += delegate (int percent)
				{
					progressHandler(StageType.Client, new ProgressHandlerArguments()
					{
						StagesCount = 3,
						Stage = 1,
						Procents = percent,
						TotalFilesCount = 1,
						FilesCount = 0
					});
				};

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				InstanceFileGetter fileGetter = (string tempDir, Func<string, TaskArgs> taskArgsGetter) =>
				{
					bool res = WithDirectory.DownloadFile(ArchiveDownloadUrl, ArchiveFileName, tempDir, taskArgsGetter(ArchiveFileName));
					return (res, tempDir + ArchiveFileName, ArchiveFileName);
				};

				// скачиваем архив модпака и из него получаем манифест
				var manifest = _installer.Extraction(fileGetter, _cancelToken);

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				if (!ManifestIsValid(manifest))
				{
					Runtime.DebugWrite("Manifest is invalid");
					return new InitData
					{
						InitResult = InstanceInit.ManifestError,
						UpdatesAvailable = true,
						ClientVersion = ProjectId
					};
				}

				// производим обработку разорхивированных файлов
				if (!_installer.HandleExtractedFiles(ref localFiles, _cancelToken))
				{
					Runtime.DebugWrite("Move files error");
					return new InitData
					{
						InitResult = InstanceInit.MoveFilesError,
						UpdatesAvailable = true,
						ClientVersion = ProjectId
					};
				}

				progressHandler(StageType.Client, new ProgressHandlerArguments()
				{
					StagesCount = 3,
					Stage = 2,
					Procents = 0
				});

				DetermineGameType(manifest, out ClientType gameType, out string modLoaderVersion);
				Runtime.DebugWrite("modLoaderVersion " + modLoaderVersion);

				var versionData = _manifest?.version;
				bool baseFFilesIsUpdates = modLoaderVersion != versionData?.ModloaderVersion || gameType != versionData?.ModloaderType || DetermineGameVersion(manifest) != versionData?.GameVersion;

				// Скачиваем основные файлы майкнрафта

				// если BaseFilesIsCheckd равно true, то это значит что в манифесте уже была версия форджа
				if (!_baseFilesIsCheckd || baseFFilesIsUpdates) // в данном случае в манифесте версии форджа не была и нам надо её получить. Или же были измнения в модлоадере или версии игры
				{
					bool isNwClient = versionData?.IsNightWorldClient == true;
					_manifest = _infoService.GetVersionManifest(DetermineGameVersion(manifest), gameType, isNwClient, modLoaderVersion);

					if (_manifest != null)
					{
						DataFilesManager.SaveManifest(_instanceId, _manifest);

						if (_cancelToken.IsCancellationRequested)
						{
							return new InitData
							{
								InitResult = InstanceInit.IsCancelled
							};
						}

						_updates = DataFilesManager.GetLastUpdates(_instanceId);
						_updatesCount = _installer.CheckBaseFiles(_manifest, ref _updates); // проверяем основные файлы клиента на обновление

						if (_updatesCount == -1)
						{
							return new InitData
							{
								InitResult = InstanceInit.GuardError,
								UpdatesAvailable = true,
								ClientVersion = ProjectId
							};
						}
					}
					else
					{
						return new InitData
						{
							InitResult = InstanceInit.ServerError,
							UpdatesAvailable = true,
							ClientVersion = ProjectId
						};
					}
				}

				Action<string, int, DownloadFileProgress> singleDownloadMethod = null;

				if (_updatesCount > 1)
				{
					_installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = 3,
							Stage = 2,
							Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
							TotalFilesCount = totalDataCount,
							FilesCount = nowDataCount
						});
					};
				}
				else if (_updatesCount == 1)
				{
					singleDownloadMethod = delegate (string file, int pr, DownloadFileProgress stage_)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = 3,
							Stage = 2,
							Procents = pr,
							TotalFilesCount = 1,
							FilesCount = 0
						});
					};

					_installer.FileDownloadEvent += singleDownloadMethod;
				}

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				List<string> errors = _installer.UpdateBaseFiles(_manifest, ref _updates, javaPath, _cancelToken);

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				if (errors.Count > 0)
				{
					return new InitData
					{
						InitResult = InstanceInit.DownloadFilesError,
						DownloadErrors = errors,
						UpdatesAvailable = true,
						ClientVersion = ProjectId
					};
				}

				progressHandler(StageType.Client, new ProgressHandlerArguments()
				{
					StagesCount = 3,
					Stage = 2,
					Procents = 100
				});

				if (singleDownloadMethod != null)
				{
					_installer.FileDownloadEvent -= singleDownloadMethod;
				}

				_installer.AddonsDownload += delegate (int totalDataCount, int nowDataCount)
				{
					if (nowDataCount != 0)
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = 3,
							Stage = 3,
							Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
							TotalFilesCount = totalDataCount,
							FilesCount = nowDataCount
						});
					}
					else
					{
						progressHandler(StageType.Client, new ProgressHandlerArguments()
						{
							StagesCount = 3,
							Stage = 3,
							Procents = 0,
							TotalFilesCount = totalDataCount,
							FilesCount = nowDataCount
						});
					}
				};

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				// скачиваем аддоны
				errors = _installer.Install(manifest, localFiles, _cancelToken);

				if (_cancelToken.IsCancellationRequested)
				{
					return new InitData
					{
						InitResult = InstanceInit.IsCancelled
					};
				}

				if (errors.Count > 0)
				{
					return new InitData
					{
						InitResult = InstanceInit.DownloadFilesError,
						DownloadErrors = errors,
						UpdatesAvailable = true,
						ClientVersion = ProjectId
					};
				}
			}
			else
			{
				if (_baseFilesIsCheckd)
				{
					if (_updatesCount > 1)
					{
						_installer.BaseDownloadEvent += delegate (int totalDataCount, int nowDataCount)
						{
							progressHandler(StageType.Client, new ProgressHandlerArguments()
							{
								StagesCount = 1,
								Stage = 1,
								Procents = (int)(((decimal)nowDataCount / (decimal)totalDataCount) * 100),
								TotalFilesCount = totalDataCount,
								FilesCount = nowDataCount
							});
						};
					}
					else if (_updatesCount == 1)
					{
						_installer.FileDownloadEvent += delegate (string file, int pr, DownloadFileProgress stage_)
						{
							progressHandler(StageType.Client, new ProgressHandlerArguments()
							{
								StagesCount = 1,
								Stage = 1,
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

					List<string> errors = _installer.UpdateBaseFiles(_manifest, ref _updates, javaPath, _cancelToken);

					if (_cancelToken.IsCancellationRequested)
					{
						return new InitData
						{
							InitResult = InstanceInit.IsCancelled
						};
					}

					if (errors.Count > 0)
					{
						return new InitData
						{
							InitResult = InstanceInit.DownloadFilesError,
							DownloadErrors = errors,
							UpdatesAvailable = true,
							ClientVersion = ProjectId ?? ""
						};
					}
				}
				else
				{
					return new InitData
					{
						InitResult = InstanceInit.ForgeVersionError,
						UpdatesAvailable = true,
						ClientVersion = ProjectId ?? ""
					};
				}
			}

			DataFilesManager.SaveManifest(_instanceId, _manifest);

			return new InitData
			{
				InitResult = InstanceInit.Successful,
				VersionFile = _manifest.version,
				Libraries = _manifest.libraries,
				UpdatesAvailable = false,
				ClientVersion = ProjectId ?? String.Empty
			};
		}
	}
}
