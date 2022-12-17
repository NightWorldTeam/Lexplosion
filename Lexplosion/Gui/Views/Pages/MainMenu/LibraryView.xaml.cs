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
            var viewer = (ScrollViewer)sender;
            if (viewer.VerticalOffset >= 48)
            {
                UpButton.Visibility = Visibility.Visible;
            }
            else
            {
                UpButton.Visibility = Visibility.Collapsed;
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

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Lexplosion.Gui.Extension.ScrollViewer.ScroollToPosAnimated(
                ContainerPage_ScrollViewer,
                Lexplosion.Gui.Extension.ScrollViewer.GetScrollBar(ContainerPage_ScrollViewer).Minimum
            );
        }
    }
}
