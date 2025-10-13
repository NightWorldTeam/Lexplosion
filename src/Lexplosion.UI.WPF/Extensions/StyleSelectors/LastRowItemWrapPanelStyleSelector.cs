using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Extensions
{
    public class LastRowItemWrapPanelStyleSelector : StyleSelector
    {
        public Style DefaultStyle { get; set; }
        public Style LastRowElementStyle { get; set; }
        public double ElementWidth { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

            if ((itemsControl.ActualWidth / ElementWidth) % index == 0)
                return LastRowElementStyle;

            return DefaultStyle;
        }
    }
}
