using Lexplosion.Logic.Management.Instances;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.Common.Views.Pages.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        private static double _scrollValue = 0;

        public LibraryView()
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
            InstanceClient.Created += OnCreatedInstance;
        }

        private void OnCreatedInstance()
        {
            Lexplosion.Common.Extension.ScrollViewer.ScroollToPosAnimated(
                ContainerPage_ScrollViewer,
                Lexplosion.Common.Extension.ScrollViewer.GetScrollBar(ContainerPage_ScrollViewer).Maximum
            );
        }

        private void LibraryItemsControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double animationTime = 0.3;

            var viewer = (ScrollViewer)sender;
            _scrollValue = viewer.VerticalOffset;

            if (viewer.VerticalOffset >= 48)
            {
                if (UpButton.Visibility != Visibility.Visible)
                {
                    UpButton.Visibility = Visibility.Visible;

                    DoubleAnimation doubleAnimation = new DoubleAnimation()
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseIn
                        }
                    };

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = new Thickness(0, 10, 0, -50),
                        To = new Thickness(0, 10, 0, 0),
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseIn
                        }
                    };

                    UpButton.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
                    UpButton.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
                }
            }
            else
            {
                if (UpButton.Visibility == Visibility.Visible)
                {
                    DoubleAnimation doubleAnimation = new DoubleAnimation()
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseOut
                        }
                    };

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = new Thickness(0, 10, 0, 0),
                        To = new Thickness(0, 10, 0, -50),
                        Duration = TimeSpan.FromSeconds(animationTime),
                        EasingFunction = new SineEase()
                        {
                            EasingMode = EasingMode.EaseOut
                        }
                    };

                    thicknessAnimation.Completed += delegate (object sender, EventArgs e)
                    {
                        UpButton.Visibility = Visibility.Collapsed;
                    };

                    UpButton.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
                    UpButton.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
                }
            }

            try
            {
                var onScrollCommand = Lexplosion.Common.Extension.ScrollViewer.GetOnScrollCommand(viewer);
                onScrollCommand.Execute(null);
            }
            catch
            {
                Runtime.DebugWrite("tes12");
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Lexplosion.Common.Extension.ScrollViewer.ScroollToPosAnimated(
                ContainerPage_ScrollViewer,
                Lexplosion.Common.Extension.ScrollViewer.GetScrollBar(ContainerPage_ScrollViewer).Minimum
            );
        }

        private void CloseGroupMenu_Click(object sender, RoutedEventArgs e)
        {
            //GroupsMenu.Visibility = Visibility.Visible;
            //DoubleAnimation doubleAnimation = new DoubleAnimation()
            //{
            //    From = GroupsMenu.Width,
            //    To = 0.0,
            //    Duration = TimeSpan.FromSeconds(0.4)
            //};
            //DoubleAnimation doubleAnimation1 = new DoubleAnimation()
            //{
            //    From = GroupsMenu.Opacity,
            //    To = 0.0,
            //    Duration = TimeSpan.FromSeconds(0.4)
            //};
            //GroupsMenu.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation);
            //GroupsMenu.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation1);
            //doubleAnimation1.Completed += (object sender, EventArgs e) => {
            //    GroupsMenu.Visibility = Visibility.Collapsed;
            //};
        }

        private void FloatingMultiButton_Click(object sender, RoutedEventArgs e)
        {


            //if (FloatingMultiButtonContent.Visibility == Visibility.Visible)
            //{
            //    var opacityAnimation = new DoubleAnimation()
            //    {
            //        From = FloatingMultiButtonContent.Opacity,
            //        To = 0.0,
            //        Duration = TimeSpan.FromSeconds(0.4)
            //    };
            //    opacityAnimation.Completed += (object sender, EventArgs e) =>
            //    {
            //        FloatingMultiButtonContent.Visibility = Visibility.Collapsed;
            //    };
            //    FloatingMultiButtonContent.BeginAnimation(FrameworkElement.OpacityProperty, opacityAnimation);

            //    var rotateAnimation = new DoubleAnimation()
            //    {
            //        From = -45,
            //        To = 0,
            //        Duration = TimeSpan.FromSeconds(0.2)
            //    };

            //    Storyboard sb = new Storyboard();
            //    Storyboard.SetTargetName(rotateAnimation, "rtAngel");
            //    Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(RotateTransform.AngleProperty));

            //    sb.Children.Add(rotateAnimation);
            //    sb.Begin(FloatingMultiButton);
            //}
            //else
            //{
            //    FloatingMultiButtonContent.Visibility = Visibility.Visible;

            //    var rotateAnimation = new DoubleAnimation()
            //    {
            //        From = 0,
            //        To = -45,
            //        Duration = TimeSpan.FromSeconds(0.2)
            //    };

            //    Storyboard sb = new Storyboard();
            //    Storyboard.SetTargetName(rotateAnimation, "rtAngel");
            //    Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(RotateTransform.AngleProperty));

            //    sb.Children.Add(rotateAnimation);
            //    sb.Begin(FloatingMultiButton);


            //    var opacityAnimation = new DoubleAnimation()
            //    {
            //        From = FloatingMultiButtonContent.Opacity,
            //        To = 1,
            //        Duration = TimeSpan.FromSeconds(0.4)
            //    };

            //    FloatingMultiButtonContent.BeginAnimation(FrameworkElement.OpacityProperty, opacityAnimation);
            //}
        }

        private void GroupsManagerOpen_Click(object sender, RoutedEventArgs e)
        {
            //if (GroupsMenu.Width == 0)
            //{
            //    GroupsMenu.Visibility = Visibility.Visible;
            //    GroupsMenu.Opacity = 0;
            //    DoubleAnimation doubleAnimation = new DoubleAnimation()
            //    {
            //        From = GroupsMenu.Width,
            //        To = 200,
            //        Duration = TimeSpan.FromSeconds(0.4)
            //    };
            //    DoubleAnimation doubleAnimation1 = new DoubleAnimation()
            //    {
            //        From = GroupsMenu.Opacity,
            //        To = 1,
            //        Duration = TimeSpan.FromSeconds(0.4)
            //    };
            //    GroupsMenu.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation);
            //    GroupsMenu.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation1);
            //}
            //else 
            //{
            //    CloseGroupMenu_Click(sender, e);
            //}
        }
    }
}
