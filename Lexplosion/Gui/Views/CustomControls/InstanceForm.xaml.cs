using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Lexplosion.Gui.Views.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для InstanceForm.xaml
    /// </summary>
    public partial class InstanceForm : UserControl
    {
        public InstanceForm()
        {
            InitializeComponent();
        }

        private const double AnimationTime = 200;

        private void InstanceLogo_MouseEnter(object sender, MouseEventArgs e) 
        {
            InstanceLogo_Background.Effect = new BlurEffect();
            InstanceLogo_Background.BeginAnimation(Border.OpacityProperty, new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(AnimationTime)));
            InstanceLogo_Background.Effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(0, 5, TimeSpan.FromMilliseconds(AnimationTime)));
            InstanceLogo_Text.BeginAnimation(TextBlock.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(AnimationTime)));
            InstanceLogo_Text.Visibility = Visibility.Visible;
        }

        private void InstanceLogo_MouseLeave(object sender, MouseEventArgs e)
        {
            InstanceLogo_Background.BeginAnimation(Border.OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(AnimationTime)));
            InstanceLogo_Background.Effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(5, 0, TimeSpan.FromMilliseconds(AnimationTime)));
            InstanceLogo_Text.BeginAnimation(TextBlock.OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(AnimationTime)));
            //InstanceLogo_Background.Effect = null;
            //InstanceLogo_Text.Visibility = Visibility.Collapsed;
        }
    }
}
