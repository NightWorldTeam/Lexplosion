using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Pages.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для CatalogView.xaml
    /// </summary>
    public partial class CatalogView : UserControl
    {
        public CatalogView()
        {
            InitializeComponent();
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

        private void ContainerPage_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double animationTime = 0.3;

            var viewer = (ScrollViewer)sender;
            if (viewer.VerticalOffset >= 96)
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
                        From = new Thickness(0, 0, 20, -20),
                        To = new Thickness(0, 0, 20, 20),
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
                        From = new Thickness(0, 0, 20, 20),
                        To = new Thickness(0, 0, 20, -20),
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
                var onScrollCommand = Lexplosion.Gui.Extension.ScrollViewer.GetOnScrollCommand(viewer);
                onScrollCommand.Execute(null);
            }
            catch
            {

            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltersDropdownMenu.IsOpen = false;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Lexplosion.Gui.Extension.ScrollViewer.ScroollToPosAnimated(
                ContainerPage_ScrollViewer,
                Lexplosion.Gui.Extension.ScrollViewer.GetScrollBar(ContainerPage_ScrollViewer).Minimum
            );
        }
    }
}
