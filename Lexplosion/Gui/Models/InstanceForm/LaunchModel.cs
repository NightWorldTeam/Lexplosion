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


        public bool IsGameLaunched { get => MainModel.Instance.IsInstanceRunning; }


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
                MainModel.Instance.RunningInstance = _formViewModel;
                MainModel.Instance.IsInstanceRunning = true;
                _formModel.UpdateButtons();
                _formModel.InstanceClient.Run();
                
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
                MainModel.Instance.IsInstanceRunning = false;
                MainViewModel.ShowToastMessage(
                    ResourceGetter.GetString("runUnsuccessfulNotification"),
                    ResourceGetter.GetString("instanceName") + " : " + _formModel.InstanceClient.Name,
                    Controls.ToastMessageState.Error
                );

                MainModel.Instance.IsInstanceRunning = false;
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
            MainModel.Instance.IsInstanceRunning = false;
            _formModel.DownloadModel.IsDownloadInProgress = false;
            _formModel.IsLaunch = false;

            _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
            _formModel.UpperButton.ChangeFuncPlay();
            _formModel.UpdateLowerButton();
        }


        #endregion Private Methods
    }
}
