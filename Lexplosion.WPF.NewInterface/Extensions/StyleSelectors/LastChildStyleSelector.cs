﻿using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public class LastChildStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

            if ((index + 1) % 2 == 0)
            {
                return (Style)itemsControl.FindResource("EvenItemStyle");
            }

            return base.SelectStyle(item, container);
        }
    }
}
