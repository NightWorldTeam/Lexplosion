using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for SwitchButton.xaml
    /// </summary>
    public partial class SwitchButton : UserControl
    {
        Thickness Off = new Thickness(3, 3, 32, 3);

        SolidColorBrush OffColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        SolidColorBrush OnColor = new SolidColorBrush(Color.FromRgb(22, 127, 252));

        private bool Toggled = false;
        
        public SwitchButton()
        {
            InitializeComponent();
            Background.Fill = OffColor;
            Toggled = false;
            Dot.Margin = Off;
        }

        private bool Toggle1 { get => Toggled; set => Toggled = value; }

        private void Switch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            SwitchAnimation(sender);
        }

        private void SwitchAnimation(object sender) 
        {
            if (!Toggled && Dot.Margin == new Thickness(3, 3, 32, 3))
            {
                Background.Fill = OnColor;
                Toggled = true;
                
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation()
                {
                    From = Dot.Margin,
                    To = new Thickness(32, 3, 3, 3),
                    Duration = TimeSpan.FromSeconds(0.3),
                };
                Dot.BeginAnimation(Canvas.MarginProperty, thicknessAnimation);
            }
            else
            {
                Background.Fill = OffColor;
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
