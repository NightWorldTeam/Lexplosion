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
    /// Interaction logic for SwitchButton.xaml
    /// </summary>
    public partial class SwitchButton : UserControl
    {
        Thickness Off = new Thickness(3, 3, 32, 3);

        SolidColorBrush OffColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        SolidColorBrush OnColor = new SolidColorBrush(Color.FromRgb(30, 175, 221));

        private bool Toggled = false;
        
        public SwitchButton()
        {
            InitializeComponent();
            Back.Fill = OffColor;
            Toggled = false;
            Dot.Margin = Off;
        }

        private bool Toggle1 { get => Toggled; set => Toggled = value; }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            SwitchAnimation(sender);
        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            SwitchAnimation(sender);
        }

        private void SwitchAnimation(object sender) 
        {
            if (!Toggled)
            {
                if (Dot.Margin == new Thickness(3, 3, 32, 3))
                {
                    Back.Fill = OnColor;
                    Toggled = true;

                    ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                    {
                        From = Dot.Margin,
                        To = new Thickness(32, 3, 3, 3),
                        Duration = TimeSpan.FromSeconds(0.3),
                    };

                    Dot.BeginAnimation(Canvas.MarginProperty, thicknessAnimation);
                }
            }
            else
            {
                if (Dot.Margin == new Thickness(32, 3, 3, 3))
                {
                    Back.Fill = OffColor;
                    Toggled = false;

                    ThicknessAnimation animation = new ThicknessAnimation()
                    {
                        From = Dot.Margin,
                        To = new Thickness(3, 3, 32, 3),
                        Duration = TimeSpan.FromSeconds(0.3)
                    };

                    Dot.BeginAnimation(Canvas.MarginProperty, animation);
                }
            }
        }
    }
}
