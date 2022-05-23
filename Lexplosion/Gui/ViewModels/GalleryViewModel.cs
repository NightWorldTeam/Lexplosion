using System;
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

        private bool _isLeftBorder;
        private bool _isRightBorder;

        #region command
        public RelayCommand PrevImage
        {
            get => new RelayCommand(obj =>
            {
                if (SelectedImagesIndex > 0)
                {
                    SelectedImagesIndex--;
                }
                else IsLeftBorder = true;
            });
        }

        public RelayCommand NextImage
        {
            get => new RelayCommand(obj =>
            {
                if ((SelectedImagesIndex < Images.Count - 1))
                {
                    SelectedImagesIndex++;
                }
                else IsRightBorder = true;
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
                if (value > 0) IsLeftBorder = false;
                if (value < Images.Count - 1) IsRightBorder = false;
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

        public bool IsLeftBorder 
        {
            get => _isLeftBorder; set
            {
                _isLeftBorder = value;
                OnPropertyChanged(nameof(IsLeftBorder));
            }
        }

        public bool IsRightBorder 
        {
            get => _isRightBorder; set
            {
                _isRightBorder = value;
                OnPropertyChanged(nameof(IsRightBorder));
            }
        }
        #endregion 

        public ObservableCollection<BitmapImage> Images { get; } = new ObservableCollection<BitmapImage>();
        public GalleryViewModel(List<byte[]> images, ISubmenu submenuViewModel)
        {
            submenuViewModel.NavigationToMainMenu += ClearGallery;
            App.Current.Dispatcher.Invoke(() => {
                foreach (var i in images)
                    Images.Add(Utilities.ToImage(i));

                IsLeftBorder = true;

                if (Images.Count > 0)
                    SelectedImage = Images[SelectedImagesIndex];
                else
                {
                    IsNoneImages = true;
                    IsRightBorder = true;
                }
                if (Images.Count == 1) IsRightBorder = true;
            });
        }

        public void ClearGallery() 
        {
            for (var i = 0; i < Images.Count; i++) 
            {
                Images[i] = null;
            }
        }
    }
}
