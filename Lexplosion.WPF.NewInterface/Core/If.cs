using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Core
{
    public class ConditionCollection : Collection<ICondition>
    {

    }

    public interface ICondition
    {
        public object LeftOperander { get; set; }
        public object RightOperander { get; set; }
        public string Operator { get; set; }

        public bool Result { get; }

        bool Compile();
    }

    public sealed class Condition : DependencyObject, ICondition
    {
        public static readonly DependencyProperty LeftOperanderProperty
            = DependencyProperty.Register(nameof(LeftOperander), typeof(object), typeof(Condition),
                new FrameworkPropertyMetadata(null, propertyChangedCallback: OnLeftOperanderChanged));

        private static void OnLeftOperanderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as Condition;
            _this.Result = _this.Compile();
        }

        public static readonly DependencyProperty RightOperanderProperty
            = DependencyProperty.Register(nameof(RightOperander), typeof(object), typeof(Condition),
            new FrameworkPropertyMetadata(null));

        public object LeftOperander 
        {
            get => GetValue(LeftOperanderProperty); 
            set => SetValue(LeftOperanderProperty, value); 
        }

        public object RightOperander
        {
            get => GetValue(RightOperanderProperty);
            set => SetValue(RightOperanderProperty, value);
        }

        public string Operator { get; set; } = "Equal";

        public bool Result { get; private set; }

        public bool Compile()
        {
            Runtime.DebugWrite(LeftOperander?.GetType());
            Runtime.DebugWrite(LeftOperander + " " + RightOperander);

            if (LeftOperander == null)
                return false;
            if (LeftOperander is bool && RightOperander is string right) 
            {
                if (string.Equals(right, "True", StringComparison.OrdinalIgnoreCase))
                {
                    RightOperander = true;
                }
                else if (string.Equals(right, "False", StringComparison.OrdinalIgnoreCase))
                {
                    RightOperander = false;
                }
            }

            return Operator switch
            {
                "Equal" or "==" => LeftOperander.Equals(RightOperander),
                "NotEqual" or "!=" => !LeftOperander.Equals(RightOperander),
                "LessThan" or "<" => CompareObjects() == -1,
                "LessThanOrEqual" or "<=" => CompareObjects() <= -1,
                "GreaterThan" or ">" => CompareObjects() == 1,
                "GreaterThanOrEqual" or ">=" => CompareObjects() >= 0,
                "Not" or "!" => Not(),
                _ => false
            };
        }

        private bool Not()
        {
            if (LeftOperander is bool value)
            {
                return !value;
            }

            return false;
        }

        private int CompareObjects()
        {
            if (LeftOperander is IComparable compr1)
            {
                return compr1.CompareTo(RightOperander);
            }

            throw new ArgumentException();
        }
    }

    public class If : UserControl
    {

        #region Dependency Properties


        public static readonly DependencyProperty ConditionProperty
            = DependencyProperty.Register(nameof(Condition), typeof(bool), typeof(If),
                new FrameworkPropertyMetadata(false, propertyChangedCallback: OnConditionPropertyChanged));

        public static readonly DependencyProperty ConditionsProperty
            = DependencyProperty.RegisterAttached(nameof(Conditions), typeof(ConditionCollection), typeof(If),
            new FrameworkPropertyMetadata(new ConditionCollection(), propertyChangedCallback: OnConditionsPropertyChanged));

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

        public ConditionCollection Conditions
        {
            get => (ConditionCollection)GetValue(ConditionsProperty);
            set => SetValue(ConditionsProperty, value);
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


        private static void OnContentDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (If)d;
            _this.UpdateContent();
        }

        private static void OnConditionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (If)d;
            _this.UpdateContent();
        }

        private static void OnConditionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as If;
            bool result = _this.Conditions[0].Compile();

            Runtime.DebugWrite(_this.Conditions.Count);
            foreach (var condition in _this.Conditions.Skip(0))
            {
                result = result && condition.Compile();
            }

            _this.Condition = result;
            _this.UpdateContent();
        }
    }
}
