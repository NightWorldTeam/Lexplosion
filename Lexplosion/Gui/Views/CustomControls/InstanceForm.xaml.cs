using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.ViewModels;
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
            _ease = new SineEase() 
            {
                EasingMode = EasingMode.EaseOut
            };

            InitializeComponent();
        }

        private const double AnimationTime = 300;

        private IEasingFunction _ease;

        private void InstanceLogo_MouseEnter(object sender, MouseEventArgs e) 
        {
            InstanceLogo_Background.Effect = new BlurEffect();
            InstanceLogo_Background.BeginAnimation(Border.OpacityProperty, new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = _ease });
            InstanceLogo_Background.Effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(0, 5, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = _ease});
            InstanceLogo_Text.BeginAnimation(TextBlock.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = _ease });
            InstanceLogo_Text.Visibility = Visibility.Visible;
        }

        private void InstanceLogo_MouseLeave(object sender, MouseEventArgs e)
        {
            InstanceLogo_Background.BeginAnimation(Border.OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = _ease });
            InstanceLogo_Background.Effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(5, 0, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = _ease });
            InstanceLogo_Text.BeginAnimation(TextBlock.OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(AnimationTime)) { EasingFunction = _ease });
            //InstanceLogo_Background.Effect = null;
            //InstanceLogo_Text.Visibility = Visibility.Collapsed;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = (Grid)sender;

            var authorTextBoxFinalWidth = (Author.ActualWidth + 16.65);

            var columnDefin = new ColumnDefinition()
            {
                Width = new GridLength(440 - authorTextBoxFinalWidth)
            };

            Console.WriteLine(Author.ActualWidth);

            var columnDefin1 = new ColumnDefinition()
            {
                MaxWidth = authorTextBoxFinalWidth,
                Width = new GridLength(1, GridUnitType.Star)
            };

            grid.ColumnDefinitions.Add(columnDefin);
            grid.ColumnDefinitions.Add(columnDefin1);
        }
    }
}
