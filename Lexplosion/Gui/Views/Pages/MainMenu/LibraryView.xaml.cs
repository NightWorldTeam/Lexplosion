using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Pages.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView()
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

        private void LibraryItemsControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double animationTime = 0.3;

            var viewer = (ScrollViewer)sender;
            if (viewer.VerticalOffset >= 48)
            {
                if (UpButton.Visibility != Visibility.Visible)
                {
                    UpButton.Visibility = Visibility.Visible;

                    DoubleAnimation doubleAnimation = new DoubleAnimation()
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(animationTime)
                    };

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = new Thickness(0, 10, 0, -40),
                        To = new Thickness(0, 10, 0, 0),
                        Duration = TimeSpan.FromSeconds(animationTime)
                    };

                    UpButton.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimation);
                    UpButton.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
                }
            }
            else
            {
                Runtime.DebugWrite(UpButton.Visibility.ToString());

                if (UpButton.Visibility == Visibility.Visible)
                {
                    DoubleAnimation doubleAnimation = new DoubleAnimation()
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(animationTime)
                    };

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = new Thickness(0, 10, 0, 0),
                        To = new Thickness(0, 10, 0, -40),
                        Duration = TimeSpan.FromSeconds(animationTime)
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
                Runtime.DebugWrite("tes12");
            }
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
