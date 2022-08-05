using Lexplosion.Gui.ViewModels;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class LaunchModel
    {
        private InstanceFormModel _formModel;
        private MainViewModel _mainViewModel;

        public bool IsGameLaunched { get; set; }

        public LaunchModel(InstanceFormModel instanceFormModel, MainViewModel mainViewModel)
        {
            _formModel = instanceFormModel;
            _mainViewModel = mainViewModel;
            instanceFormModel.InstanceClient.ComplitedLaunch += LaunchCompleted;
            instanceFormModel.InstanceClient.GameExited += GameExited;
        }

        #region methods

        public void LaunchInstance()
        {
            Lexplosion.Run.TaskRun(delegate
            {
                _formModel.DownloadModel.IsDownloadInProgress = true;
                _formModel.DownloadModel.IsIndeterminate = true;
                _formModel.InstanceClient.Run();
                _mainViewModel.IsInstanceRunning = true;
                _formModel.UpperButton.ChangeFuncClose();
            });
        }

        public void LaunchCompleted(string id, bool successful)
        {
            if (successful)
            {
                MainViewModel.ShowToastMessage(
                    "Launch Successfully Completed",
                    "Название: " + id
                );
            }
            else 
            {
            }
            _formModel.OverviewField = _formModel.InstanceClient.Summary;
        }

        public void GameExited(string id)
        {
            _formModel.UpperButton.ChangeFuncPlay();
            _mainViewModel.IsInstanceRunning = false;
        }

        #endregion
    }
}
