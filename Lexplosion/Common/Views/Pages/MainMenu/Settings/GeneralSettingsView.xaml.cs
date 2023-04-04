using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Common.Views.Pages.MainMenu.Settings
{
    /// <summary>
    /// Логика взаимодействия для GeneralSettingsView.xaml
    /// </summary>
    public partial class GeneralSettingsView : UserControl
    {
        public GeneralSettingsView()
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
