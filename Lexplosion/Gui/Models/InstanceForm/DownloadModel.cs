using Lexplosion.Logic.Management.Instances;
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

        private InstanceClient _instanceClient;
        private MultibuttonModel _multibuttonModel;
        private bool _isIndeterminate;

        private bool _isDownloadInProgress;

        #region prop
        public bool IsDownloadInProgress 
        {
            get => _isDownloadInProgress;
            set 
            {
                _isDownloadInProgress = value;
                OnPropertyChanged(nameof(_isDownloadInProgress));
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

        public DownloadModel(InstanceClient instanceModel, MultibuttonModel multibuttonModel)
        {
            _instanceClient = instanceModel;
            _multibuttonModel = multibuttonModel;
        }

        #region methods

        public void DonwloadPrepare()
        {
            _instanceClient.AddToLibrary();
            _multibuttonModel.ChangeFuncProgressBar();
            _isDownloadInProgress = true;

            Lexplosion.Run.TaskRun(delegate
            {
                _instanceClient.UpdateInstance(Download, InstanceDownloadCompleted);
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
                //_instanceClient.OverviewField = "Идёт скачивание Java...";
                IsIndeterminate = false;
            }
            else if (downloadStageType == DownloadStageTypes.Prepare)
            {
                //_instanceClient.OverviewField = "Идёт подготовка к запуску...";
                IsIndeterminate = true;
            }
            else
            {
                //_instanceClient.OverviewField = String.Format("Идёт скачивание... Этап {0}/{1}", stage, stagesCount);
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
                        _multibuttonModel.ChangeFuncPlay();
                    }
                    break;
                case InstanceInit.DownloadFilesError:
                    {
                        IsDownloadInProgress = false;
                        _multibuttonModel.ChangeFuncDownload(true);
                        foreach (var de in downloadErrors)
                        {
                            Console.WriteLine("InstanceClient Download Completed --- Error: " + de);
                        }
                    }
                    break;
                default:
                    break;
            }
            //_instanceClient.OverviewField = _instanceClient.Properties.InstanceAssets.description;
        }

        public void CancelInstanceDownload() 
        {
            
        }
        #endregion
    }
}
