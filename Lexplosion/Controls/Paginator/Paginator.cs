using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    [TemplatePart()]
    public class Paginator : Control
    {
        /// 2 кнопки и textbox/

        private const string PART_BACK_BUTTON = "PART_Back_Button";
        private const string PART_FORWARD_BUTTON = "PART_Forward_Button";

        private const uint _minIndexValue = 1;
        //public event PageChangedCallback PageChanged;

        public static readonly DependencyProperty CurrentIndexProperty
            = DependencyProperty.Register("CurrentIndex", typeof(uint), typeof(Paginator), new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty MaxIndexProperty
            = DependencyProperty.Register("MaxIndex", typeof(uint), typeof(Paginator), new FrameworkPropertyMetadata(uint.MaxValue));

        public static readonly DependencyProperty IndexChangedActionProperty
            = DependencyProperty.Register("IndexChangedAction", typeof(Action), typeof(Paginator));

        public uint CurrentIndex
        {
            get => (uint)GetValue(CurrentIndexProperty);
            set => SetValue(CurrentIndexProperty, value);
        }

        public uint MaxIndex
        {
            get => (uint)GetValue(MaxIndexProperty);
            set => SetValue(MaxIndexProperty, value);
        }

        public Action IndexChangedAction
        {
            get => (Action)GetValue(IndexChangedActionProperty);
            set => SetValue(IndexChangedActionProperty, value);
        }

        static Paginator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Paginator), new FrameworkPropertyMetadata(typeof(Paginator)));
        }

        ///// <summary>
        ///// Deincrease index by one.
        ///// </summary>
        //public static readonly RoutedCommand IndexDownCommand = new RoutedCommand("IndexDown", typeof(Paginator));
        ///// <summary>
        ///// Increase index by one.
        ///// </summary>
        //public static readonly RoutedCommand IndexUpCommand = new RoutedCommand("IndexUp", typeof(Paginator));
        ///// <summary>
        ///// Go to current index.
        ///// </summary>
        //public static readonly RoutedCommand GoToCurrentIndexCommand = new RoutedCommand("GoToCurrentIndex", typeof(Paginator));


        private static void OnIndexChanged() { }
    }
}
