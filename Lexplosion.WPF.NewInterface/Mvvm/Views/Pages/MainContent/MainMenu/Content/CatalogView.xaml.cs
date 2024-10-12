using Lexplosion.WPF.NewInterface.Controls;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lexplosion.WPF.NewInterface.Extensions;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для CatalogView.xaml
    /// </summary>
    public partial class CatalogView : UserControl
    {
        private static double _scrollValue = 0;

        public CatalogView()
        {
            InitializeComponent();
            ContainerPage_ScrollViewer.ScrollToVerticalOffset(_scrollValue);
            this.Opacity = 0.0;
            this.Visibility = Visibility.Visible;
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            this.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        //private void ContainerPage_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    const double animationTime = 0.3;

        //    var viewer = (ScrollViewer)sender;
        //    _scrollValue = viewer.VerticalOffset;

        //    if (viewer.VerticalOffset >= 96)
        //    {
        //        if (UpButton.Visibility != Visibility.Visible)
        //        {
        //            UpButton.Visibility = Visibility.Visible;

        //            DoubleAnimation doubleAnimation = new DoubleAnimation()
        //            {
        //                From = 0.0,
        //                To = 1.0,
        //                Duration = TimeSpan.FromSeconds(animationTime),
        //                EasingFunction = new SineEase()
        //                {
        //                    EasingMode = EasingMode.EaseIn
        //                }
        //            };

        //            ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
        //            {
        //                From = new Thickness(0, 0, 20, -20),
        //                To = new Thickness(0, 0, 20, 20),
        //                Duration = TimeSpan.FromSeconds(animationTime),
        //                EasingFunction = new SineEase()
        //                {
        //                    EasingMode = EasingMode.EaseIn
        //                }
        //            };

        //            UpButton.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
        //            UpButton.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        //        }
        //    }
        //    else
        //    {
        //        if (UpButton.Visibility == Visibility.Visible)
        //        {
        //            DoubleAnimation doubleAnimation = new DoubleAnimation()
        //            {
        //                From = 1.0,
        //                To = 0.0,
        //                Duration = TimeSpan.FromSeconds(animationTime),
        //                EasingFunction = new SineEase()
        //                {
        //                    EasingMode = EasingMode.EaseOut
        //                }
        //            };

        //            ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
        //            {
        //                From = new Thickness(0, 0, 20, 20),
        //                To = new Thickness(0, 0, 20, -20),
        //                Duration = TimeSpan.FromSeconds(animationTime),
        //                EasingFunction = new SineEase()
        //                {
        //                    EasingMode = EasingMode.EaseOut
        //                }
        //            };

        //            thicknessAnimation.Completed += delegate (object sender, EventArgs e)
        //            {
        //                UpButton.Visibility = Visibility.Collapsed;
        //            };

        //            UpButton.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
        //            UpButton.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        //        }
        //    }

        //    try
        //    {
        //        var onScrollCommand = ScrollViewerExtensions.GetOnScrollCommand(viewer);
        //        onScrollCommand.Execute(null);
        //    }
        //    catch
        //    {

        //    }
        //}

        //private void UpButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ScrollViewerExtensions.ScroollToPosAnimated(
        //        ContainerPage_ScrollViewer,
        //        ScrollViewerExtensions.GetScrollBar(ContainerPage_ScrollViewer).Minimum
        //    );
        //}

        private void InstanceForm_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var if_ = (InstanceForm)sender;
            Runtime.DebugWrite(if_.ActualWidth);
        }

        private void Paginator_PageChanged(uint obj)
        {
            ScrollViewerExtensions.ScroollToPosAnimated(
                ContainerPage_ScrollViewer,
                ScrollViewerExtensions.GetScrollBar(ContainerPage_ScrollViewer).Minimum
            );
        }
    }
}
