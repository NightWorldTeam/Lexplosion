using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileOverviewGalleryModel : ObservableObject  
    {
        private readonly AppCore _appCore;

        public MinecraftServerInstance ServerModel { get; }

        public ServerProfileOverviewGalleryModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            _appCore = appCore;
            ServerModel = minecraftServerInstance;
        }

        public void OpenImage(string value) 
        {
            _appCore.GalleryManager.ChangeContext(ServerModel.ImagesUrls);
            _appCore.GalleryManager.SelectImage(value);
        }
    }

    public sealed class ServerProfileOverviewGalleryViewModel : ViewModelBase
    {
        public ServerProfileOverviewGalleryModel Model { get; }


        private RelayCommand _openImageCommand;
        public ICommand OpenImageCommand 
        {
            get => RelayCommand.GetCommand<string>(ref _openImageCommand, Model.OpenImage);
        }


        public ServerProfileOverviewGalleryViewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            Model = new(appCore, minecraftServerInstance);
        }
    }
}
