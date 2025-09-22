using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public class NthChildStyleSelector : StyleSelector
    {
        protected enum NthChildType
        {
            Odd,
            Even,
            N,
            IntOnly,
            NoExpression
        }

        private string _expression;
        public string Expression
        {
            get => _expression; set
            {
                _expression = value;
                OnExpressionChanged();
            }
        }

        private Style _style;
        public Style Style
        {
            get => _style; set
            {
                _style = value;
            }
        }

        private int _NValue = 0;

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

            if (IsSelected(Expression, index, out var type))
            {
                return Style;
            }

            return base.SelectStyle(item, container);
        }

        protected bool IsSelected(string expression, int itemIndex, out NthChildType type)
        {
            type = NthChildType.NoExpression;
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            if (expression == "even")
            {
                type = NthChildType.Even;
                return itemIndex % 2 == 0;
            }
            else if (expression == "odd")
            {
                type = NthChildType.Odd;
                return itemIndex % 2 != 0;
            }

            if (expression.All(char.IsDigit))
            {
                type = NthChildType.IntOnly;
                return Int32.Parse(expression) == itemIndex;
            }

            if (expression.Contains('n'))
            {
                type = NthChildType.N;
                return NExpressionHandler(expression, itemIndex);
            }

            return false;
        }

        private bool NExpressionHandler(string expression, int itemIndex)
        {
            var nIndex = expression.IndexOf('n');

            if (nIndex > 0 & char.IsDigit(expression[nIndex - 1]))
            {
                expression = expression.Replace("n", "*n");
            }

            var isPrevNIndexDigit = char.IsDigit(expression[nIndex - 1]);

            var onlyMathExpr = _expression.Replace("n", (_NValue - 1).ToString());
            var result = Convert.ToInt32(new DataTable().Compute(onlyMathExpr, null));
            return result == itemIndex;
        }


        private void OnExpressionChanged()
        {
            _NValue = 0;
        }
    }
}
