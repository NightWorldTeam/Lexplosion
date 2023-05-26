using Lexplosion.Controls;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;

namespace Lexplosion.Common.Models.InstanceForm
{
    public sealed class DownloadModel : VMBase
    {
        private readonly InstanceFormModel _instanceFormModel;

        public readonly List<Action<StageType, ProgressHandlerArguments>> DownloadActions = new List<Action<StageType, ProgressHandlerArguments>>();
        public readonly List<Action<InstanceInit, List<string>, bool>> ComplitedDownloadActions = new List<Action<InstanceInit, List<string>, bool>>();


        private readonly DoNotificationCallback _doNotification = (header, message, time, type) => { };
        private readonly Action _complitedError;


        #region Properties


        private bool _isPrepareOnly = true;
        public bool IsPrepareOnly
        {
            get => _isPrepareOnly; set
            {
                _isPrepareOnly = value;
                OnPropertyChanged();
            }
        }

        private bool _isPrepare = false;
        public bool IsPrepare
        {
            get => _isPrepare; set
            {
                _isPrepare = value;
                OnPropertyChanged();
            }
        }

        private bool _isFilesDownload;
        public bool IsFilesDownload
        {
            get => _isFilesDownload; set
            {
                _isFilesDownload = value;
                OnPropertyChanged();
            }
        }

        private bool _isDownloadInProgress;
        public bool IsDownloadInProgress
        {
            get => _isDownloadInProgress;
            set
            {
                _isDownloadInProgress = value;
                OnPropertyChanged();
            }
        }

        private int _downloadProgress;
        public int DownloadProgress
        {
            get => _downloadProgress; set
            {
                _downloadProgress = value;
                OnPropertyChanged();
            }
        }

        private int _stage;
        public int Stage
        {
            get => _stage; set
            {
                _stage = value;
                OnPropertyChanged();
            }
        }

        private int _stagesCount;
        public int StagesCount
        {
            get => _stagesCount; set
            {
                _stagesCount = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndeterminate;
        public bool HasProcents
        {
            get => _isIndeterminate; set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        private StageType _downloadStageType;
        public StageType DownloadStageType
        {
            get => _downloadStageType; set
            {
                _downloadStageType = value;
                OnPropertyChanged();
            }
        }

        private int _totalDownloadingFilesCount;
        /// <summary>
        /// Всего файлов для скачивания.
        /// </summary>
        public int TotalDownloadingFilesCount
        {
            get => _totalDownloadingFilesCount; set
            {
                _totalDownloadingFilesCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DownloadFilesNof));
            }
        }

        private int _downloadingFilesCount;
        /// <summary>
        /// Всего файлов скачено.
        /// </summary>
        public int DownloadingFilesCount
        {
            get => _downloadingFilesCount; set
            {
                _downloadingFilesCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DownloadFilesNof));

            }
        }

        public string DownloadFilesNof
        {
            get => String.Format(ResourceGetter.GetString("downloadFilesCountNof"), DownloadingFilesCount, TotalDownloadingFilesCount);
        }


        #endregion Properties


        #region Constructors


        public DownloadModel(InstanceFormModel instanceFormModel)
        {
            _instanceFormModel = instanceFormModel;

            DownloadActions.Add(Download);
            ComplitedDownloadActions.Add(InstanceDownloadCompleted);

            instanceFormModel.InstanceClient.ProgressHandler += DownloadProcess;
            instanceFormModel.InstanceClient.DownloadComplited += ComplitedDownloadAction;
        }


        public DownloadModel(InstanceFormModel instanceFormModel, Action complitedError, DoNotificationCallback doNotification = null)
        {
            _complitedError = complitedError;
            _doNotification = doNotification ?? _doNotification;
            _instanceFormModel = instanceFormModel;

            DownloadActions.Add(Download);
            ComplitedDownloadActions.Add(InstanceDownloadCompleted);

            instanceFormModel.InstanceClient.ProgressHandler += DownloadProcess;
            instanceFormModel.InstanceClient.DownloadComplited += ComplitedDownloadAction;
        }

        #endregion Construtors


        #region Public & Protected Methods


        /// <summary>
        /// Запускает скачивание
        /// </summary>
        public void DownloadPrepare(string version = null)
        {
            _instanceFormModel.InstanceClient.AddToLibrary();
            _instanceFormModel.UpperButton.ChangeFuncProgressBar();
            IsDownloadInProgress = true;

            _instanceFormModel.UpdateLowerButton();

            Lexplosion.Runtime.TaskRun(delegate
            {
                _instanceFormModel.InstanceClient.Update(version);
            });
        }

        /// <summary>
        /// Обрабатывает информацию о скачивании
        /// </summary>
        /// <param name="downloadStageType">Тип стадии></param>
        /// <param name="stagesCount">Количество стадиый</param>
        /// <param name="stage">Номер текущей стадии</param>
        /// <param name="procent">Процетны</param>
        public void Download(StageType downloadStageType, ProgressHandlerArguments progressHandlerArguments)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StagesCount = progressHandlerArguments.StagesCount;
                Stage = progressHandlerArguments.Stage;
                DownloadProgress = progressHandlerArguments.Procents;
                DownloadStageType = downloadStageType;
                TotalDownloadingFilesCount = progressHandlerArguments.TotalFilesCount;
                DownloadingFilesCount = progressHandlerArguments.FilesCount;


                if (downloadStageType != StageType.Prepare)
                {
                    IsPrepareOnly = false;
                    IsPrepare = false;
                }

