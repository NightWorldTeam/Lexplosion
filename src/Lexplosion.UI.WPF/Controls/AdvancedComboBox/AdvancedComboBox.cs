using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Controls
{
    public class AdvancedComboBox : ComboBox
    {
        public static readonly DependencyProperty IsLoadingProperty
            = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(AdvancedComboBox),
            new FrameworkPropertyMetadata());


        public bool IsLoading 
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }


        #region Constructors


        static AdvancedComboBox() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedComboBox), new FrameworkPropertyMetadata(typeof(AdvancedComboBox)));
        }

        public AdvancedComboBox()
        {
        }


        #endregion Constructors
    }
}
