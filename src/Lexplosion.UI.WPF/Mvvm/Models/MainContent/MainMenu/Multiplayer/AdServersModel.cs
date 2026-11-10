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
using System.Threading.Tasks;
using Lexplosion.Logic.Lifecycle;
using System.Windows;
using System.Windows.Input;
#if DEBUG
using Lexplosion.QA;
using Lexplosion.UI.WPF.Core;
#endif

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.MainMenu
{
    public class AdServersModel : ObservableObject, IAsyncLoadable
    {
        private readonly AppCore _appCore;
        private readonly ICommand _backCommand;
        private readonly SelectInstanceForServerArgs _selectInstanceForServerArgs;
        private readonly ClientsManager _clientsManager = Runtime.ClientsManager;
        private int _loadVersion;
        private Task _loadTask;
        private bool _loaded;
        private bool _isLoading;
        private bool _hasLoadError;

        private readonly BulkObservableCollection<MinecraftServerInstance> _servers =
            new BulkObservableCollection<MinecraftServerInstance>();

        public ObservableCollection<MinecraftServerInstance> Servers => _servers;

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
        }

        public Task EnsureLoadedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_loaded) return Task.CompletedTask;
            if (_loadTask != null && !_loadTask.IsCompleted) return _loadTask;
            return RefreshAsync(cancellationToken);
        }

        // Keep the existing retry command intact.
        public void ReloadServers()
        {
            _ = RefreshAsync();
        }

        public Task RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            int version = Interlocked.Increment(ref _loadVersion);
            _loaded = false;
            _loadTask = LoadCoreAsync(version, cancellationToken);
            return _loadTask;
        }

        private async Task LoadCoreAsync(int version, CancellationToken cancellationToken)
        {
            IsLoading = true;
            HasLoadError = false;
            try
            {
                List<MinecraftServerInstance> servers;
#if DEBUG
                using (UiDebugTrace.Measure("servers.api-get", 5))
#endif
                    servers = await Runtime.ServicesContainer.MinecraftService
                        .GetMinecraftServersListAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                if (version != Volatile.Read(ref _loadVersion)) return;

                // Publishing one snapshot avoids a WPF layout pass for
                // every server card while the user navigates elsewhere.
#if DEBUG
                using (UiDebugTrace.Measure("servers.update-collection", 3))
#endif
                    _servers.ReplaceAll(servers);

                _loaded = true;
                foreach (var server in servers)
                    SetOnlineInfoToServer(server);
#if DEBUG
                if (PerformanceTelemetry.Enabled)
                    PerformanceTelemetry.Record(servers.Count == 0
                        ? "servers.load.empty" : "servers.load.success", 0);
#endif
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Allow a subsequent activation to retry.
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("[MinecraftServers] Load failed: " + ex.GetType().Name);
                if (version == Volatile.Read(ref _loadVersion))
                {
                    HasLoadError = true;
#if DEBUG
                    if (PerformanceTelemetry.Enabled)
                        PerformanceTelemetry.Record("servers.load.failed", 0);
#endif
                }
            }
            finally
            {
                if (version == Volatile.Read(ref _loadVersion))
                {
                    IsLoading = false;
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
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
