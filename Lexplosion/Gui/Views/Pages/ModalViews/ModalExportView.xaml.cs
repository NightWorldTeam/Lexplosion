using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Pages.ModalViews
{
    /// <summary>
    /// Логика взаимодействия для ExportModalView.xaml
    /// </summary>
    public partial class ModalExportView : UserControl
    {
        public ModalExportView()
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
