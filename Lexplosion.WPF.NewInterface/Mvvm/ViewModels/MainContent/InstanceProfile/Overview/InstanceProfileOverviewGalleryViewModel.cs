using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewGalleryModel : ObservableObject
    {
        private readonly AppCore _appCore;

        public InstanceModelBase InstanceModel { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; private set
            {
                _isLoading = value;
                Console.WriteLine("Test");
                OnPropertyChanged();
            }
        }

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
                OnPropertyChanged(nameof(AdditionalData.Images));
                OnPropertyChanged();
                IsLoading = false;
            }
        }

        public InstanceProfileOverviewGalleryModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {
            _appCore = appCore;
            InstanceModel = instanceModelBase;
            IsLoading = true;
        }

        public void OpenImage(object value)
        {
            _appCore.GalleryManager.ChangeContext(InstanceModel.AdditionalData.Images);
            _appCore.GalleryManager.SelectImage(value);
        }
    }

    public sealed class InstanceProfileOverviewGalleryViewModel : ViewModelBase
    {
        public InstanceProfileOverviewGalleryModel Model { get; }


        private RelayCommand _openImageCommand;
        public ICommand OpenImageCommand
        {
            get => RelayCommand.GetCommand<object>(ref _openImageCommand, Model.OpenImage);
        }


        public InstanceProfileOverviewGalleryViewModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {
            Model = new(appCore, instanceModelBase);
        }
    }
}
