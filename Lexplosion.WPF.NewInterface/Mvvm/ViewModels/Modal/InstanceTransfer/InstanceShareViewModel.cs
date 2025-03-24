﻿using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Modal;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceTransfer;
using System;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal.InstanceTransfer
{
    public sealed class InstanceShareModel : ViewModelBase 
    {
        // true - exporting | false - nothing
        public event Action<bool> SharePreparingStarted;


        private readonly Action<int> _navigateTo;
        private readonly InstanceClient _instanceClient;


        private readonly InstanceSharesController _instanceSharesController;


        #region Properties


        public InstanceFileTree InstanceFileTree { get; }
        public string InstanceName { get; set; }


        public bool IsAlreadySharing => _instanceClient.IsSharing;
        public bool IsPreparingToShare { get; private set; }


        private bool _isFullExport;
        /// <summary>
        /// Выбран экспорт всех файлов?
        /// </summary>
        public bool IsFullExport
        {
            get => _isFullExport; set
            {
                _isFullExport = value;
                InstanceFileTree.ReselectedAllUnits(value);
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceShareModel(InstanceClient instanceClient, InstanceSharesController controller, Action<int> navigateTo)
        {
            _instanceClient = instanceClient;
            InstanceName = _instanceClient.Name;
            InstanceFileTree = new InstanceFileTree(instanceClient);
            _instanceSharesController = controller;
            controller.ShareStopped += Controller_ShareStopped;
            _navigateTo = navigateTo;
        }

        private void Controller_ShareStopped(string obj)
        {
            if (_instanceClient.LocalId == obj) 
            {
                OnPropertyChanged(nameof(IsAlreadySharing));
                OnPropertyChanged(nameof(IsPreparingToShare));
            }
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает экспорт асинхронно (в другом потоке). Возвращает результат экспорта.
        /// </summary>
        /// <param name="fileName">Название файла</param>
        /// <returns></returns>
        public void Share()
        {
            IsPreparingToShare = true;
            SharePreparingStarted?.Invoke(true);
            Lexplosion.Runtime.TaskRun(() =>
            {
                var result = _instanceClient.Share(InstanceFileTree.UnitsList, out FileDistributor fileDistribution);

                //ExportResultHandler(result);

                App.Current.Dispatcher.Invoke(() =>
                {
                    var wrapper = new DistributedInstance(_instanceClient.LocalId, _instanceClient.Name, fileDistribution);
                    _instanceSharesController.AddActiveShare(wrapper);
                    OnPropertyChanged(nameof(IsAlreadySharing));
                    IsPreparingToShare = false;
                    OnPropertyChanged(nameof(IsPreparingToShare));
                    _instanceSharesController.ShareStopped += OnActiveShareStopped;
                    SharePreparingStarted?.Invoke(false);
                    _navigateTo(2);
                });
            });
        }


        public void OnActiveShareStopped(string id) 
        {
            if (id == _instanceClient.LocalId)
            {
                OnPropertyChanged(nameof(IsAlreadySharing));
            }
        }


        #endregion Public Methods
    }

    public sealed class InstanceShareViewModel : ActionModalViewModelBase, ILimitedAccess
    {
        public InstanceShareModel Model { get; }
        public bool HasAccess => throw new NotImplementedException();


        #region  Commands


        private RelayCommand _stopSharingCommand;
        public ICommand StopSharingCommand 
        {
            get => RelayCommand.GetCommand(ref _stopSharingCommand, () => 
            {
                //Model.StopShare
            });
        }

        #endregion Commands


        public InstanceShareViewModel(InstanceClient instanceClient, InstanceSharesController controller, Action<int> navigateTo)
        {
            Model = new InstanceShareModel(instanceClient, controller, navigateTo);
            ActionCommandExecutedEvent += OnInstanceShareActionCommandExecuted;
        }

        private void OnInstanceShareActionCommandExecuted(object obj)
        {
            Model.Share();
        }

        public void RefreshAccessData()
        {

        }
    }
}
