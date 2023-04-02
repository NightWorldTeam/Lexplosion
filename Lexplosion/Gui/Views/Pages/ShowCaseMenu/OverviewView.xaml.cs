using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Pages.ShowCaseMenu
{
    /// <summary>
    /// Логика взаимодействия для OverviewView.xaml
    /// </summary>
    public partial class OverviewView : UserControl
    {
        public OverviewView()
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
