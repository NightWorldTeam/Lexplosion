using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Args;
using Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile;
using Lexplosion.UI.WPF.Mvvm.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
#if DEBUG
using Lexplosion.QA;
using Lexplosion.UI.WPF.Core;
#endif

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AdServersModel : ObservableObject
    {
        private readonly AppCore _appCore;
        private readonly ICommand _backCommand;
        private readonly SelectInstanceForServerArgs _selectInstanceForServerArgs;
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;
        private int _loadVersion;
        private bool _isLoading;
        private bool _hasLoadError;

        public ObservableCollection<MinecraftServerInstance> Servers { get; } =
            new ObservableCollection<MinecraftServerInstance>();

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public bool HasLoadError
        {
            get => _hasLoadError;
            private set
            {
                if (_hasLoadError == value) return;
                _hasLoadError = value;
                OnPropertyChanged(nameof(HasLoadError));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public bool IsEmpty => !IsLoading && !HasLoadError && Servers.Count == 0;

        public AdServersModel(AppCore appCore, ICommand backCommand,
            SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            _appCore = appCore;
            _selectInstanceForServerArgs = selectInstanceForServerArgs;
            _backCommand = backCommand;
            ReloadServers();
        }

        public void ReloadServers()
        {
            int version = Interlocked.Increment(ref _loadVersion);
            Servers.Clear();
            HasLoadError = false;
            IsLoading = true;

            Runtime.TaskRun(() =>
            {
                bool success;
                List<MinecraftServerInstance> servers;
                try
                {
#if DEBUG
                    using (UiDebugTrace.Measure("servers.api-get", 5))
#endif
                        success = Runtime.ServicesContainer.MinecraftService
                            .TryGetMinecraftServersList(out servers);
                }
                catch (Exception ex)
                {
                    Runtime.DebugWrite("[MinecraftServers] Load failed: " + ex.GetType().Name);
                    success = false;
                    servers = new List<MinecraftServerInstance>();
                }

                _appCore.UIThread(() =>
                {
                    if (version != Volatile.Read(ref _loadVersion)) return;

                    if (success)
                    {
                        foreach (var server in servers)
                        {
                            Servers.Add(server);
                            SetOnlineInfoToServer(server);
                        }
                    }
                    HasLoadError = !success;
                    IsLoading = false;
                    OnPropertyChanged(nameof(IsEmpty));
#if DEBUG
                    if (PerformanceTelemetry.Enabled)
                        PerformanceTelemetry.Record(!success
                            ? "servers.load.failed"
                            : Servers.Count == 0 ? "servers.load.empty" : "servers.load.success", 0);
#endif
                });
            });
        }

        public void CopyServerIpAddress(MinecraftServerInstance server)
        {
            Clipboard.SetText(server.Address);
            _appCore.MessageService.Success("ServerIpAddressCopied", true);
        }

        public void ConnectToServer(MinecraftServerInstance server)
        {
            if (server.InstanceSource == InstanceSource.None ||
                server.InstanceSource == InstanceSource.Local)
            {
                _appCore.ModalNavigationStore.Open(
                    new SelectInstanceForServerViewModel(server, _selectInstanceForServerArgs));
            }
            else
            {
                _appCore.ModalNavigationStore.Open(
                    new AskServerInstanceInstallingViewModel(_appCore, (isAutoConnectToServer) =>
                    {
                        _clientsManager.CreateClient(server, isAutoConnectToServer);
                        _appCore.MessageService.Success(
                            "InstanceForServerAddedToLibrary", true, server.Name);
                    }));
            }
        }

        public void OpenServerPage(MinecraftServerInstance server)
        {
            _appCore.NavigationStore.CurrentViewModel =
                new ServerProfileLayoutViewModel(_appCore, _backCommand, server);
        }

        private async void SetOnlineInfoToServer(MinecraftServerInstance server)
        {
            server.OnlineCount = await Runtime.ServicesContainer.MinecraftService
                .GetMcServerOnline(server);
        }

        internal void OnServerBannerLoaded(MinecraftServerInstance instance)
        {
            instance.IsBannerLoaded = true;
            OnPropertyChanged(nameof(instance.IsBannerLoaded));
        }
    }
}
