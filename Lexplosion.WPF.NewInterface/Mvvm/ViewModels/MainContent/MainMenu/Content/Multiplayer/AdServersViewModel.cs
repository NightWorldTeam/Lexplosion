using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AdServersModel : ObservableObject
    {
        private readonly AppCore _appCore;

        public ObservableCollection<MinecraftServerInstance> Servers { get; }

        public AdServersModel(AppCore appCore)
        {
            _appCore = appCore;

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
            // TODO
            //MainViewModel.ShowToastMessage(ResourceGetter.GetString("coping"), ResourceGetter.GetString("ipAddressCopiedSuccessfully"), 3, 0);
        }

        public void ConnectToServer(MinecraftServerInstance server)
        {
            // vanilla
            if (server.InstanceSource == InstanceSource.None || server.InstanceSource == InstanceSource.Local)
            {
                _appCore.ModalNavigationStore.Open(null);//(new SelectMenuInstanceForServerViewModel(server));
            }
            else // modded
            {
                //var dialog = new DialogViewModel(350, 220, true);
                //dialog.ShowDialog(ResourceGetter.GetString("serverInstanceInstallingTitle"), string.Format(ResourceGetter.GetString("serverInstanceInstallingDescription"), server.InstanceName), () =>
                //{
                //    var ic = InstanceClient.CreateClient(server, dialog.IsCheckBoxChecked);
                //    MainModel.Instance.AddInstanceForm(ic);
                //});
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


        #endregion Commands


        public AdServersViewModel(AppCore appCore)
        {
            Model = new(appCore);
        }
    }
}
