using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Converters;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public sealed class InstanceProfileOverviewGalleryModel : ObservableObject
    {
        private readonly AppCore _appCore;

        public InstanceModelBase InstanceModel { get; }

		private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading; private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
		private bool _isInitializing;

		private bool _hasThumbnails;
		public bool HasThumbnails
		{
			get => _hasThumbnails;
			private set
			{
				_hasThumbnails = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<ThumbnailItem> Thumbnails { get; } = new();

		public InstanceProfileOverviewGalleryModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {
            _appCore = appCore;
            InstanceModel = instanceModelBase;
			IsLoading = true;
			Thumbnails.CollectionChanged += (_, _) => HasThumbnails = Thumbnails.Count > 0;
        }

		/// <summary>
		/// Initializes the additional data asynchronously without blocking the UI thread.
		/// All thumbnail generation happens on background threads.
		/// </summary>
		public async Task InitializeAsync()
		{
			if (_isInitializing) return;
			if (Thumbnails.Count > 0) return;

			_isInitializing = true;
			IsLoading = true;

			try
			{
				var data = await Task.Run(() => InstanceModel.AdditionalData).ConfigureAwait(true);

				if (data?.Images == null || data.Images.Count == 0)
					return;

				const int thumbWidth = 200;
				const int thumbHeight = 110;

				var images = data.Images;
				var thumbArray = new ThumbnailItem[images.Count];

				await Task.Run(() =>
				{
					Parallel.For(0, images.Count, i =>
					{
						thumbArray[i] = new ThumbnailItem
						{
							OriginalSource = images[i],
							ThumbnailImage = ImageToThumbnailConverter.ResizeImageWpf(images[i], thumbWidth, thumbHeight)
						};
					});
				}).ConfigureAwait(true);

				foreach (var item in thumbArray)
				{
					if (item != null)
						Thumbnails.Add(item);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to load gallery: {ex.Message}");
			}
			finally
			{
				_isInitializing = false;
				IsLoading = false;
			}
		}


		public void OpenImage(object value)
        {
            if (value is ThumbnailItem thumb)
            {
                var originals = new List<object>();
                foreach (var t in Thumbnails)
                    originals.Add(t.OriginalSource);

                _appCore.GalleryManager.ChangeContext(originals, thumb.OriginalSource);
            }
        }
    }

    public sealed class InstanceProfileOverviewGalleryViewModel : ViewModelBase
    {
        public InstanceProfileOverviewGalleryModel Model { get; }


        private RelayCommand _openImageCommand;
        public ICommand OpenImageCommand
        {
            get => RelayCommand.GetCommand<object>(ref _openImageCommand, Model.OpenImage);
        }


        public InstanceProfileOverviewGalleryViewModel(AppCore appCore, InstanceModelBase instanceModelBase)
        {
            Model = new(appCore, instanceModelBase);
        }
    }
}
