using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.ServerProfile
{
    public sealed class ServerProfileOverviewGalleryModel : ObservableObject
	{
		private readonly AppCore _appCore;
		private readonly MinecraftServerInstance _minecraftServerInstance;

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

		private bool _hasImages;
		public bool HasImages
		{
			get => _hasImages;
			private set
			{
				_hasImages = value;
				OnPropertyChanged();
			}
		}

		private bool _isInitializing;

        public ServerProfileOverviewGalleryModel(AppCore appCore, MinecraftServerInstance minecraftServerInstance)
        {
            IsLoading = true;
            _appCore = appCore;
			_minecraftServerInstance = minecraftServerInstance;
			Images.CollectionChanged += (_, _) => HasImages = Images.Count > 0;
		}

		public void Initialize()
		{
			if (_isInitializing) return;
			if (Images.Count > 0) return;

			_isInitializing = true;
			IsLoading = true;

			Runtime.TaskRun(() =>
			{
				try
				{
					var images = _minecraftServerInstance.GetImages();
					App.Current.Dispatcher.BeginInvoke(() =>
					{
						if (images != null)
						{
							foreach (var i in images)
							{
								Images.Add(i);
							}
						}
					});
				}
				finally
				{
					App.Current.Dispatcher.BeginInvoke(() =>
					{
						_isInitializing = false;
						IsLoading = false;
					});
				}
			});
		}

		public void OpenImage(object value)
        {
            _appCore.GalleryManager.ChangeContext(Images, value);
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
