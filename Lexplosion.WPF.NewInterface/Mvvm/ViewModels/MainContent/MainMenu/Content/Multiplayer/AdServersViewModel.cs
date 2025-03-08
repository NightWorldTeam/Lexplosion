using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Modal;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Xml.Schema;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AdServersModel : ObservableObject
    {
        private readonly AppCore _appCore;
        private readonly SelectInstanceForServerArgs _selectInstanceForServerArgs;

        public ObservableCollection<MinecraftServerInstance> Servers { get; }

        public AdServersModel(AppCore appCore, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            _appCore = appCore;
            _selectInstanceForServerArgs = selectInstanceForServerArgs;

            var servers = ToServer.GetMinecraftServersList();

            foreach (var server in servers) 
            {
                SetOnlineInfoToServer(server);   
            }

            Servers = new(servers);
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
                    var ic = InstanceClient.CreateClient(server, isAutoConnectToServer);
                    _selectInstanceForServerArgs.AddNewInstanceInLibrary(ic);
                    _appCore.MessageService.Success("InstanceForServerAddedToLibrary", true, server.Name);
                }));
            }
        }

        public void OpenServerPage(MinecraftServerInstance server) 
        {
            
        }


        #endregion Public Methods


        #region Private Methods


        private async void SetOnlineInfoToServer(MinecraftServerInstance server)
        {
            server.OnlineCount = await ToServer.GetMcServerOnline(server);
        }

        internal void OnServerBannerLoaded(MinecraftServerInstance instance)
        {
            instance.IsBannerLoaded = true;
            OnPropertyChanged(nameof(instance.IsBannerLoaded));
        }


        #endregion Private Methods
    }

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


        public AdServersViewModel(AppCore appCore, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            Model = new(appCore, selectInstanceForServerArgs);
        }
    }
}
