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
