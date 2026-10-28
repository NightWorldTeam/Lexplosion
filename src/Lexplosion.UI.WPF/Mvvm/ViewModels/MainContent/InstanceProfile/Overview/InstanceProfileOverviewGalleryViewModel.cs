using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
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
                OnPropertyChanged();
            }
        }

        private readonly Lazy<Task<InstanceData>> _additionalDataTask;
        private InstanceData _additionalData;

        public InstanceData AdditionalData
        {
            get
            {
                // Start once on first binding access; no repeated dispatcher callbacks.
                _ = _additionalDataTask.Value;
                return _additionalData;
            }
        }

        private async Task<InstanceData> LoadAdditionalDataAsync()
        {
            // Avoid raising PropertyChanged recursively inside Lazy.Value's factory
            // when the shared data task has already completed.
            await Task.Yield();
            try
            {
                var data = await InstanceModel.GetFullInfoAsync();
                _additionalData = data;
                OnPropertyChanged(nameof(AdditionalData));
                return data;
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Gallery full info failed: " + ex);
                return null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public InstanceProfileOverviewGalleryModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {
            _appCore = appCore;
            InstanceModel = instanceModelBase;
            _additionalDataTask = new Lazy<Task<InstanceData>>(LoadAdditionalDataAsync);
            IsLoading = true;
        }

        public async void OpenImage(object value)
        {
            var data = await _additionalDataTask.Value;
            if (data?.Images == null)
                return;

            _appCore.GalleryManager.ChangeContext(data.Images);
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
