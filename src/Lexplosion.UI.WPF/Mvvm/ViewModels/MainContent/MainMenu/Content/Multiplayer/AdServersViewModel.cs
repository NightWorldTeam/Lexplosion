using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Args;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using Lexplosion.Logic.Lifecycle;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{

    public class AdServersViewModel : ViewModelBase, IAsyncLoadable
    {
        public AdServersModel Model { get; }


        #region Commands

        private RelayCommand _reloadCommand;
        public ICommand ReloadCommand =>
            RelayCommand.GetCommand(ref _reloadCommand, Model.ReloadServers);



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

        public Task EnsureLoadedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Model.EnsureLoadedAsync(cancellationToken);
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Model.RefreshAsync(cancellationToken);
        }
    }
}
