﻿using Lexplosion.WPF.NewInterface.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Collections;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.WindowComponents.Header;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Tools;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    public interface IScalable
    {
        double ActualWidth { get; }
        double ActualHeight { get; }
        double ScalingFactor { get; }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IScalable
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
        private bool _isScalled = false;
        private Gallery _gallery;


        public string currentLang = "ru";

        public double ScalingKeff { get; private set; } = 1;
        public double ScalingFactor { get; private set; } = 1;

        public MainWindow(AppCore appCore)
        {
            InitializeComponent();

            _appCore = appCore;

            PrepareAnimationForThemeService();

            MouseDown += delegate { try { DragMove(); } catch { } };
			this.Closing += Close;

			HeaderContainer.DataContext = new WindowHeaderArgs(Close, Maximized, Minimized);

            _gallery = appCore.GalleryManager;
            InitGallery();
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


        private void Scalling()
        {
            double factor = 0.25;
            var yScale = factor + 1;

            if (_isScalled)
            {
                factor *= -1;
                yScale = 1;
            }

            ContainerGrid.LayoutTransform = new ScaleTransform(yScale, yScale);
            this.Width += Width * factor;
            this.Height += Height * factor;
            ScalingFactor = factor;
            // Bring window center screen
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            Top = (screenHeight - Height) / 2;
            Left = (screenWidth - Width) / 2;

            _isScalled = !_isScalled;
        }

        private void Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Runtime.Exit();
        }


        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;

            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                //Runtime.DebugWrite(i.ToString() + " " + grid.ColumnDefinitions[i].ActualWidth.ToString());
            }
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

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void MaximazedWindow_Click(object sender, RoutedEventArgs e)
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

        private void MinimazedWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        #endregion Window State


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //App.Current.Resources.MergedDictionaries.Clear();

            App.Current.Resources.MergedDictionaries.Clear();

            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Fonts.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Icons.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/TextBlock.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/Buttons.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/TextBox.xaml")
            });
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources/Styles/CheckBox.xaml")
            });

            if (currentLang == "ru")
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Assets/langs/ru-RU.xaml")
                });
            }
            else
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Assets/langs/en-US.xaml")
                });
            }
        }


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


        public static bool Contains(ICollection collection, string key)
        {
            if (collection == null || collection.Count == 0)
                return false;

            foreach (var item in collection)
            {
                if (item is string str)
                {
                    if (str.Contains(key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ChangeTheme_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var themeService = _appCore.Settings.ThemeService;
            Theme selectedTheme = null;
            //if (themeService.SelectedTheme.Name == "Open Space")
            //{
            //    var resourceLoader = new ResourcesLoader();
            //    var newTheme = resourceLoader.LoadThemeFromPath("D:\\EmptyFolder\\Theme1.xml");
            //    themeService.AddAndActiveTheme(newTheme.Item2);
            //    return;
            //}
            //else 
            if (themeService.SelectedTheme.Name == "Light Punch")
            {
                selectedTheme = themeService.Themes.FirstOrDefault(t => t.Name == "Open Space");
            }
            else
            {
                selectedTheme = themeService.Themes.FirstOrDefault(t => t.Name == "Light Punch");
            }

            themeService.ChangeTheme(selectedTheme);
        }

        private void Dba_Completed(object sender, EventArgs e, Border border)
        {
            PaintArea.Visibility = Visibility.Hidden;
            border.IsEnabled = true;
            // TODO: !IMPORTANT! Придумать как от этого избавиться. 
            //GC.Collect();
        }

        private void ChangeLanguage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentLang == "ru")
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Assets/langs/en-US.xaml")
                });
                currentLang = "en";
            }
            else
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Assets/langs/ru-RU.xaml")
                });
                currentLang = "ru";
            }
        }

        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            ChangeWHPHorizontalOrintationAnimation();
        }


        private void ChangeWHPHorizontalOrintationAnimation()
        {
            //var opacityAdditionalFuncsHideAnimation = new DoubleAnimation()
            //{
            //    Duration = TimeSpan.FromSeconds(0.35 / 2),
            //    To = 0
            //};

            //var opacityHideAnimation = new DoubleAnimation()
            //{
            //    Duration = TimeSpan.FromSeconds(0.35 / 2),
            //    To = 0
            //};

            //var opacityShowAnimation = new DoubleAnimation()
            //{
            //    Duration = TimeSpan.FromSeconds(0.35 / 2),
            //    To = 1
            //};

            //// перемещаем кнопки и панель в нужную сторону.
            //opacityHideAnimation.Completed += (object sender, EventArgs e) =>
            //{
            //    ChangeWHPHorizontalOrintation();
            //    WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityShowAnimation);
            //    AddtionalFuncs.BeginAnimation(OpacityProperty, opacityShowAnimation);
            //};

            //// скрываем 
            //WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityHideAnimation);
            //AddtionalFuncs.BeginAnimation(OpacityProperty, opacityAdditionalFuncsHideAnimation);
        }

        private void ChangeWHPHorizontalOrintation()
        {
            //if (WindowHeaderPanelButtonsGrid.HorizontalAlignment == HorizontalAlignment.Left)
            //{
            //    WindowHeaderPanelButtons.RenderTransform = new RotateTransform(180);
            //    WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Right;

            //    AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Left;

            //    Grid.SetColumn(DebugPanel, 0);
            //    Grid.SetColumn(WindowHeaderPanelButtons, 1);

            //    RuntimeApp.HeaderState = HeaderState.Right;
            //}
            //else
            //{
            //    WindowHeaderPanelButtons.RenderTransform = new RotateTransform(360);
            //    WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Left;

            //    AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Right;

            //    Grid.SetColumn(DebugPanel, 1);
            //    Grid.SetColumn(WindowHeaderPanelButtons, 0);

            //    RuntimeApp.HeaderState = HeaderState.Left;
            //}
        }

        private void ScaleFit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Scalling();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            //Runtime.DebugWrite(grid.ActualWidth.ToString() + "x" + grid.ActualHeight.ToString());
        }
    }
}
