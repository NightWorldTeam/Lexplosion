using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Core
{
    public class If : UserControl
    {

        #region Dependency Properties


        public static readonly DependencyProperty ConditionProperty
            = DependencyProperty.Register(nameof(Condition), typeof(bool), typeof(If), 
                new FrameworkPropertyMetadata(false, propertyChangedCallback: OnContentDependencyPropertyChanged));

        public static readonly DependencyProperty TrueProperty
            = DependencyProperty.Register(nameof(True), typeof(object), typeof(If),
                new FrameworkPropertyMetadata(null, propertyChangedCallback: OnContentDependencyPropertyChanged));
        public static readonly DependencyProperty FalseProperty
            = DependencyProperty.Register(nameof(False), typeof(object), typeof(If), 
                new FrameworkPropertyMetadata(null, propertyChangedCallback: OnContentDependencyPropertyChanged));


        #endregion Dependency Properties


        #region Properties


        public bool Condition 
        {
            get => (bool)GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        public object True 
        {
            get => (object)GetValue(TrueProperty);
            set => SetValue(TrueProperty, value);
        }

        public object False
        {
            get => (object)GetValue(FalseProperty);
            set => SetValue(FalseProperty, value);
        }


        #endregion Properties


        private static void OnContentDependencyPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e) 
        {
            var _if = (If)dp;

            _if.UpdateContent();
        }

        private void UpdateContent()
        {
            if (Condition)
            {
                Content = True;
            }
            else 
            {
                Content = False;
            }
        }
    }
}
