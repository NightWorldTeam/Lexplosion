using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Threading;
using System.Windows.Documents;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewLayoutViewModel : ContentLayoutViewModelBase
    {
        private ViewModelBase _overviewViewModel;
        private ViewModelBase _galleryViewModel;
        private ViewModelBase _versionViewModel;

        public InstanceProfileOverviewLayoutViewModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {

            _overviewViewModel = new InstanceProfileOverviewViewModel(instanceModelBase);
            _galleryViewModel = new InstanceProfileOverviewGalleryViewModel(appCore, instanceModelBase);

            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Description", Content = _overviewViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Gallery", Content = _galleryViewModel });


            if (!instanceModelBase.IsLocal)
            {
                _versionViewModel = new InstanceProfileVersionsViewModel(instanceModelBase);
                _tabs.Add(new TabItemModel { Id = 0, TextKey = "Versions", Content = _versionViewModel });
            }
        }
    }
}
