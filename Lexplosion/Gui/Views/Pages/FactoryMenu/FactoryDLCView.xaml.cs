using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Pages.FactoryMenu
{
    /// <summary>
    /// Логика взаимодействия для FactoryDLCView.xaml
    /// </summary>
    public partial class FactoryDLCView : UserControl
    {
        public FactoryDLCView()
        {
            InitializeComponent();
            this.Opacity = 0.0;
            this.Visibility = Visibility.Visible;
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            this.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        private void DlcContainer_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
