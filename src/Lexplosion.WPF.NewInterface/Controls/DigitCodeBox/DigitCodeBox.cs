using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Controls
{

    [TemplatePart(Name = PART_WRAP_NAME, Type = typeof(WrapPanel))]
    public class DigitCodeBox : Control
    {
        private const string PART_WRAP_NAME = "PART_WrapPanel";

        private WrapPanel _fieldsPanel;
        private Dictionary<TextBox, String> CurrentInputFieldsValues = new Dictionary<TextBox, string>();


        #region Dependency Properties


        public static readonly DependencyProperty CodeSizeProperty
            = DependencyProperty.Register("CodeSize", typeof(int), typeof(DigitCodeBox), new FrameworkPropertyMetadata(6));

        public static readonly DependencyProperty IsFullCodeProperty
            = DependencyProperty.Register("IsFullCode", typeof(bool), typeof(DigitCodeBox), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CodeProperty
            = DependencyProperty.Register("Code", typeof(string), typeof(DigitCodeBox),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsNumericOnlyProperty
            = DependencyProperty.Register("IsNumericOnly", typeof(bool), typeof(DigitCodeBox), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty TextBoxStyleProperty
            = DependencyProperty.Register("TextBoxStyle", typeof(Style), typeof(DigitCodeBox), new PropertyMetadata());

        public int CodeSize
        {
            get => (int)GetValue(CodeSizeProperty);
            set => SetValue(CodeSizeProperty, value);
        }

        // TODO: вынести код из setter в отдельные методы.
        public bool IsFullCode
        {
            get { return (bool)GetValue(IsFullCodeProperty); }
            set
            {
                // Если код был значение для IsFullCode было изменено внутри класс элемента.
                if (IsBoxChangeIsFullCode)
                {
                    SetValue(IsFullCodeProperty, value);
                    IsBoxChangeIsFullCode = false;
                }
            }
        }

        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set
            {
                // Если код был значение для Code было изменено внутри класс элемента.
                if (IsBoxChangeCode)
                {
                    SetValue(CodeProperty, value);
                    IsBoxChangeCode = false;
                }
            }
        }

        public bool IsNumericOnly
        {
            get => (bool)GetValue(IsNumericOnlyProperty);
            set => SetValue(IsNumericOnlyProperty, value);
        }

        public Style TextBoxStyle
        {
            get => (Style)GetValue(TextBoxStyleProperty);
            set => SetValue(TextBoxStyleProperty, value);
        }


        #endregion Dependency Properties


        #region Properties


        protected bool IsBoxChangeCode { get; set; }
        protected bool IsBoxChangeIsFullCode { get; set; }


        #endregion Properties


        #region Constructors 


        static DigitCodeBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DigitCodeBox), new FrameworkPropertyMetadata(typeof(DigitCodeBox)));
        }


        #endregion Constructors


        #region Public & Protected Properties


        public override void OnApplyTemplate()
        {
            _fieldsPanel = Template.FindName(PART_WRAP_NAME, this) as WrapPanel;

            InitializeInputFields();
            base.OnApplyTemplate();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }


        #endregion Public & Protected Properties


        #region Private Methods


        private void InitializeInputFields()
        {
            for (var i = 0; i < CodeSize; i++)
            {
                var template = new TextBox()
                {
                    Style = TextBoxStyle,
                    Margin = i > 0 ? new Thickness(8, 0, 0, 0) : new Thickness(0),
                    Width = 40,
                    Height = 44,
                    MaxLength = 1,
                };

                CurrentInputFieldsValues.Add(template, "");

                template.TextChanged += (e, a) =>
                {
                    InputFieldTextChanged(e, a);
                };

                template.PreviewKeyDown += (e, a) =>
                {
                    if (a.Key == Key.Left)
                    {
                        template.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    }
                    else if (a.Key == Key.Right)
                    {
                        template.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    }
                };

                template.GotFocus += (e, a) =>
                {
                    CurrentInputFieldsValues[template] = template.Text;
                    template.Text = string.Empty;
                };

                template.LostFocus += (e, a) =>
                {
                    template.Text = CurrentInputFieldsValues[template];
                };

                template.PreviewTextInput += InputFieldValidation;

                if (_fieldsPanel == null)
                {
                    new Exception("Panel for input fields doesn't exists");
                }
                else
                {
                    _fieldsPanel.Children.Add(template);
                }
            }

            _fieldsPanel.Children[0].Focus();
        }

        private void InputFieldTextChanged(object sender, TextChangedEventArgs args, bool isBackspace = false)
        {
            var currentTextBox = (TextBox)sender;

            // Выходим из метода, если строка пустая или не является числом.
            if (string.IsNullOrEmpty(currentTextBox.Text))
            {
                return;
            }

            CurrentInputFieldsValues[currentTextBox] = currentTextBox.Text;

            OnCodeChanged();

            // Используем keyboard navigation, чтобы перемещаться между элементами внутри нашей панели.
            // после тогда как мы закончим обход всех textbox, следующий фокус будет падать на элемент с focusable = true;
            // Если был нажат backspace не переносим фокус на следующую ячейку, пока текущая не будет заполнена.
            if (!isBackspace)
            {
                currentTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            IsBoxChangeIsFullCode = true;
            IsFullCode = Code.Length == CodeSize;
        }


        private void OnCodeChanged()
        {
            IsBoxChangeCode = true;
            Code = string.Join("", CurrentInputFieldsValues.Keys.Select(textbox => textbox.Text));
        }


        /// <summary>
        /// Если код отличается от суммы строк inputfields, то мы будем возвращать сумму строк inputfields
        /// </summary>
        private string ValidCode(string value)
        {
            // если код больше заявленной длины
            if (value.Length > CodeSize)
            {
                OnCodeChanged();
                return "";
            }
            return "";

        }


        // TODO: подумать над оптимизацией, если конечно это будет требоваться
        private void InputFieldValidation(object sender, TextCompositionEventArgs e)
        {
            if (IsNumericOnly)
            {
                e.Handled = !Int32.TryParse(e.Text, out var _);
            }
        }


        #endregion Private Methods
    }
}