                if (downloadStageType == StageType.Java)
                {
                    _instanceFormModel.OverviewField = ResourceGetter.GetString("javaInstalling");
                    HasProcents = false;
                    IsFilesDownload = true;
                }
                else if (downloadStageType == StageType.Prepare)
                {
                    _instanceFormModel.OverviewField = ResourceGetter.GetString("checkLocalFiles");
                    IsPrepare = true;
                    HasProcents = true;
                    IsFilesDownload = false;
                }
                else
                {
                    _instanceFormModel.OverviewField = ResourceGetter.GetString("instanceLoading") + " " + Stage + '/' + StagesCount;
                    HasProcents = false;
                    IsFilesDownload = true;
                }
            });
        }

        public void InstanceDownloadCompleted(InstanceInit result, IEnumerable<string> downloadErrors, bool IsGameRun)
        {
            var isError = true;
            App.Current.Dispatcher.Invoke(() =>
            {
                // TODO: localization
                IsFilesDownload = false;
                switch (result)
                {
                    case InstanceInit.Successful:
                        {
                            isError = false;
                            if (MainModel.Instance.RunningInstance != null && MainModel.Instance.IsInstanceRunning && MainModel.Instance.RunningInstance.Model == _instanceFormModel)
                            {
                                IsPrepareOnly = true;
                                _instanceFormModel.UpdateLowerButton();
                            }
                            else if (!IsPrepareOnly)
                            {
                                IsDownloadInProgress = false;
                                _doNotification(
                                    ResourceGetter.GetString("downloadSuccessfullyCompleted"),
                                    ResourceGetter.GetString("instanceTitle") + ": " + _instanceFormModel.InstanceClient.Name +
                                    "\n" + ResourceGetter.GetString("version") + ": " + _instanceFormModel.InstanceClient.GameVersion, 5, 0);
                                _instanceFormModel.UpperButton.ChangeFuncPlay();
                                _instanceFormModel.UpdateLowerButton();
                            }
                        }
                        break;
                    case InstanceInit.DownloadFilesError:
                        {
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            string files = ResourceGetter.GetString("failedToDownloadFiles") + ":\n";
                            foreach (var de in downloadErrors ?? new List<string>())
                            {
                                files += de + "\n";
                            }
                            files += "\n" + ResourceGetter.GetString("tryDownloadAgain")  + "\n";
                            _doNotification(ResourceGetter.GetString("failedToDownloadSomeFiles"), files, 0, 1);
                            IsDownloadInProgress = false;
                            IsPrepare = false;
                            _complitedError.Invoke();
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.CurseforgeIdError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Curseforge Id Error", ResourceGetter.GetString("externalIdIncorrect"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.NightworldIdError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Nightworld Id Error", ResourceGetter.GetString("externalIdIncorrect"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.ServerError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification(ResourceGetter.GetString("serverError"), ResourceGetter.GetString("failedGetDataFromServer"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.GuardError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Guard Error", ResourceGetter.GetString("fileVerificationFailed"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.VersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Version Error", ResourceGetter.GetString("versionVerificationFailed"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.ForgeVersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Forge Version Error", ResourceGetter.GetString("modloaderVerificationFailed"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.GamePathError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Game Path Error", ResourceGetter.GetString("invalidGameDirectory"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.ManifestError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Manifest Error", ResourceGetter.GetString("failedLoadInstanceManifest"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.JavaDownloadError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Java Download Error", ResourceGetter.GetString("trySetCustomJavaPath"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                    case InstanceInit.IsCancelled:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification(ResourceGetter.GetString("instanceDownloadWasCanceledSuccessfully"), ResourceGetter.GetString("instanceTitle") + ": " + _instanceFormModel.InstanceClient.Name, 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                            break;
                        }
                    default:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Unknown Error", ResourceGetter.GetString("unknownErrorTryRestartLauncher"), 0, 1);
                            MainModel.Instance.IsInstanceRunning = false;
                        }
                        break;
                }

                    if (isError || !IsPrepareOnly)
                    {
                        IsDownloadInProgress = false;
                        IsPrepare = false;
                        IsPrepareOnly = false;
                        _instanceFormModel.OverviewField = _instanceFormModel.InstanceClient.Summary;
                        _instanceFormModel.UpdateLowerButton();
                    }
                    else
                    {
                        //IsDownloadInProgress = false;
                        IsPrepare = false;
                        _instanceFormModel.UpdateLowerButton();
                        _instanceFormModel.OverviewField = ResourceGetter.GetString("gameRunning");
                    }
            });
        }

        public void CancelDownload()
        {
            _instanceFormModel.OverviewField = ResourceGetter.GetString("downloadCancelling");

            HasProcents = false;
            IsDownloadInProgress = false;
            IsFilesDownload = false;
            IsPrepare = true;

            _instanceFormModel.InstanceClient.DownloadCanceled += () =>
            {
                _instanceFormModel.UpdateButtons();
                _instanceFormModel.OverviewField = _instanceFormModel.InstanceClient.Summary;
                IsPrepare = false;
            };
            _instanceFormModel.InstanceClient.CancelDownload();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void DownloadProcess(StageType downloadStageType, ProgressHandlerArguments progressArgs)
        {
            foreach (var action in DownloadActions)
            {
                action(downloadStageType, progressArgs);
            }
        }

        private void ComplitedDownloadAction(InstanceInit result, List<string> downloadErrors, bool launchGame)
        {
            foreach (var action in ComplitedDownloadActions)
            {
                action(result, downloadErrors, launchGame);
            }
        }


        #endregion Private Methods
    }
}
