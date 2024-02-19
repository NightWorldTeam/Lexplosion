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
        public string IconUrl { get => _minecraftServerInstance.IconUrl; }

        public MinecraftServerInstance.Links ExternalLinks { get => _minecraftServerInstance.SocialLinks; }

        public int OnlineCount { get; private set; }
        public bool IsOnline { get; private set; }
        public bool IsImagesLoading { get; private set; } = true;
        public bool IsOnlineLoading { get; private set; } = true;

        public AdServerOverviewModel(MinecraftServerInstance minecraftServerInstance)
        {
            _minecraftServerInstance = minecraftServerInstance;

            _tags = new ObservableCollection<MinecraftServerInstance.Tag>(_minecraftServerInstance.Tags);
            _tags.Insert(0, new MinecraftServerInstance.Tag(_minecraftServerInstance.GameVersion, ""));

            GetOnline();

            Runtime.TaskRun(() => {
                _imageSources = new(minecraftServerInstance.ImagesUrls.Select(i => ImageTools.GetImageByUrl(i)));
                OnPropertyChanged(nameof(Images));
                IsImagesLoading = false;
                OnPropertyChanged(nameof(IsImagesLoading));
            });
        }

        private async void GetOnline() 
        {
            OnlineCount = await ToServer.GetMcServerOnline(_minecraftServerInstance); ;
            IsOnline = OnlineCount > 0;
            OnPropertyChanged(nameof(OnlineCount));
            OnPropertyChanged(nameof(IsOnline));
            IsOnlineLoading = false;
            OnPropertyChanged(nameof(IsOnlineLoading));
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

        private RelayCommand _goToExternalLinkCommand;
        public ICommand GoToExternalLinkCommand
        {
            get => _goToExternalLinkCommand ?? (_goToExternalLinkCommand = new RelayCommand(obj =>
            {
                System.Diagnostics.Process.Start(obj as string);
            }));
        }

        public AdServerOverviewViewModel(MinecraftServerInstance adServer)
        {
            Model = new AdServerOverviewModel(adServer);
        }
    }
}
