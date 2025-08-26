using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.WindowComponents.Header
{
    /// <summary>
    /// Interaction logic for WindowHeader.xaml
    /// </summary>
    public partial class WindowHeaderContainer : UserControl
    {
        public static readonly DependencyProperty HeaderTypeProperty
            = DependencyProperty.Register(nameof(HeaderType), typeof(string), typeof(WindowHeaderContainer),
                new FrameworkPropertyMetadata(defaultValue: string.Empty));

        public string HeaderType
        {
            get => (string)GetValue(HeaderTypeProperty);
            set => SetValue(HeaderTypeProperty, value);
        }

        public WindowHeaderContainer()
        {
            InitializeComponent();
        }
    }
}
