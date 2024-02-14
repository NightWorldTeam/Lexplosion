using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.Common.ViewModels.AdvertisedServer
{
    public sealed class AdServerOverviewModel : VMBase 
    {
        private readonly MinecraftServerInstance _minecraftServerInstance;


        private ObservableCollection<ImageSource> _imageSources { get; set; } = new();
        public IEnumerable<ImageSource> Images { get => _imageSources; }


        private ObservableCollection<MinecraftServerInstance.Tag> _tags;
        public IEnumerable<MinecraftServerInstance.Tag> Tags { get => _tags; }
        public string Description { get => _minecraftServerInstance.Description; }
        public string GameVersion { get => _minecraftServerInstance.GameVersion; }

        public string OnlineCount { get; private set; } = null;
        public bool IsOnline { get; private set; }
        public bool IsStatusLoaded { get => _isImagesLoaded && _isOnlineLoaded; }

        private bool _isOnlineLoaded;
        private bool _isImagesLoaded;

        public AdServerOverviewModel(MinecraftServerInstance minecraftServerInstance)
        {
            _minecraftServerInstance = minecraftServerInstance;

            _tags = new ObservableCollection<MinecraftServerInstance.Tag>(_minecraftServerInstance.Tags);
            _tags.Insert(0, new MinecraftServerInstance.Tag(_minecraftServerInstance.GameVersion, ""));

            GetOnline();

            Runtime.TaskRun(() => {
                _imageSources = new(minecraftServerInstance.ImagesUrls.Select(i => ImageTools.GetImageByUrl(i)));
                OnPropertyChanged(nameof(Images));
                _isImagesLoaded = true;
                OnPropertyChanged(nameof(IsStatusLoaded));
            });
        }

        private async void GetOnline() 
        {
            var online = await ToServer.GetMcServerOnline(_minecraftServerInstance);
            OnlineCount = online.ToString();
            IsOnline = online > 0;
            OnPropertyChanged(nameof(OnlineCount));
            OnPropertyChanged(nameof(IsOnline));

            _isOnlineLoaded = true;
            OnPropertyChanged(nameof(IsStatusLoaded));
        }

        public void CopyIpToClipboard() 
        {
            Clipboard.SetText(_minecraftServerInstance.Address);
        }
    }

    public sealed class AdServerOverviewViewModel : VMBase
    {
        public AdServerOverviewModel Model { get; }

        public ICommand LaunchGameCommand { get; }

        private RelayCommand _copyIpAddress;
        public ICommand CopyIpAddress 
        {
            get => _copyIpAddress ?? (_copyIpAddress = new RelayCommand(obj => 
            {
                Model.CopyIpToClipboard();
            }));
        }

        public AdServerOverviewViewModel(MinecraftServerInstance adServer)
        {
            Model = new AdServerOverviewModel(adServer);
        }
    }
}
