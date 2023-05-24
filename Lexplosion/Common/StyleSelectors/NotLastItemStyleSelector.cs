using System.Windows.Controls;
using System.Windows;

namespace Lexplosion.Common.StyleSelectors
{
    public class NotLastItemStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);
            var count = itemsControl.ItemContainerGenerator.Items.Count;

            if (index != count)
            {
                return (Style)itemsControl.FindResource("NotLastItemStyle");
            }

            return base.SelectStyle(item, container);
        }
    }
}
