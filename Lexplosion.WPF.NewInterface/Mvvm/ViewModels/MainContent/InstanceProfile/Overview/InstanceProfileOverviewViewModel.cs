using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewModel : ViewModelBase
    {
        public InstanceModelBase InstanceModel { get; }
        public InstanceData InstanceData { get => InstanceModel.PageData; }
        public BaseInstanceData BaseInstanceData { get => InstanceModel.BaseData; }

        public bool IsLocal { get; set; }

        public Lazy<Task<InstanceData>> AdditionalDataLazy
        {
            get => new Lazy<Task<InstanceData>>(() => Task.Run(() => InstanceModel.AdditionalData));
        }

        private InstanceData _additionalData;
        public InstanceData AdditionalData
        {
            get
            {
                if (_additionalData == null)
                {
                    App.Current.Dispatcher.InvokeAsync(async () => AdditionalData = await AdditionalDataLazy.Value);
                }
                return _additionalData;
            }

            private set
            {
                _additionalData = value;
                OnPropertyChanged();
                _changeLoadingStatus?.Invoke(false);
            }
        }

        private readonly Action<bool> _changeLoadingStatus;

        public InstanceProfileOverviewModel(InstanceModelBase instanceModel, Action<bool> changeLoadingStatus)
        {
            InstanceModel = instanceModel;

            IsLocal = instanceModel.IsLocal;

            _changeLoadingStatus = changeLoadingStatus;
            instanceModel.DataChanged += OnDataChanged;

            if (instanceModel.IsLocal) 
            {
                changeLoadingStatus?.Invoke(false);
            }

            if (instanceModel.Source == InstanceSource.Nightworld)
            {
                IsLocal = true;
                changeLoadingStatus?.Invoke(false);
            }
        }

        private void OnDataChanged()
        {
            OnPropertyChanged(nameof(BaseInstanceData));
        }
    }

    public sealed class InstanceProfileOverviewViewModel : ViewModelBase
    {
        public InstanceProfileOverviewModel Model { get; }

        public InstanceProfileOverviewViewModel(InstanceModelBase instanceModel, Action<bool> changeLoadingStatus)
        {
            Model = new InstanceProfileOverviewModel(instanceModel, changeLoadingStatus);
        }
    }
}
