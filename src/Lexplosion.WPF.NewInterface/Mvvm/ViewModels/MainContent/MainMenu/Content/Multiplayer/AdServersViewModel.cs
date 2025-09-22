using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{

    public class AdServersViewModel : ViewModelBase
    {
        public AdServersModel Model { get; }


        #region Commands


        private RelayCommand _openServerPageCommand;
        public ICommand OpenServerPageCommand
        {
            get => RelayCommand.GetCommand<MinecraftServerInstance>(ref _openServerPageCommand, Model.OpenServerPage);
        }

        private RelayCommand _connectToServerCommand;
        public ICommand ConnectToServerCommand
        {
            get => RelayCommand.GetCommand<MinecraftServerInstance>(ref _connectToServerCommand, Model.ConnectToServer);
        }

        private RelayCommand _copyAddressCommand;
        public ICommand CopyAddressCommand
        {
            get => RelayCommand.GetCommand<MinecraftServerInstance>(ref _copyAddressCommand, Model.CopyServerIpAddress);
        }

        private RelayCommand _serverBannerLoadedCommand;
        public ICommand ServerBannerLoadedCommand
        {
            get => RelayCommand.GetCommand<MinecraftServerInstance>(ref _serverBannerLoadedCommand, Model.OnServerBannerLoaded);
        }


        #endregion Commands


        public AdServersViewModel(AppCore appCore, ICommand backCommand, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            Model = new(appCore, backCommand, selectInstanceForServerArgs);
        }
    }
}
