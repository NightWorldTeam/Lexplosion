using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.ViewModels
{
    public class GalleryViewModel : VMBase
    {
        private BitmapImage _bitmapImage;
        private RelayCommand _prevImage;
        private RelayCommand _nextImage;

        private byte _selectedImagesIndex = 0;

        public RelayCommand PrevImage
        {
            get => new RelayCommand(obj =>
            {
                if (SelectedImagesIndex > 0)
                {
                    SelectedImagesIndex--;
                }
            });
        }

        public RelayCommand NextImage
        {
            get => new RelayCommand(obj =>
            {
                if ((Images.Count > SelectedImagesIndex + 1))
                {
                    SelectedImagesIndex++;
                }
            });
        }

        public byte SelectedImagesIndex
        {
            get => _selectedImagesIndex; set
            {
                _selectedImagesIndex = value;
                SelectedImage = Images[value];
                OnPropertyChanged(nameof(SelectedImagesIndex));
            }
        }

        public BitmapImage SelectedImage
        {
            get => _bitmapImage; set
            {
                _bitmapImage = value;
                OnPropertyChanged(nameof(SelectedImage));
            }
        }

        public ObservableCollection<BitmapImage> Images { get; } = new ObservableCollection<BitmapImage>();
        public GalleryViewModel(List<byte[]> images)
        {
            App.Current.Dispatcher.Invoke(() => {
                foreach (var i in images)
                {
                    Images.Add(Utilities.ToImage(i));
                }
                if (Images.Count > 0) 
                { 
                    SelectedImage = Images[SelectedImagesIndex];
                }
            });
        }
    }
}
