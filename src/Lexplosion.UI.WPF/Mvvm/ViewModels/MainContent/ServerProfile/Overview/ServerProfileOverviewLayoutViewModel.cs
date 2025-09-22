using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Objects;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileOverviewLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _overviewViewModel;
        private readonly ViewModelBase _galleryViewModel;

        public ServerProfileOverviewLayoutViewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            _overviewViewModel = new ServerProfileOverviewViewModel(appCore, minecraftServerInstance);
            _galleryViewModel = new ServerProfileOverviewGalleryViewModel(appCore, minecraftServerInstance);

            _tabs.Add(new TabItemModel { Id = 0, TextKey = "Overview", Content = _overviewViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { Id = 1, TextKey = "Gallery", Content = _galleryViewModel });
        }
    }
}
