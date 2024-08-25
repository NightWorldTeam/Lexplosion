using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class Empty : ContentControl
    {
        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(nameof(Description), typeof(string), typeof(Empty),
            new FrameworkPropertyMetadata(defaultValue: "No data"));

        public string Description 
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }


        #region Constructors


        static Empty()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Empty), new FrameworkPropertyMetadata(typeof(Empty)));
        }


        #endregion Constructors
    }
}
