using Lexplosion.Gui.ViewModels;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class LaunchModel
    {
        private InstanceFormModel _formModel;

        public bool IsGameLaunched { get; set; }

        public LaunchModel(InstanceFormModel instanceFormModel)
        {
            _formModel = instanceFormModel;
        }

        #region methods

        public void LaunchInstance()
        {
            Lexplosion.Run.TaskRun(delegate
            {
                _formModel.DownloadModel.IsDownloadInProgress = true;
                _formModel.DownloadModel.IsIndeterminate = true;
                _formModel.InstanceClient.Run(
                    _formModel.DownloadModel.Download, _formModel.DownloadModel.InstanceDownloadCompleted, InstanceRunCompleted, InstanceGameExit
                    );
                MainViewModel.IsInstanceRunning = true;
                _formModel.ButtonModel.ChangeFuncClose();
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
            _formModel.OverviewField = _formModel.InstanceClient.Description;
        }

        public void InstanceGameExit(string id)
        {
            _formModel.ButtonModel.ChangeFuncPlay();
            MainViewModel.IsInstanceRunning = false;
        }

        #endregion
    }
}
