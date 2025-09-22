using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lexplosion.UI.WPF.Controls
{
    [TemplatePart(Name = PART_FIRST_TEXT_NAME, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_SECOND_TEXT_NAME, Type = typeof(TextBlock))]
    public class TextSwitcherButton : ToggleButton
    {
        private const string PART_FIRST_TEXT_NAME = "PART_FirstText";
        private const string PART_SECOND_TEXT_NAME = "PART_SecondText";

        private TextBlock _firstTextBlock;
        private TextBlock _secondTextBlock;


        #region Dependency Properties


        public static readonly DependencyProperty FirstTextProperty
            = DependencyProperty.Register(nameof(FirstText), typeof(string), typeof(TextSwitcherButton),
                new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty SecondTextProperty
            = DependencyProperty.Register(nameof(SecondText), typeof(string), typeof(TextSwitcherButton),
                new FrameworkPropertyMetadata(string.Empty));

        public string FirstText 
        {
            get => (string)GetValue(FirstTextProperty);
            set => SetValue(FirstTextProperty, value);
        }

        public string SecondText
        {
            get => (string)GetValue(FirstTextProperty);
            set => SetValue(FirstTextProperty, value);
        }


        #endregion Dependency Properties


        #region Consturctors


        static TextSwitcherButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextSwitcherButton), new FrameworkPropertyMetadata(typeof(TextSwitcherButton)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _firstTextBlock = Template.FindName(PART_FIRST_TEXT_NAME, this) as TextBlock;
            _secondTextBlock = Template.FindName(PART_SECOND_TEXT_NAME, this) as TextBlock;


            if (_firstTextBlock == null)
            {
                new Exception("First TextBlock is not exists");
            }

            if (_secondTextBlock == null)
            {
                new Exception("Second TextBlock is not exists");
            }

            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods
    }
}
