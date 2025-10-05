using Lexplosion.Global;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Tools;
using Lexplosion.UI.WPF.WindowComponents.Header;
using System;
using System.ComponentModel;
using System.Threading;
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

            InitScalingFactorHandler();
        }

        private void InitScalingFactorHandler()
        {
            this.SetResourceReference(ScalingFactorProperty, "ScalingFactorValue");

            // Watch for changes
            DependencyPropertyDescriptor
                .FromProperty(ScalingFactorProperty, typeof(FrameworkElement))
                .AddValueChanged(this, OnScalingFactorChanged);
        }

        private void OnScalingFactorChanged(object sender, EventArgs e)
        {
            if (_appCore.Settings.Core.IsScalingAnimationEnabled)
            {
                RescaleWithAnimation();
            }
            else 
            {
                Rescale();
            }
        }

        public static readonly DependencyProperty ScalingFactorProperty =
            DependencyProperty.RegisterAttached("ScalingFactor", typeof(double), typeof(MainWindow), new PropertyMetadata(1.0));

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NoFactorWidth = Width;
            NoFactorHeight = Height;

            Rescale();
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

        private bool _isScalled = false;
        public double ScalingKeff { get; private set; } = 1;
        public double ScalingFactor { get; private set; } = 1;


        public const int DefaultMinWidth = 944;
        public const int DefaultMinHeight = 528;

        private double NoFactorWidth;
        private double NoFactorHeight;

        private double PreviousScaleValue = 1;

        private void Rescale()
        {
            var scalingFactor = _appCore.Settings.Core.ZoomLevel;
            var isCenterWindowAuto = (bool)_appCore.Settings.Core.IsCenterWindowAuto;
            ScalingFactor = scalingFactor > 1 ? scalingFactor - 1 : 0;

            ScalingKeff = ScalingFactor + 1;
            var isScalled = ScalingFactor > 0;

            var scaleTransform = ContainerGrid.RenderTransform as ScaleTransform ?? new ScaleTransform(ScalingFactor, ScalingFactor);
            if (scaleTransform.ScaleX != ScalingKeff && scaleTransform.ScaleY != ScalingKeff)
            {
                var newMinWidth = isScalled ? DefaultMinWidth + DefaultMinWidth * ScalingFactor : DefaultMinWidth;
                var newMinHeight = isScalled ? DefaultMinHeight + DefaultMinHeight * ScalingFactor : DefaultMinHeight;

                var newWidth = NoFactorWidth * (1 + ScalingFactor);
                var newHeight = NoFactorHeight * (1 + ScalingFactor);

                ContainerGrid.LayoutTransform = new ScaleTransform(ScalingKeff, ScalingKeff);

                // Удаляем scope анимаций, чтобы иметь возможность изменить значения свойств.
                this.BeginAnimation(Window.MinWidthProperty, null);
                this.BeginAnimation(Window.MinHeightProperty, null);

                MinWidth = newMinWidth;
                MinHeight = newMinHeight;
                Width = newWidth;
                Height = newHeight;

                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

                Left = (screenWidth / 2) - (Width / 2);
                Top = (screenHeight / 2) - (Height / 2);
            }
        }

        private void RescaleWithAnimation(Action scaleAnimationCompletedAction = null)
        {
            var scalingFactor = _appCore.Settings.Core.ZoomLevel;
            var isCenterWindowAuto = (bool)_appCore.Settings.Core.IsCenterWindowAuto;
            ScalingFactor = scalingFactor > 1 ? scalingFactor - 1 : 0;

            ScalingKeff = ScalingFactor + 1;
            var isScalled = ScalingFactor > 0;

            var scaleTransform = ContainerGrid.RenderTransform as ScaleTransform ?? new ScaleTransform(ScalingFactor, ScalingFactor);

            if (scaleTransform.ScaleX != ScalingKeff && scaleTransform.ScaleY != ScalingKeff)
            {
                scaleTransform = new ScaleTransform(scaleTransform.ScaleX, scaleTransform.ScaleY);
                ContainerGrid.LayoutTransform = scaleTransform;

                var newWidth = NoFactorWidth * (1 + ScalingFactor);
                var newHeight = NoFactorHeight * (1 + ScalingFactor);

                var animation = new DoubleAnimation(PreviousScaleValue, ScalingKeff,
                    new Duration(TimeSpan.FromMilliseconds(300)))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

                var newMinWidth = isScalled ? DefaultMinWidth + DefaultMinWidth * ScalingFactor : DefaultMinWidth;
                var newMinHeight = isScalled ? DefaultMinHeight + DefaultMinHeight * ScalingFactor : DefaultMinHeight;

                Storyboard minPropertiesStoryboard = new Storyboard();

                var minWidthAnimation = new DoubleAnimation()
                {
                    To = newMinWidth,
                    From = MinWidth,
                    Duration = TimeSpan.FromMilliseconds(50)
                };

                var minHeightAnimation = new DoubleAnimation()
                {
                    To = newMinHeight,
                    From = MinHeight,
                    Duration = TimeSpan.FromMilliseconds(50)
                };

                Storyboard.SetTarget(minWidthAnimation, this);
                Storyboard.SetTargetProperty(minWidthAnimation, new PropertyPath(Window.MinWidthProperty));
                Storyboard.SetTarget(minHeightAnimation, this);
                Storyboard.SetTargetProperty(minHeightAnimation, new PropertyPath(Window.MinHeightProperty));
                minPropertiesStoryboard.Children.Add(minWidthAnimation);
                minPropertiesStoryboard.Children.Add(minHeightAnimation);

                minPropertiesStoryboard.Completed += (s, e) =>
                {
                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        Thread.Sleep(100);
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MinWidth = newMinWidth;
                            MinHeight = newMinHeight;
                            Width = newWidth;
                            Height = newHeight;
                            if (isCenterWindowAuto)
                            {
                                CenterWindow();
                            }
                            else if (scaleAnimationCompletedAction != null)
                            {
                                scaleAnimationCompletedAction.Invoke();
                            }
                        });
                    });
                };

                minPropertiesStoryboard.Begin();

                PreviousScaleValue = ScalingKeff;
                _isScalled = isScalled;
            }
        }

        private void CenterWindow()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            Storyboard centerWindowStoryboard = new Storyboard();

            var newLeft = (screenWidth / 2) - (Width / 2);
            var newTop = (screenHeight / 2) - (Height / 2);

            var leftPointAnimation = new DoubleAnimation()
            {
                To = newLeft,
                From = this.Left,
                Duration = TimeSpan.FromMilliseconds(1500),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }, // SineEase
                DecelerationRatio = 0.1
            };

            var topPointAnimation = new DoubleAnimation()
            {
                To = newTop,
                From = this.Top,
                Duration = TimeSpan.FromMilliseconds(1500),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn },
                DecelerationRatio = 0.1
            };

            Storyboard.SetTarget(leftPointAnimation, this);
            Storyboard.SetTargetProperty(leftPointAnimation, new PropertyPath(Window.LeftProperty));
            Storyboard.SetTarget(topPointAnimation, this);
            Storyboard.SetTargetProperty(topPointAnimation, new PropertyPath(Window.TopProperty));
            centerWindowStoryboard.Children.Add(leftPointAnimation);
            centerWindowStoryboard.Children.Add(topPointAnimation);

            centerWindowStoryboard.Begin();
        }
    }
}
