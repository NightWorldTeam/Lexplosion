using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class ToastMessageContainer : ContentControl
    {
        #region DependencyProperty Register

        public static readonly DependencyProperty Messages =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ContentControl), new PropertyMetadata((IEnumerable<MessageModel>)null));



        #endregion DependencyProperty Register
    }
}
