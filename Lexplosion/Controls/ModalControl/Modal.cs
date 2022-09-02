using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Controls
{
    public class Modal : ContentControl
    {
        public static readonly DependencyProperty IsOpenProperty
            = DependencyProperty.Register(
                "IsOpen",
                typeof(bool),
                typeof(Modal),
                new PropertyMetadata(false, OnIsOpenChanged)
                );

        public static readonly DependencyProperty BackgroundOpacityProperty
            = DependencyProperty.Register(
                "BackgroundOpacity",
                typeof(double),
                typeof(Modal),
                new FrameworkPropertyMetadata(0.7)
                );

        public static readonly DependencyProperty WindowWidthProperty
            = DependencyProperty.Register(
                "WindowWidth",
                typeof(double),
                typeof(Modal),
                new FrameworkPropertyMetadata(0.7)
                );

        public static readonly DependencyProperty WindowHeightProperty
            = DependencyProperty.Register(
                "WindowHeight",
                typeof(double),
                typeof(Modal),
                new FrameworkPropertyMetadata(0.7)
                );



        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
        }

        public double WindowWidth 
        {
            get => (double)GetValue(WindowWidthProperty);
            set => SetValue(WindowWidthProperty, value);
        }

        public double WindowHeight 
        {
            get => (double)GetValue(WindowHeightProperty);
            set => SetValue(WindowWidthProperty, value);
        }

        static Modal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(typeof(Modal)));
        }

        private void ShowModalAnimation() 
        {
            this.Opacity = 0.0;
            this.Visibility = Visibility.Visible;

            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            this.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }
        
        private void CloseModalAnimation() 
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            doubleAnimation.Completed += (object sender, EventArgs e) => 
            {
                this.Visibility = Visibility.Collapsed;
            };

            this.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dObj = (Modal)d;
            
                if ((bool)e.NewValue == true) 
                {
                    dObj.ShowModalAnimation();
                }
                else 
                {
                    dObj.CloseModalAnimation();
                }
        }
    }
}
