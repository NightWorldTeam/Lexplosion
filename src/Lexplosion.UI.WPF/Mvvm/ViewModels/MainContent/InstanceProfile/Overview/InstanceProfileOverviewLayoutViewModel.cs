using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewLayoutViewModel : ContentLayoutViewModelBase
    {
        private ViewModelBase _overviewViewModel;
        private ViewModelBase _galleryViewModel;
        private ViewModelBase _versionViewModel;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public InstanceProfileOverviewLayoutViewModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {
            IsLoading = true;
            _overviewViewModel = new InstanceProfileOverviewViewModel(instanceModelBase, ChangeLoadingStatus);
            _galleryViewModel = new InstanceProfileOverviewGalleryViewModel(appCore, instanceModelBase);

            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Description", Content = _overviewViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Gallery", Content = _galleryViewModel });


            if (!instanceModelBase.IsLocal)
            {
                _versionViewModel = new InstanceProfileVersionsViewModel(instanceModelBase);
                _tabs.Add(new TabItemModel { Id = 0, TextKey = "Versions", Content = _versionViewModel });
            }
        }

        private void ChangeLoadingStatus(bool newStatus)
        {
            IsLoading = newStatus;
        }
    }
}
