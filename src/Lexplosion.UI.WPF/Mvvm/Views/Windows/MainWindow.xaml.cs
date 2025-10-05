using Lexplosion.Global;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Services;
using Lexplosion.UI.WPF.Core.Tools;
using Lexplosion.UI.WPF.WindowComponents.Header;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Lexplosion.UI.WPF.Mvvm.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DoubleAnimation _defaultChangeThemeAnimation = new DoubleAnimation()
        {
            From = 1700,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.35 * 3),
            EasingFunction = new SineEase
            { EasingMode = EasingMode.EaseInOut }
        };


        private readonly AppCore _appCore;
        private Gallery _gallery;
        private ScalingService _scalingService;

        public MainWindow(AppCore appCore)
        {
            _appCore = appCore;
            _appCore.Resources["ScalingFactorValue"] = _appCore.Settings.Core.ZoomLevel;

            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
            this.Closing += OmWindowClosing;

            PrepareAnimationForThemeService();

            HeaderContainer.DataContext = new WindowHeaderArgs(GlobalData.GeneralSettings.AppHeaderTemplateName, Close, Maximized, Minimized);
            _appCore.Settings.ThemeService.AppHeaderTemplateNameChanged += () =>
            {
                HeaderContainer.DataContext = new WindowHeaderArgs(_appCore.Settings.ThemeService.SelectedAppHeaderTemplateName, Close, Maximized, Minimized);
            };

            _gallery = appCore.GalleryManager;
            InitGallery();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _scalingService = new ScalingService(_appCore, this, ContainerGrid);
            _scalingService.ChangeNoFactorSizeValues(Width, Height);
            _scalingService.Rescale(this, ContainerGrid);
        }

        private void PrepareAnimationForThemeService()
        {
            var themeService = _appCore.Settings.ThemeService;

            _defaultChangeThemeAnimation.Completed += (sender, e) =>
            {
                PaintArea.Visibility = Visibility.Hidden;
            };

            themeService.BeforeAnimations.Add(() =>
            {
                PaintArea.Opacity = 1;
                PaintArea.Visibility = Visibility.Visible;
                PaintArea.Background = CreateBrushFromVisual(this);
            });

            themeService.Animations.Add(() =>
            {
                PaintArea.BeginAnimation(OpacityProperty, _defaultChangeThemeAnimation);
                CircleReveal.BeginAnimation(EllipseGeometry.RadiusXProperty, _defaultChangeThemeAnimation);
                CircleReveal.BeginAnimation(EllipseGeometry.RadiusYProperty, _defaultChangeThemeAnimation);
            });

            /// 
            /// Анимации для welcomepage
            /// 

            var welcomePageChangeThemeAnimation = new DoubleAnimation()
            {
                From = 1700,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.35 * 4),
                EasingFunction = new SineEase
                { EasingMode = EasingMode.EaseInOut }
            };

            themeService.BeforeAnimationsList["welcome-page"] = () =>
            {
                CircleReveal.Center = new Point(309, 261);
                PaintArea.Opacity = 1;
                PaintArea.Visibility = Visibility.Visible;
                PaintArea.Background = CreateBrushFromVisual(this);
            };

            themeService.AnimationsList["welcome-page"] = (complete) =>
            {
                welcomePageChangeThemeAnimation.Completed += (sender, e) =>
                {
                    CircleReveal.Center = new Point(101, 22);
                    complete?.Invoke();
                };
                PaintArea.BeginAnimation(OpacityProperty, welcomePageChangeThemeAnimation);
                CircleReveal.BeginAnimation(EllipseGeometry.RadiusXProperty, welcomePageChangeThemeAnimation);
                CircleReveal.BeginAnimation(EllipseGeometry.RadiusYProperty, welcomePageChangeThemeAnimation);
            };
        }

        private void InitGallery()
        {
            ImageViewer.Visibility = _gallery.HasSelectedImage ? Visibility.Visible : Visibility.Collapsed;

            _gallery.StateChanged += OnGalleryStateChanged;

            CloseImage.Click += OnCloseImageClicked;
            //NextImage.Click += OnNextImageClicked;
            //PrevImage.Click += OnPrevImageClicked;

            //// Создаем привязку
            //Binding hasPrevBinding = new Binding("HasPrev")
            //{
            //    Source = _gallery, // Источник данных
            //    Mode = BindingMode.OneWay, // Режим привязки (двусторонний)
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged // Обновление источника при изменении текста
            //};

            //// Создаем привязку
            //Binding hasNextBinding = new Binding("HasNext")
            //{
            //    Source = _gallery, // Источник данных
            //    Mode = BindingMode.OneWay, // Режим привязки (двусторонний)
            //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged // Обновление источника при изменении текста
            //};

            //// Устанавливаем привязку для свойства Text
            //PrevImage.SetBinding(FrameworkElement.IsEnabledProperty, hasPrevBinding);
            //NextImage.SetBinding(FrameworkElement.IsEnabledProperty, hasNextBinding);
        }

        private void OnGalleryStateChanged()
        {
            ImageViewer.Visibility = _gallery.HasSelectedImage ? Visibility.Visible : Visibility.Collapsed;

            if (!_gallery.HasSelectedImage)
                Image.ImageSource = null;
            if (Image.ImageSource == null || Image.ImageSource.ToString() == "pack://Application:,,,/Assets/images/icons/non_image.png")
            {
                BitmapImage image = null;

                Runtime.TaskRun(() =>
                {
                    if (_gallery.SelectedImageSource is byte[] byteImage)
                    {
                        image = ImageTools.ToImage(byteImage) ?? image;
                    }
                    else if (_gallery.SelectedImageSource is string stringImage)
                    {
                        image = new BitmapImage(new Uri(stringImage)) ?? image;
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Image.ImageSource = image;
                    });
                });

            }
        }

        #region Image Viewer


        private void OnPrevImageClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnNextImageClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnCloseImageClicked(object sender, RoutedEventArgs e)
        {
            _gallery.CloseImage();
        }


        #endregion ImageViewer

        private void OmWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Runtime.Exit();
        }


        #region Window State Buttons


        private void Close()
        {
            App.Current.MainWindow.Close();
        }

        private void Maximized()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                Runtime.DebugWrite(this.ActualWidth.ToString() + " x " + this.ActualHeight.ToString());
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Minimized()
        {
            this.WindowState = WindowState.Minimized;
        }


        #endregion Window State


        // TODO : Сделать для подобных махинаций отдельный класс
        /// <summary>
        /// Creates a brush based on the current appearance of a visual element. 
        /// The brush is an ImageBrush and once created, won't update its look
        /// </summary>
        /// <param name="v">The visual element to take a snapshot of</param>
        private Brush CreateBrushFromVisual(Visual v)
        {
            if (v == null)
                throw new ArgumentNullException("v");

            var _dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);

            var target = new RenderTargetBitmap((int)(this.ActualWidth * _dpi.DpiScaleX), (int)(this.ActualHeight * _dpi.DpiScaleY),
                                                _dpi.PixelsPerInchX, _dpi.PixelsPerInchY, PixelFormats.Default);
            target.Render(v);
            var brush = new ImageBrush(target);
            brush.Freeze();
            return brush;
        }

        private void Dba_Completed(object sender, EventArgs e, Border border)
        {
            PaintArea.Visibility = Visibility.Hidden;
            border.IsEnabled = true;
            // TODO: !IMPORTANT! Придумать как от этого избавиться. 
            //GC.Collect();
        }
    }
}
