using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.ViewModels
{
    public class GalleryViewModel : VMBase
    {
        private BitmapImage _bitmapImage;

        private byte _selectedImagesIndex = 0;
        private bool _isNoneImages = false;
        private double _blurEffectRadius = 0;

        #region command
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

        #endregion

        #region props
        public double BlurEffectRadius 
        {
            get => _blurEffectRadius; set
            {
                _blurEffectRadius = value;
                OnPropertyChanged(nameof(BlurEffectRadius));
            }
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

        public bool IsNoneImages 
        {
            get => _isNoneImages; set
            {
                _isNoneImages = value;
                OnPropertyChanged(nameof(IsNoneImages));
                SelectedImage = new BitmapImage(
                    new System.Uri("pack://application:,,,/assets/images/background/regBG.png")
                );
                BlurEffectRadius = 10;
            }
        }
        #endregion 

        public ObservableCollection<BitmapImage> Images { get; } = new ObservableCollection<BitmapImage>();
        public GalleryViewModel(List<byte[]> images)
        {
            App.Current.Dispatcher.Invoke(() => {
                foreach (var i in images)
                    Images.Add(Utilities.ToImage(i));
                
                if (Images.Count > 0)
                {
                    SelectedImage = Images[SelectedImagesIndex];
                }
                else IsNoneImages = true;
            });
        }
    }
}
