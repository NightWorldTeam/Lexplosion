using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class LoadingBoard : ContentControl
    {
        #region Properties and Events

        public static readonly DependencyProperty IsLoadingFinishedProperty =
            DependencyProperty.Register("IsLoadingFinished", typeof(bool), typeof(LoadingBoard), new PropertyMetadata(false));

        public bool IsLoadingFinished
        {
            get => (bool)GetValue(IsLoadingFinishedProperty);
            set => SetValue(IsLoadingFinishedProperty, value);
        }

        #endregion

        static LoadingBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingBoard), new FrameworkPropertyMetadata(typeof(LoadingBoard)));
        }
    }
}
