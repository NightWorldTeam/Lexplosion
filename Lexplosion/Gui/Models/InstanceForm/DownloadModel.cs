using Lexplosion.Gui.ViewModels;
using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class DownloadModel : VMBase
    {
        private int _downloadProgress;
        private int _stage;
        private int _stagesCount;
        private DownloadStageTypes _downloadStageType;

        private bool _isPrepareOnly = true;

        private InstanceFormModel _instanceFormModel;

        private bool _isIndeterminate;
        private bool _isDownloadInProgress;

        #region prop
        public bool IsDownloadInProgress 
        {
            get => _isDownloadInProgress;
            set 
            {
                _isDownloadInProgress = value;
                OnPropertyChanged();
            }
        }

        public int DownloadProgress
        {
            get => _downloadProgress; set
            {
                _downloadProgress = value;
                OnPropertyChanged(nameof(DownloadProgress));
            }
        }

        public int Stage
        {
            get => _stage; set
            {
                _stage = value;
                OnPropertyChanged(nameof(Stage));
            }
        }

        public int StagesCount
        {
            get => _stagesCount; set
            {
                _stagesCount = value;
                OnPropertyChanged(nameof(StagesCount));
            }
        }

        public bool HasProcents 
        {
            get => _isIndeterminate; set 
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(HasProcents));
            }
        }

        public DownloadStageTypes DownloadStageType
        {
            get => _downloadStageType; set
            {
                _downloadStageType = value;
                OnPropertyChanged(nameof(DownloadStageType));
            }
        }
        #endregion

        public DownloadModel(InstanceFormModel instanceFormModel)
        {
            _instanceFormModel = instanceFormModel;
            instanceFormModel.InstanceClient.ProgressHandler += Download;
            instanceFormModel.InstanceClient.ComplitedDownload += InstanceDownloadCompleted;
        }

        #region methods

        /// <summary>
        /// Запускает скачивание
        /// </summary>
        public void DonwloadPrepare()
        {
            _instanceFormModel.InstanceClient.AddToLibrary();
            _instanceFormModel.UpperButton.ChangeFuncProgressBar();
            IsDownloadInProgress = true;

            _instanceFormModel.UpdateLowerButton();

            Lexplosion.Run.TaskRun(delegate
            {
                _instanceFormModel.InstanceClient.UpdateInstance();
            });
        }

        /// <summary>
        /// Обрабатывает информацию о скачивании
        /// </summary>
        /// <param name="downloadStageType">Тип стадии></param>
        /// <param name="stagesCount">Количество стадиый</param>
        /// <param name="stage">Номер текущей стадии</param>
        /// <param name="procent">Процетны</param>
        public void Download(DownloadStageTypes downloadStageType, int stagesCount, int stage, int procent)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StagesCount = stagesCount;
                Stage = stage;
                DownloadProgress = procent;
                DownloadStageType = downloadStageType;

                if (downloadStageType != DownloadStageTypes.Prepare)
                {
                    _isPrepareOnly = false;
                }

                if (downloadStageType == DownloadStageTypes.Java)
                {
                    _instanceFormModel.OverviewField = "Идёт скачивание Java...";
                    HasProcents = false;
                }
                else if (downloadStageType == DownloadStageTypes.Prepare)
                {
                    _instanceFormModel.OverviewField = "Идёт подготовка к запуску...";
                    HasProcents = true;
                }
                else
                {
                    _instanceFormModel.OverviewField = "Идёт скачивание... Этап " + stage + '/' + stagesCount;
                    HasProcents = false;
                }
            });         
        }

        public void InstanceDownloadCompleted(InstanceInit result, IEnumerable<string> downloadErrors, bool IsGameRun)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Console.WriteLine("Strange Test");
                switch (result)
                {
                    case InstanceInit.Successful:
                        {
                            IsDownloadInProgress = false;
                            if (!_isPrepareOnly) { 
                                MainViewModel.ShowToastMessage(
                                    "Download Successfully Completed",
                                    "Название: " + _instanceFormModel.InstanceClient.Name + 
                                    "\nВерсия: " + _instanceFormModel.InstanceClient.GameVersion,
                                    TimeSpan.FromSeconds(5d)
                                    );
                            }
                            _instanceFormModel.UpperButton.ChangeFuncPlay();
                            _instanceFormModel.UpdateLowerButton();
                        }
                        break;
                    case InstanceInit.DownloadFilesError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            string files = "Не удалось скачать следующие файлы:\n";
                            foreach (var de in downloadErrors)
                            {
                                files += de + "\n";
                            }
                            files += "\nПовторное скачивание может решить проблему, но это не точно.\n";
                            MainViewModel.ShowToastMessage("Не удалось скачать некоторые файлы", files, Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.NightworldIdError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Nightworld Id Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break;
                    case InstanceInit.CursforgeIdError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            if (downloadErrors != null) 
                                foreach (var de in downloadErrors)
                                {
                                    MainViewModel.ShowToastMessage("Curseforge Id Error", de, Controls.ToastMessageState.Error);
                                }
                        }
                        break;
                    case InstanceInit.ServerError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            MainViewModel.ShowToastMessage("Server Error", "Не удалось получить данные с сервера.\nИгорёша просто не хочет дописывать парсер.\nПростите)))", Controls.ToastMessageState.Error);
                        }
                        break;
                    case InstanceInit.GuardError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Guard Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break;
                    case InstanceInit.VersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Version Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break;
                    case InstanceInit.ForgeVersionError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Forge Version Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break;
                    case InstanceInit.GamePathError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Game Path Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break;
                    case InstanceInit.ManifestError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Manifest Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break; 
                    case InstanceInit.JavaDownloadError:
                        {
                            IsDownloadInProgress = false;
                            _instanceFormModel.UpperButton.ChangeFuncDownload();
                            foreach (var de in downloadErrors)
                            {
                                MainViewModel.ShowToastMessage("Java Download Error", de, Controls.ToastMessageState.Error);
                            }
                        }
                        break;
                    default:
                        IsDownloadInProgress = false;
                        _instanceFormModel.UpperButton.ChangeFuncDownload();
                        foreach (var de in downloadErrors)
                        {
                            MainViewModel.ShowToastMessage("Unknown Error", de, Controls.ToastMessageState.Error);
                        }
                        break;
                }
                _instanceFormModel.OverviewField = _instanceFormModel.InstanceClient.Summary;
            }); 
        }

        public void CancelInstanceDownload() 
        {
            
        }

        #endregion
    }
}
