using Lexplosion.Common.Models;
using Lexplosion.Common.Models.Objects;
using Lexplosion.Common.ViewModels.AdvertisedServer;
using Lexplosion.Common.ViewModels.ModalVMs;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public class AdServersListTabViewModel : VMBase
    {
        public ObservableCollection<MinecraftServerInstance> Servers { get; } = new();

        private readonly MainViewModel _mainViewModel;

        private readonly ObservableCollection<Tab<VMBase>> _showCaseTabMenu = new ObservableCollection<Tab<VMBase>>();


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
                if (server.InstanceSource == InstanceSource.Local)
                {
                    _mainViewModel.ModalWindowVM.Open(new SelectMenuInstanceForServerViewModel(server));
                }
                else // modded
                {
                    // TODO: Translate
                    new DialogViewModel().ShowDialog("Установка сборки сервера.", $"Для игры на данном сервере требуется установить сборку {server.InstanceName}, желаете установить данную сборку?", () => 
                    {
                        
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
                MainViewModel.ShowToastMessage("Копирование", "Ip address успешно был скопирован", 3, 0);
            }));
        }


        #endregion Commands

        
        public AdServersListTabViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Servers = new(ToServer.GetMinecraftServersList());
            OnPropertyChanged(nameof(Servers));
        }

        #region Private Methods


        #endregion Private Methods
    }
}
