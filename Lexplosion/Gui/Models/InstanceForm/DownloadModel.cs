using System;
using System.Collections.Generic;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class DownloadModel : VMBase
    {
        private int _downloadProgress;
        private int _stage;
        private int _stagesCount;
        private DownloadStageTypes _downloadStageType;

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

        public bool IsIndeterminate 
        {
            get => _isIndeterminate; set 
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
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

        public void Download(DownloadStageTypes downloadStageType, int stagesCount, int stage, int procent)
        {
            StagesCount = stagesCount;
            Stage = stage;
            DownloadProgress = procent;
            DownloadStageType = downloadStageType;

            if (downloadStageType == DownloadStageTypes.Java)
            {
                _instanceFormModel.OverviewField = "Идёт скачивание Java...";
                IsIndeterminate = false;
            }
            else if (downloadStageType == DownloadStageTypes.Prepare)
            {
                _instanceFormModel.OverviewField = "Идёт подготовка к запуску...";
                IsIndeterminate = true;
            }
            else
            {
                _instanceFormModel.OverviewField = String.Format("Идёт скачивание... Этап {0}/{1}", stage, stagesCount);
                IsIndeterminate = false;
            }
        }

        public void InstanceDownloadCompleted(InstanceInit result, List<string> downloadErrors, bool IsGameRun)
        {
            switch (result)
            {
                case InstanceInit.Successful:
                    {
                        IsDownloadInProgress = false;
                        _instanceFormModel.UpperButton.ChangeFuncPlay();
                        _instanceFormModel.UpdateLowerButton();
                    }
                    break;
                case InstanceInit.DownloadFilesError:
                    {
                        IsDownloadInProgress = false;
                        _instanceFormModel.UpperButton.ChangeFuncDownload(true);
                        foreach (var de in downloadErrors)
                        {
                            Console.WriteLine("InstanceClient Download Completed --- Error: " + de);
                        }
                    }
                    break;
                default:
                    IsDownloadInProgress = false;
                    break;
            }
            _instanceFormModel.OverviewField = _instanceFormModel.InstanceClient.Description;
        }

        public void CancelInstanceDownload() 
        {
            
        }
        #endregion
    }
}
