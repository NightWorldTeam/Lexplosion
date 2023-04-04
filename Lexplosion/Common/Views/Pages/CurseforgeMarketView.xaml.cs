using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Common.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для CurseforgeMarketView.xaml
    /// </summary>
    public partial class CurseforgeMarketView : UserControl
    {

        public CurseforgeMarketView()
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
            var viewer = (ScrollViewer)sender;
            try
            {
                var onScrollCommand = Lexplosion.Common.Extension.ScrollViewer.GetOnScrollCommand(viewer);
                onScrollCommand.Execute(null);
            }
            catch
            {

            }
        }
    }
}
