using Lexplosion.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Lexplosion.Common.ViewModels
{
    public class GalleryViewModel : VMBase
    {
        #region Commands


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


        #endregion Commands


        #region Properties


        public ObservableCollection<BitmapImage> Images { get; } = new ObservableCollection<BitmapImage>();

        private double _blurEffectRadius;
        public double BlurEffectRadius
        {
            get => _blurEffectRadius; set
            {
                _blurEffectRadius = value;
                OnPropertyChanged();
            }
        }


        private byte _selectedImagesIndex;
        public byte SelectedImagesIndex
        {
            get => _selectedImagesIndex; set
            {
                _selectedImagesIndex = value;
                if (value > 0) IsLeftBorder = false;
                if (value < Images.Count - 1) IsRightBorder = false;
                SelectedImage = Images[value];
                OnPropertyChanged();
            }
        }

        private BitmapImage _bitmapImage;
        public BitmapImage SelectedImage
        {
            get => _bitmapImage; set
            {
                _bitmapImage = value;
                OnPropertyChanged();
            }
        }

        private bool _isNoneImages = false;
        public bool IsNoneImages
        {
            get => _isNoneImages; set
            {
                _isNoneImages = value;
                OnPropertyChanged();
                SelectedImage = new BitmapImage(
                    new System.Uri("pack://application:,,,/assets/images/background/authBG.png")
                );
                BlurEffectRadius = 10;
            }
        }

        private bool _isLeftBorder;
        public bool IsLeftBorder
        {
            get => _isLeftBorder; set
            {
                _isLeftBorder = value;
                OnPropertyChanged();
            }
        }

        private bool _isRightBorder;
        public bool IsRightBorder
        {
            get => _isRightBorder; set
            {
                _isRightBorder = value;
                OnPropertyChanged();
            }
        }


        #endregion Properities


        #region Constructors


        public GalleryViewModel(List<byte[]> images, ISubmenu submenuViewModel)
        {
            submenuViewModel.NavigationToMainMenu += ClearGallery;
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var i in images)
                    Images.Add(ImageTools.ToImageWithResize(i, 450, 240));

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


        #endregion Constructors


        #region Public & Protected Methods

        public void ClearGallery()
        {
            for (var i = 0; i < Images.Count; i++)
            {
                Images[i] = null;
            }
        }


        #endregion Public & Protected Methods
    }
}
