using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.ServerProfile;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileOverviewLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _overviewViewModel;
        private readonly ViewModelBase _galleryViewModel;

        public ServerProfileOverviewLayoutViewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            _overviewViewModel = new ServerProfileOverviewViewModel(appCore, minecraftServerInstance);
            _galleryViewModel = new ServerProfileOverviewGalleryViewModel(appCore, minecraftServerInstance);

            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Overview", Content = _overviewViewModel });
            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Gallery", Content = _galleryViewModel });
        }
    }
}
