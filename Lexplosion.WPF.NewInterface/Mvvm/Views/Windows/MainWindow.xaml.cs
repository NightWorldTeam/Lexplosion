﻿using Lexplosion.WPF.NewInterface.Tools;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string currentLang = "ru";
        private bool _isScalled = false;

        private readonly DoubleAnimation _defaultChangeThemeAnimation = new DoubleAnimation()
        {
            From = 1700,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.35 * 3),
            EasingFunction = new SineEase
            { EasingMode = EasingMode.EaseInOut }
        };

        public MainWindow()
        {
            InitializeComponent();

            _defaultChangeThemeAnimation.Completed += (sender, e) => 
            {
                PaintArea.Visibility = Visibility.Hidden;
            };

            RuntimeApp.AppColorThemeService.BeforeAnimations.Add(() => 
            {
                PaintArea.Opacity = 1;
                PaintArea.Visibility = Visibility.Visible;
                PaintArea.Background = CreateBrushFromVisual(this);
            });

            RuntimeApp.AppColorThemeService.Animations.Add(() => 
            {
                PaintArea.BeginAnimation(OpacityProperty, _defaultChangeThemeAnimation);
                CircleReveal.BeginAnimation(EllipseGeometry.RadiusXProperty, _defaultChangeThemeAnimation);
                CircleReveal.BeginAnimation(EllipseGeometry.RadiusYProperty, _defaultChangeThemeAnimation);
            });

            MouseDown += delegate { try { DragMove(); } catch { } };
        }
        private void Scalling()
        {
            double keff = 0.25;
            var yScale = keff + 1;

            if (_isScalled)
            {
                keff *= -1;
                yScale = 1;
            }

            ContainerGrid.LayoutTransform = new ScaleTransform(yScale, yScale);
            this.Width += Width * keff;
            this.Height += Height * keff;

            // Bring window center screen
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            Top = (screenHeight - Height) / 2;
            Left = (screenWidth - Width) / 2;

            _isScalled = !_isScalled;
        }


        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;

            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                Runtime.DebugWrite(i.ToString() + " " + grid.ColumnDefinitions[i].ActualWidth.ToString());
            }
        }


        #region Window State Buttons


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.ChangeColor(ColorTools.GetColorByHex("#167FFC"));
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


        private void ChangeTheme_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            border.IsEnabled = false;

            PaintArea.Opacity = 1;
            PaintArea.Visibility = Visibility.Visible;
            PaintArea.Background = CreateBrushFromVisual(this);

            DoubleAnimation dba;

            var resourceDictionaries = new List<ResourceDictionary>();

            var currentThemeName = "";

            foreach (var s in App.Current.Resources.MergedDictionaries)
            {
                if (s.Source.ToString().Contains("ColorTheme"))
                {
                    currentThemeName = s.Source.ToString();
                    resourceDictionaries.Add(s);
                }
            }

            foreach (var s in resourceDictionaries)
            {
                App.Current.Resources.MergedDictionaries.Remove(s);
            }

            if (currentThemeName.Contains("Light"))
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Resources/Themes/DarkColorTheme.xaml")
                });
            }
            else
            {
                App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Resources/Themes/LightColorTheme.xaml")
                });
            }

            dba = new DoubleAnimation()
            {
                From = 1700,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.35 * 1.5),
                EasingFunction = new SineEase
                { EasingMode = EasingMode.EaseInOut }
            };

            dba.Completed += (s, e) => Dba_Completed(s, e, border);
            //PaintArea.BeginAnimation(OpacityProperty, dba);
            CircleReveal.BeginAnimation(EllipseGeometry.RadiusXProperty, dba);
            CircleReveal.BeginAnimation(EllipseGeometry.RadiusYProperty, dba);
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
            var opacityAdditionalFuncsHideAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 0
            };

            var opacityHideAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 0
            };

            var opacityShowAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 1
            };

            // перемещаем кнопки и панель в нужную сторону.
            opacityHideAnimation.Completed += (object sender, EventArgs e) =>
            {
                ChangeWHPHorizontalOrintation();
                WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityShowAnimation);
                AddtionalFuncs.BeginAnimation(OpacityProperty, opacityShowAnimation);
            };

            // скрываем 
            WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityHideAnimation);
            AddtionalFuncs.BeginAnimation(OpacityProperty, opacityAdditionalFuncsHideAnimation);
        }

        private void ChangeWHPHorizontalOrintation()
        {
            if (WindowHeaderPanelButtonsGrid.HorizontalAlignment == HorizontalAlignment.Left)
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(180);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Right;

                AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Left;

                Grid.SetColumn(DebugPanel, 0);
                Grid.SetColumn(WindowHeaderPanelButtons, 1);

                RuntimeApp.HeaderState = HeaderState.Right;
            }
            else
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(360);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Left;

                AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Right;

                Grid.SetColumn(DebugPanel, 1);
                Grid.SetColumn(WindowHeaderPanelButtons, 0);

                RuntimeApp.HeaderState = HeaderState.Left;
            }
        }

        private void ScaleFit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Scalling();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            Runtime.DebugWrite(grid.ActualWidth.ToString() + "x" + grid.ActualHeight.ToString());
        }
    }
}
