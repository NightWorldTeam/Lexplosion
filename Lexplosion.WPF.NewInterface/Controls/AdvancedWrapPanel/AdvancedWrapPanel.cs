using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class AdvancedWrapPanel : WrapPanel
    {
        public static readonly DependencyProperty LastRowElementStyleProperty
            = DependencyProperty.Register("LastRowElementStyle", typeof(Style), typeof(AdvancedWrapPanel),
            new FrameworkPropertyMetadata(propertyChangedCallback: OnLastRowElementStyleChanged));

        public static readonly DependencyProperty DefaultElementStyleProperty
            = DependencyProperty.Register("DefaultElementStyle", typeof(Style), typeof(AdvancedWrapPanel),
            new FrameworkPropertyMetadata(propertyChangedCallback: OnDefaultElementStyleChanged));

        public Style LastRowElementStyle 
        {
            get => (Style)GetValue(LastRowElementStyleProperty);
            set => SetValue(LastRowElementStyleProperty, value);
        }

        public Style DefaultElementStyle
        {
            get => (Style)GetValue(DefaultElementStyleProperty);
            set => SetValue(DefaultElementStyleProperty, value);
        }


        public AdvancedWrapPanel()
        {
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Console.WriteLine(sizeInfo.NewSize.ToString());
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void UpdateLastRowElementStyle() 
        {
            if (Children.Count == 0)
                return;

            var i = 0;
            var countInRow = (int)(ActualWidth / (Children[0] as FrameworkElement).ActualWidth);
            foreach (FrameworkElement item in Children) 
            {
                if (i % countInRow == 0)
                    item.Style = LastRowElementStyle;
                else
                    item.Style = DefaultElementStyle;
            }
        }

        private static void OnLastRowElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedWrapPanel awp) 
            {
                awp.UpdateLastRowElementStyle();
            }
        }

        private static void OnDefaultElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AdvancedWrapPanel awp)
            {
                awp.UpdateLastRowElementStyle();
            }
        }
    }
}
