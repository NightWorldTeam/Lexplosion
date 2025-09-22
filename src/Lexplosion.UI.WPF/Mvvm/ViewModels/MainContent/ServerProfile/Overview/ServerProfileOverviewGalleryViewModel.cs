using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileOverviewGalleryModel : ObservableObject
    {
        private readonly AppCore _appCore;

        public ObservableCollection<byte[]> Images { get; private set; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ServerProfileOverviewGalleryModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            IsLoading = true;
            _appCore = appCore;
            Runtime.TaskRun(() =>
            {
                var images = minecraftServerInstance.GetImages();
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var i in images)
                    {
                        Images.Add(i);
                    }
                    IsLoading = false;
                });
            });
        }

        public void OpenImage(object value)
        {
            _appCore.GalleryManager.ChangeContext(Images);
            _appCore.GalleryManager.SelectImage(value);
        }
    }

    public sealed class ServerProfileOverviewGalleryViewModel : ViewModelBase
    {
        public ServerProfileOverviewGalleryModel Model { get; }


        private RelayCommand _openImageCommand;
        public ICommand OpenImageCommand
        {
            get => RelayCommand.GetCommand(ref _openImageCommand, Model.OpenImage);
        }


        public ServerProfileOverviewGalleryViewModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            Model = new(appCore, minecraftServerInstance);
        }
    }
}
