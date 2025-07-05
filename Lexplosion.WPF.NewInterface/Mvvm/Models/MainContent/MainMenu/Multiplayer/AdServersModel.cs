using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.ServerProfile;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AdServersModel : ObservableObject
    {
        private readonly AppCore _appCore;
        private readonly ICommand _backCommand;
        private readonly SelectInstanceForServerArgs _selectInstanceForServerArgs;
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;

        public ObservableCollection<MinecraftServerInstance> Servers { get; } = [];

        public AdServersModel(AppCore appCore, ICommand backCommand, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            _appCore = appCore;
            _selectInstanceForServerArgs = selectInstanceForServerArgs;
            _backCommand = backCommand;

            Runtime.TaskRun(() =>
            {
                var servers = Runtime.ServicesContainer.MinecraftService.GetMinecraftServersList();

                appCore.UIThread(() =>
                {
                    foreach (var server in servers)
                    {
                        SetOnlineInfoToServer(server);
                        Servers.Add(server);
                    };
                });
            });
        }


        #region Public Methods


        public void CopyServerIpAddress(MinecraftServerInstance server)
        {
            Clipboard.SetText(server.Address);
            _appCore.MessageService.Success("ServerIpAddressCopied", true);
        }

        public void ConnectToServer(MinecraftServerInstance server)
        {
            // vanilla
            if (server.InstanceSource == InstanceSource.None || server.InstanceSource == InstanceSource.Local)
            {
                _appCore.ModalNavigationStore.Open(new SelectInstanceForServerViewModel(server, _selectInstanceForServerArgs));
            }
            else // modded
            {
                _appCore.ModalNavigationStore.Open(new AskServerInstanceInstallingViewModel(_appCore, (isAutoConnectToServer) =>
                {
                    var ic = _clientsManager.CreateClient(server, isAutoConnectToServer);
                    _appCore.MessageService.Success("InstanceForServerAddedToLibrary", true, server.Name);
                }));
            }
        }

        public void OpenServerPage(MinecraftServerInstance server)
        {
            _appCore.NavigationStore.CurrentViewModel = new ServerProfileLayoutViewModel(_appCore, _backCommand, server);
        }


        #endregion Public Methods


        #region Private Methods


        private async void SetOnlineInfoToServer(MinecraftServerInstance server)
        {
            server.OnlineCount = await Runtime.ServicesContainer.MinecraftService.GetMcServerOnline(server);
        }

        internal void OnServerBannerLoaded(MinecraftServerInstance instance)
        {
            instance.IsBannerLoaded = true;
            OnPropertyChanged(nameof(instance.IsBannerLoaded));
        }


        #endregion Private Methods
    }
}
