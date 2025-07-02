using Lexplosion.WPF.NewInterface.Controls;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lexplosion.WPF.NewInterface.Extensions;
using System.Threading;

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

        private void Paginator_PageChanged(uint obj)
        {
            ScrollViewerExtensions.ScroollToPosAnimated(
                ContainerPage_ScrollViewer,
                ScrollViewerExtensions.GetScrollBar(ContainerPage_ScrollViewer).Minimum
            );
        }

        private void OnFilterPanelSourceClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FiltersButton.IsChecked = false;
            //PropertyChanged?.Invoke(FiltersButton, new PropertyChangedEventArgs(nameof(FiltersButton.IsChecked)));
        }

        private void ContainerPage_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

		private void border_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			ThreadPool.QueueUserWorkItem((object _) =>
			{
				Thread.Sleep(10);
				App.Current.Dispatcher.Invoke(() =>
				{
					FiltersButton.IsChecked = false;
				});
			});
		}
	}
}
