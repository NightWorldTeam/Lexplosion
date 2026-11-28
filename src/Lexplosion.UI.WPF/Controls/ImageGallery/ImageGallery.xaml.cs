using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Controls
{
    public partial class ImageGallery : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ImageGallery));
        public static readonly DependencyProperty OpenImageCommandProperty = DependencyProperty.Register(nameof(OpenImageCommand), typeof(ICommand), typeof(ImageGallery));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand OpenImageCommand
        {
            get => (ICommand)GetValue(OpenImageCommandProperty);
            set => SetValue(OpenImageCommandProperty, value);
        }

        public ImageGallery()
        {
            InitializeComponent();
        }
    }
}
