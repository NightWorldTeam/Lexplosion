using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class DownloadModel : VMBase
    {
        private readonly InstanceFormModel _instanceFormModel;

        public readonly List<Action<StageType, ProgressHandlerArguments>> DownloadActions = new List<Action<StageType, ProgressHandlerArguments>>();
        public readonly List<Action<InstanceInit, List<string>, bool>> ComplitedDownloadActions = new List<Action<InstanceInit, List<string>, bool>>();


        private readonly Action<string, string, uint, byte> _doNotification = (header, message, time, type) => {};


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


        public DownloadModel(InstanceFormModel instanceFormModel, Action<string, string, uint, byte> doNotification)
        {
            _doNotification = doNotification;
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
            App.Current.Dispatcher.Invoke(() =>
            {
                IsFilesDownload = false;
                switch (result)
                {
                    case InstanceInit.Successful:
                        {
                            if (MainModel.Instance.RunningInstance != null && MainModel.Instance.IsInstanceRunning && MainModel.Instance.RunningInstance.Model == _instanceFormModel)
                            {
                                IsPrepareOnly = true;
                                _instanceFormModel.UpdateLowerButton();
                            }
                            else if (!IsPrepareOnly)
                            {
                                IsDownloadInProgress = false;
                                //TODO: ЛОКАЗИЛАЦИЯ ТУТ
                                _doNotification(
                                    "Download Successfully Completed",
                                    "Название: " + _instanceFormModel.InstanceClient.Name +
                                    "\nВерсия: " + _instanceFormModel.InstanceClient.GameVersion, 5, 0);
                                _instanceFormModel.UpperButton.ChangeFuncPlay();
                                _instanceFormModel.UpdateLowerButton();
                            }
                        }
                        break;
                    case InstanceInit.DownloadFilesError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            string files = "Не удалось скачать следующие файлы:\n";
                            foreach (var de in downloadErrors ?? new List<string>())
                            {
                                files += de + "\n";
                            }
                            files += "\nПовторное скачивание может решить проблему, но это не точно.\n";
                            _doNotification("Не удалось скачать некоторые файлы", files, 0, 1);
                        }
                        break;
                    case InstanceInit.CursforgeIdError:
                    case InstanceInit.NightworldIdError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Id Error", "Внешний id сборки некорректен.", 0, 1);
                        }
                        break;
                    case InstanceInit.ServerError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Server Error", "Не удалось получить данные с сервера.", 0, 1);
                        }
                        break;
                    case InstanceInit.GuardError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Guard Error", "Не удолось выполнить проверку файла.", 0, 1);
                        }
                        break;
                    case InstanceInit.VersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Version Error", "Не удалось определить версию игры.", 0, 1);
                        }
                        break;
                    case InstanceInit.ForgeVersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Forge Version Error", "Не удалось определить версию модлоадера.", 0, 1);
                        }
                        break;
                    case InstanceInit.GamePathError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Game Path Error", "Недействительная директория игры.", 0, 1);
                        }
                        break;
                    case InstanceInit.ManifestError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Manifest Error", "Не удалось загрузить манифест сборки.", 0, 1);
                        }
                        break;
                    case InstanceInit.JavaDownloadError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Java Download Error", "Попробуйте поставить свой путь до джавы в настройках.", 0, 1);
                        }
                        break;
                    case InstanceInit.IsCancelled:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Скачивание сборки было успешно отменено.", "Название модпака: " + _instanceFormModel.InstanceClient.Name, 0, 1);
                            break;
                        }
                    default:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            _doNotification("Unknown Error", "Что-то непонятное произошло... Советуем выключить и включить.", 0, 1);
                        }
                        break;

                }

                if (!IsPrepareOnly)
                {
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
