using Lexplosion.Common.Models;
using Lexplosion.Common.ViewModels.AdvertisedServer;
using Lexplosion.Common.ViewModels.ModalVMs;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public class AdServersListTabViewModel : VMBase
    {
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<MinecraftServerInstance> Servers { get; } = new();

        
        #region Commands


        private RelayCommand _openServerPageCommand;
        public ICommand OpenServerPageCommand 
        {
            get => _openServerPageCommand ?? (_openServerPageCommand = new RelayCommand(obj => 
            {
                MainViewModel.NavigationStore.CurrentViewModel = new AdvertisedServerLayoutViewModel(_mainViewModel, obj as MinecraftServerInstance);
            }));
        }

        private RelayCommand _connectToServerCommand;
        public ICommand ConnectToServerCommand
        {
            get => _connectToServerCommand ?? (_connectToServerCommand = new RelayCommand(obj =>
            {
                //var server = obj as MinecraftServerInstance;
                //server.ConnectTo();
                var server = obj as MinecraftServerInstance;
                // vanilla
                if (server.InstanceSource == InstanceSource.None || server.InstanceSource == InstanceSource.Local)
                {
                    _mainViewModel.ModalWindowVM.Open(new SelectMenuInstanceForServerViewModel(server));
                }
                else // modded
                {
                    // TODO: Auto COnnect
                    new DialogViewModel().ShowDialog(ResourceGetter.GetString("serverInstanceInstallingTitle"), string.Format(ResourceGetter.GetString("serverInstanceInstallingDescription"), server.InstanceName), () =>
                    {
                        var ic = InstanceClient.CreateClient(server, false);
                        MainModel.Instance.AddInstanceForm(ic);
                    });
                }
            }));
        }


        private RelayCommand _copyAddressCommand;
        public ICommand CopyAddressCommand
        {
            get => _copyAddressCommand ?? (_copyAddressCommand = new RelayCommand(obj =>
            {
                Clipboard.SetText((obj as MinecraftServerInstance).Address);
                // TODO
                MainViewModel.ShowToastMessage(ResourceGetter.GetString("coping"), ResourceGetter.GetString("ipAddressCopiedSuccessfully"), 3, 0);
            }));
        }


        #endregion Commands

        
        public AdServersListTabViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            var s = ToServer.GetMinecraftServersList();

            foreach (var i in s)
            {
                GetOnline(i);
            }

            Servers = new(s);
            OnPropertyChanged(nameof(Servers));

#if DEBUG
            var g = new MinecraftServerInstance("", "ASD", "", "", new System.Collections.Generic.List<MinecraftServerInstance.Tag>(), "1.20.2", "", "", new System.Collections.Generic.List<string>(), "", "", InstanceSource.Local);
            Servers.Add(g);
            Servers.Add(g);
            Servers.Add(g);
            Servers.Add(g);
            Servers.Add(g);
            Servers.Add(g);
#endif
        }


        #region Private Methods


        private async void GetOnline(MinecraftServerInstance minecraftServerInstance)
        {
            minecraftServerInstance.OnlineCount = await ToServer.GetMcServerOnline(minecraftServerInstance);
        }


        #endregion Private Methods
    }
}
