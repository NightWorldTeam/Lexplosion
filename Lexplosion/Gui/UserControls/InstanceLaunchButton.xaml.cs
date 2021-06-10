using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for InstanceLaunchButton.xaml
    /// </summary>
    public partial class InstanceLaunchButton : UserControl
    {
        Color MouseEnterColor = System.Windows.Media.Color.FromArgb(255, 44, 153, 194);
        Color MouseLeaveColor = System.Windows.Media.Color.FromArgb(255, 21, 23, 25);
        private Geometry GeometryDownloadIcon = Geometry.Parse("M 50.625 0 h 18.75 A 5.612 5.612 0 0 1 75 5.625 V 45 H 95.555 a 4.679 4.679 0 0 1 3.3 7.992 L 63.211 88.664 a 4.541 4.541 0 0 1 -6.4 0 l -35.7 -35.672 A 4.679 4.679 0 0 1 24.422 45 H 45 V 5.625 A 5.612 5.612 0 0 1 50.625 0 Z M 120 88.125 v 26.25 A 5.612 5.612 0 0 1 114.375 120 H 5.625 A 5.612 5.612 0 0 1 0 114.375 V 88.125 A 5.612 5.612 0 0 1 5.625 82.5 H 40.008 L 51.492 93.984 a 12.01 12.01 0 0 0 17.016 0 L 79.992 82.5 h 34.383 A 5.612 5.612 0 0 1 120 88.125 Z M 90.938 108.75 a 4.688 4.688 0 1 0 -4.687 4.688 A 4.7 4.7 0 0 0 90.938 108.75 Z m 15 0 a 4.688 4.688 0 1 0 -4.687 4.688 A 4.7 4.7 0 0 0 105.938 108.75 Z");
        private Geometry GeometryPlayIcon = Geometry.Parse("M0 0V28L22 14L0 0Z");
        private bool IsInstalled = false;

        public InstanceLaunchButton()
        {
            InitializeComponent();
            background.Color = MouseLeaveColor;
            if (IsInstalled) InstanceLaunchPath.Data = GeometryPlayIcon;
            else InstanceLaunchPath.Data = GeometryDownloadIcon;
        }

        private void DownloadLaunchButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = MouseLeaveColor,
                To = MouseEnterColor,
                Duration = TimeSpan.FromSeconds(0.2),
            };
            background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void DownloadLaunchButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = MouseEnterColor,
                To = MouseLeaveColor,
                Duration = TimeSpan.FromSeconds(0.2),
            };
            background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void DownloadLaunchButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsInstalled)
            {
                IsInstalled = false;
                InstanceLaunchPath.Data = GeometryDownloadIcon;
            }
            else 
            {
                IsInstalled = true;
                InstanceLaunchPath.Data = GeometryPlayIcon;
            }
        }
    }
}
