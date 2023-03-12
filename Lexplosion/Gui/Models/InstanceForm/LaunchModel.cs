using Lexplosion.Gui.ViewModels;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class LaunchModel
    {
        private readonly InstanceFormViewModel _formViewModel;
        private readonly InstanceFormModel _formModel;
        private readonly MainViewModel _mainViewModel;


        #region Properties


        public bool IsGameLaunched { get => _mainViewModel.IsInstanceRunning; }


        #endregion Properties


        #region Constructors


        public LaunchModel(MainViewModel mainViewModel, InstanceFormModel instanceFormModel, InstanceFormViewModel instanceFormViewModel)
        {
            _mainViewModel = mainViewModel;
            _formViewModel = instanceFormViewModel;
            _formModel = instanceFormModel;
            _formModel.InstanceClient.LaunchComplited += OnLaunchCompleted;
            _formModel.InstanceClient.GameExited += OnGameExited;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void LaunchInstance()
        {
            Lexplosion.Runtime.TaskRun(() =>
            {
                _mainViewModel.RunningInstance = _formViewModel;
                _formModel.InstanceClient.Run();
                _mainViewModel.IsInstanceRunning = true;
                _formModel.UpdateButtons();
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void OnLaunchCompleted(string id, bool successful)
        {
            if (successful)
            {
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("runSuccessfulNotification"),
                    ResourceGetter.GetString("instanceName") + " : " + _formModel.InstanceClient.Name,
                    TimeSpan.FromSeconds(5)
                );

                _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
            }
            else
            {
                _mainViewModel.IsInstanceRunning = false;
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("runUnsuccessfulNotification"),
                    ResourceGetter.GetString("instanceName") + " : " + _formModel.InstanceClient.Name,
                    Controls.ToastMessageState.Error
                );

                _mainViewModel.IsInstanceRunning = false;
                _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
                _formModel.UpperButton.ChangeFuncPlay();
            }

            _formModel.DownloadModel.IsDownloadInProgress = false;
            _formModel.DownloadModel.IsPrepare = false;
            _formModel.IsLaunch = false;
            _formModel.OverviewField = _formViewModel.Model.InstanceClient.Summary;
            _formModel.UpdateLowerButton();
        }

        private void OnGameExited(string id)
        {
            _mainViewModel.IsInstanceRunning = false;
            _formModel.DownloadModel.IsDownloadInProgress = false;
            _formModel.IsLaunch = false;

            _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
            _formModel.UpperButton.ChangeFuncPlay();
            _formModel.UpdateLowerButton();
        }


        #endregion Private Methods
    }
}
