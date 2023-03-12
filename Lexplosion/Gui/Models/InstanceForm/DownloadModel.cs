using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Tools;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class DownloadModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly InstanceFormModel _instanceFormModel;

        public readonly List<Action<DownloadStageTypes, ProgressHandlerArguments>> DownloadActions = new List<Action<DownloadStageTypes, ProgressHandlerArguments>>();
        public readonly List<Action<InstanceInit, List<string>, bool>> ComplitedDownloadActions = new List<Action<InstanceInit, List<string>, bool>>();


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

        private DownloadStageTypes _downloadStageType;
        public DownloadStageTypes DownloadStageType
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


        public DownloadModel(MainViewModel mainViewModel, InstanceFormModel instanceFormModel)
        {
            _mainViewModel = mainViewModel;
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
        public void Download(DownloadStageTypes downloadStageType, ProgressHandlerArguments progressHandlerArguments)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StagesCount = progressHandlerArguments.StagesCount;
                Stage = progressHandlerArguments.Stage;
                DownloadProgress = progressHandlerArguments.Procents;
                DownloadStageType = downloadStageType;
                TotalDownloadingFilesCount = progressHandlerArguments.TotalFilesCount;
                DownloadingFilesCount = progressHandlerArguments.FilesCount;


                if (downloadStageType != DownloadStageTypes.Prepare)
                {
                    IsPrepareOnly = false;
                    IsPrepare = false;
                }

                if (downloadStageType == DownloadStageTypes.Java)
                {
                    _instanceFormModel.OverviewField = ResourceGetter.GetString("javaInstalling");
                    HasProcents = false;
                    IsFilesDownload = true;
                }
                else if (downloadStageType == DownloadStageTypes.Prepare)
                {
                    _instanceFormModel.OverviewField = ResourceGetter.GetString("runPrepare");
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
                            if (_mainViewModel.RunningInstance != null && _mainViewModel.IsInstanceRunning && _mainViewModel.RunningInstance.Model == _instanceFormModel)
                            {
                                IsPrepareOnly = true;
                                _instanceFormModel.UpdateLowerButton();
                            }
                            else if (!IsPrepareOnly)
                            {
                                IsDownloadInProgress = false;
                                //TODO: ЛОКАЗИЛАЦИЯ ТУТ
                                MainViewModel.ShowToastMessage(
                                    "Download Successfully Completed",
                                    "Название: " + _instanceFormModel.InstanceClient.Name +
                                    "\nВерсия: " + _instanceFormModel.InstanceClient.GameVersion,
                                    TimeSpan.FromSeconds(5d)
                                    );
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
                            MainViewModel.ShowToastMessage("Не удалось скачать некоторые файлы", files, Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.CursforgeIdError:
                    case InstanceInit.NightworldIdError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Id Error", "Внешний id сборки некорректен.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.ServerError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Server Error", "Не удалось получить данные с сервера.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.GuardError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Guard Error", "Не удолось выполнить проверку файла.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.VersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Version Error", "Не удалось определить версию игры.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.ForgeVersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Forge Version Error", "Не удалось определить версию модлоадера.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.GamePathError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Game Path Error", "Недействительная директория игры.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.ManifestError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Manifest Error", "Не удалось загрузить манифест сборки.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.JavaDownloadError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Java Download Error", "Попробуйте поставить свой путь до джавы в настройках.", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.IsCancelled:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Скачивание сборки было успешно отменено.", "Название модпака: " + _instanceFormModel.InstanceClient.Name, Controls.ToastMessageState.Error);
                            break;
                        }
                    default:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Unknown Error", "Что-то непонятное произошло... Советуем выключить и включить.", Controls.ToastMessageState.Error);
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
                    _instanceFormModel.UpdateLowerButton();
                    _instanceFormModel.OverviewField = ResourceGetter.GetString("gameRunning");
                }

            });
        }

        public void CancelDownload()
        {
            _instanceFormModel.OverviewField = ResourceGetter.GetString("downloadCancelling");
            _instanceFormModel.InstanceClient.DownloadCanceled += () =>
            {
                _instanceFormModel.DownloadModel.HasProcents = false;
                _instanceFormModel.DownloadModel.IsDownloadInProgress = false;
                _instanceFormModel.DownloadModel.IsFilesDownload = false;
                _instanceFormModel.UpdateButtons();
                _instanceFormModel.OverviewField = _instanceFormModel.InstanceClient.Summary;
            };
            _instanceFormModel.InstanceClient.CancelDownload();
        }

        #endregion Public & Protected Methods


        #region Private Methods


        private void DownloadProcess(DownloadStageTypes downloadStageType, ProgressHandlerArguments progressArgs)
        {
            var actions = DownloadActions.ToArray();
            foreach (var action in actions)
            {
                action(downloadStageType, progressArgs);
            }
        }

        private void ComplitedDownloadAction(InstanceInit result, List<string> downloadErrors, bool launchGame)
        {
            var actions = ComplitedDownloadActions.ToArray();
            foreach (var action in actions)
            {
                action(result, downloadErrors, launchGame);
            }
        }


        #endregion Private Methods
    }
}
