using Lexplosion.Global;
using Lexplosion.Logic.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class DownloadModel : VMBase
    {
        private int _downloadProgress;
        private int _stage;
        private int _stagesCount;
        private DownloadStageTypes _downloadStageType;

        private InstanceModel _instanceModel;
        private MultibuttonModel _multibuttonModel;
        private bool _isIndeterminate;

        #region prop
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

        public DownloadModel(InstanceModel instanceModel, MultibuttonModel multibuttonModel)
        {
            _instanceModel = instanceModel;
            _multibuttonModel = multibuttonModel;
        }

        #region methods

        public void DonwloadPrepare()
        {
            if (_instanceModel.OutsideId != "") 
            {
                if (UserData.Instances.IsExistId(_instanceModel.OutsideId)) 
                {
                    _instanceModel.LocalId = UserData.Instances.ExternalIds[_instanceModel.OutsideId];
                }
                else 
                {
                    _instanceModel.LocalId = ManageLogic.CreateInstance(
                        _instanceModel.Properties.Name, _instanceModel.Properties.Type,
                        "", ModloaderType.None, "", _instanceModel.OutsideId);
                }
            }
            _multibuttonModel.ChangeFuncProgressBar();
            _instanceModel.IsDownloadingInstance = true;

            Lexplosion.Run.TaskRun(delegate
            {
                ManageLogic.UpdateInstance(_instanceModel.LocalId, Download, InstanceDownloadCompleted);
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
                _instanceModel.OverviewField = "Идёт скачивание Java...";
            }
            else if (downloadStageType == DownloadStageTypes.Prepare) 
            { 
                _instanceModel.OverviewField = "Идёт подготовка к запуску...";
                IsIndeterminate = true; 
            }
            else
            {
                _instanceModel.OverviewField = String.Format("Идёт скачивание... Этап {0}/{1}", stage, stagesCount);
                IsIndeterminate = false;
            }
        }

        public virtual void InstanceDownloadCompleted(InstanceInit result, List<string> downloadErrors, bool IsGameRun)
        {
            if (result == InstanceInit.Successful)
            {
                _instanceModel.IsInstalled = true;
                _instanceModel.IsDownloadingInstance = false;
                _multibuttonModel.ChangeFuncPlay();
            }
            else if (result == InstanceInit.DownloadFilesError) 
            { }
            else 
            { }
            _instanceModel.OverviewField = _instanceModel.Properties.InstanceAssets.description;
        }

        public virtual void CancelInstanceDownload() 
        {
            
        }
        #endregion
    }
}
