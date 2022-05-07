using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class LaunchModel
    {
        private InstanceModel _instanceModel;
        private DownloadModel _downloadModel;
        private MultibuttonModel _multibuttonModel;

        public bool IsGameLaunched { get; set; }

        public LaunchModel(InstanceModel instanceModel, DownloadModel downloadModel, MultibuttonModel multibuttonModel)
        {
            _instanceModel = instanceModel;
            _downloadModel = downloadModel;
            _multibuttonModel = multibuttonModel;
        }

        #region methods
        public void LaunchInstance()
        {
            Lexplosion.Run.TaskRun(delegate
            {
                ManageLogic.СlientManager(_instanceModel.LocalId, _downloadModel.Download,
                    _downloadModel.InstanceDownloadCompleted, InstanceRunCompleted, InstanceGameExit);
            });
            MainViewModel.IsInstanceRunning = true;
            _multibuttonModel.ChangeFuncClose();
        }

        public void InstanceRunCompleted(string id, bool successful)
        {
            if (successful)
            {

            }
            else 
            {
                _multibuttonModel.ChangeFuncPlay();
            }
            _instanceModel.OverviewField = _instanceModel.Properties.InstanceAssets.description;
        }

        public void InstanceGameExit(string id)
        {
            MainViewModel.IsInstanceRunning = false;
        }
        #endregion
    }
}
