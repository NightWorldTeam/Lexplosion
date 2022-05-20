using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class LaunchModel
    {
        private InstanceClient _instanceModel;
        private DownloadModel _downloadModel;
        private MultibuttonModel _multibuttonModel;

        public bool IsGameLaunched { get; set; }

        public LaunchModel(InstanceClient instanceModel, DownloadModel downloadModel, MultibuttonModel multibuttonModel)
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
                //_instanceModel.IsDownloadingInstance = true;
                _instanceModel.Run(_downloadModel.Download, _downloadModel.InstanceDownloadCompleted, InstanceRunCompleted, InstanceGameExit);
                MainViewModel.IsInstanceRunning = true;
                _multibuttonModel.ChangeFuncClose();
            });
        }

        public void InstanceRunCompleted(string id, bool successful)
        {
            if (successful)
            {

            }
            else 
            {
            }
            //_instanceModel.OverviewField = _instanceModel.Properties.InstanceAssets.description;
        }

        public void InstanceGameExit(string id)
        {
            _multibuttonModel.ChangeFuncPlay();
            MainViewModel.IsInstanceRunning = false;
        }

        #endregion
    }
}
