using Lexplosion.Gui.ViewModels;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class LaunchModel
    {
        private readonly InstanceFormModel _formModel;
        private readonly MainViewModel _mainViewModel;


        #region Properties


        public bool IsGameLaunched { get => _mainViewModel.IsInstanceRunning; }


        #endregion Properties


        #region Constructors


        public LaunchModel(MainViewModel mainViewModel, InstanceFormModel instanceFormModel)
        {
            _mainViewModel = mainViewModel;
            _formModel = instanceFormModel;   
            instanceFormModel.InstanceClient.ComplitedLaunch += LaunchCompleted;
            instanceFormModel.InstanceClient.GameExited += GameExited;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void LaunchInstance()
        {
            Lexplosion.Run.TaskRun(() =>
            {
                _formModel.DownloadModel.IsDownloadInProgress = true;
                _formModel.DownloadModel.HasProcents = false;
                _formModel.InstanceClient.Run();
                _mainViewModel.IsInstanceRunning = true;
                _mainViewModel.RunningInstance = _formModel;
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void LaunchCompleted(string id, bool successful)
        {
            if (successful)
            {
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("runSuccessfulNotification"),
                    ResourceGetter.GetString("instanceName") + " : " + _formModel.InstanceClient.Name,
                    TimeSpan.FromSeconds(5)
                );

                _formModel.DownloadModel.IsDownloadInProgress = false;
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

        private void GameExited(string id)
        {
            _formModel.UpperButton.ChangeFuncPlay();
            _mainViewModel.IsInstanceRunning = false;
        }


        #endregion Private Methods
    }
}
