using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileOverviewModel : ObservableObject 
    {
        public MinecraftServerInstance ServerModel { get; }

        public ServerProfileOverviewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            ServerModel = minecraftServerInstance;
        }
    }

    public sealed class ServerProfileOverviewViewModel : ViewModelBase
    {
        public ServerProfileOverviewModel Model { get; }

        public ServerProfileOverviewViewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            Model = new(appCore, minecraftServerInstance);
        }
    }
}
