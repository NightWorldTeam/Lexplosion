using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Common.Views.Pages.FactoryMenu
{
    /// <summary>
    /// Логика взаимодействия для ImportView.xaml
    /// </summary>
    public partial class ImportView : UserControl
    {
        public ImportView()
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
    }
}
