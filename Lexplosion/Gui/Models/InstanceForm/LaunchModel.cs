using Lexplosion.Gui.ViewModels;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class LaunchModel
    {
        private InstanceFormModel _formModel;
        private MainViewModel _mainViewModel;

        public bool IsGameLaunched { get; set; }

        public LaunchModel(MainViewModel mainViewModel, InstanceFormModel instanceFormModel)
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
                _formModel.DownloadModel.HasProcents = true;
                _formModel.InstanceClient.Run();
                _mainViewModel.IsInstanceRunning = true;
            });
        }

        public void LaunchCompleted(string id, bool successful)
        {
            if (successful)
            {
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("runSuccessfulNotification"),
                    ResourceGetter.GetString("instanceName") + " : " + _formModel.InstanceClient.Name,
                    TimeSpan.FromSeconds(5)
                );
                _formModel.DownloadModel.IsDownloadInProgress = false;
                _formModel.DownloadModel.HasProcents = false;
                _formModel.UpperButton.ChangeFuncClose();
            }
            else 
            {
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("runUnsuccessfulNotification"),
                    ResourceGetter.GetString("instanceName") + " : " + _formModel.InstanceClient.Name,
                    Controls.ToastMessageState.Error
                );
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
