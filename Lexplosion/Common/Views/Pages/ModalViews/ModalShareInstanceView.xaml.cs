using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Common.Views.Pages.ModalViews
{
    /// <summary>
    /// Логика взаимодействия для ModalShareInstanceView.xaml
    /// </summary>
    public partial class ModalShareInstanceView : UserControl
    {
        public ModalShareInstanceView()
        {
            InitializeComponent();
            this.Opacity = 0.0;
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
