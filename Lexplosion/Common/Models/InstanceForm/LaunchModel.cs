using Lexplosion.Common.ViewModels;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;

namespace Lexplosion.Common.Models.InstanceForm
{
    public sealed class LaunchModel
    {
        private readonly InstanceClient _instanceClient;
        private readonly InstanceFormViewModel _formViewModel;
        private readonly InstanceFormModel _formModel;
        private readonly MainViewModel _mainViewModel;


        #region Properties


        public bool IsGameLaunched { get => MainModel.Instance.IsInstanceRunning; }


        #endregion Properties


        #region Constructors


        public LaunchModel(InstanceClient instanceClient, MainViewModel mainViewModel, InstanceFormModel instanceFormModel, InstanceFormViewModel instanceFormViewModel)
        {
            _instanceClient = instanceClient;
            _mainViewModel = mainViewModel;
            _formViewModel = instanceFormViewModel;
            _formModel = instanceFormModel;
            _instanceClient.LaunchComplited += OnLaunchCompleted;
            _instanceClient.GameExited += OnGameExited;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public void LaunchInstance()
        {
            Runtime.TaskRun(() =>
            {
                MainModel.Instance.RunningInstance = _formViewModel;
                MainModel.Instance.IsInstanceRunning = true;
                _formModel.UpdateButtons();
                _instanceClient.Run();
            });
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void OnLaunchCompleted(string id, bool successful)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (successful)
                {
                    MainViewModel.ShowToastMessage(
                        ResourceGetter.GetString("runSuccessfulNotification"),
                        ResourceGetter.GetString("instanceName") + " : " + _instanceClient.Name,
                        TimeSpan.FromSeconds(5)
                    );

                    //asdasdasd
                    _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
                }
                else
                {
                    MainModel.Instance.IsInstanceRunning = false;
                    MainViewModel.ShowToastMessage(
                        ResourceGetter.GetString("runUnsuccessfulNotification"),
                        ResourceGetter.GetString("instanceName") + " : " + _instanceClient.Name,
                        Controls.ToastMessageState.Error
                    );

                    MainModel.Instance.IsInstanceRunning = false;
                    // asdasdasdasd
                    _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
                    _formModel.UpperButton.ChangeFuncPlay();
                }

                _formModel.DownloadModel.IsDownloadInProgress = false;
                _formModel.DownloadModel.IsPrepare = false;
                _formModel.IsLaunch = false;
                _formModel.OverviewField = _instanceClient.Summary;
                _formModel.UpdateLowerButton();
            });
        }

        private void OnGameExited(string id)
        {
            MainModel.Instance.IsInstanceRunning = false;
            _formModel.DownloadModel.IsDownloadInProgress = false;
            _formModel.IsLaunch = false;

            //asdasdasdasd
            _mainViewModel.InitTrayComponentsWithGame(_formViewModel);
            _formModel.UpperButton.ChangeFuncPlay();
            _formModel.UpdateLowerButton();
        }
        #endregion Private Methods
    }
}
