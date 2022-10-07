using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.Controls
{
    [TemplatePart(Name = PART_TEXTBOX_NAME, Type = typeof(PlaceholderTextBox))]
    [TemplatePart(Name = PART_BUTTON_NAME, Type = typeof(Button))]
    public class SearchBox : PlaceholderTextBox
    {
        /**
         TextBox - Content on search
         Button - Clear content on search
         Button - Search Start

        List of history (in the future)
         */

        private const string PART_TEXTBOX_NAME = "PART_TextBox";
        private const string PART_BUTTON_NAME = "PART_Button";

        private TextBox _textBox;
        private Button _searchButton;


        #region Dependency Properties fields


        public static readonly DependencyProperty SearchActionProperty
            = DependencyProperty.Register("SearchAction", typeof(Action<string, bool>), typeof(SearchBox), new PropertyMetadata(null));


        #endregion Dependency Properties fields


        #region setters / getters


        public Action<string, bool> SearchAction
        {
            get => (Action<string, bool>)GetValue(SearchActionProperty);
            set => SetValue(SearchActionProperty, value);
        }


        #endregion setters / getters


        #region Constructors

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _textBox = Template.FindName(PART_TEXTBOX_NAME, this) as TextBox;
            _searchButton = Template.FindName(PART_BUTTON_NAME, this) as Button;

            if (_textBox == null)
                throw new Exception("SearchBox: " + PART_TEXTBOX_NAME + " doesn't exist");

            try
            {
                _searchButton.Click += SearchButtonClicked;
            }
            catch
            {
                throw new Exception("SearchBox: " + PART_BUTTON_NAME + " doesn't exist");
            }

            base.OnApplyTemplate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;
            Search();

            base.OnKeyDown(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods

        private void Search()
        {
            SearchAction?.Invoke(_textBox.Text, false);
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            Search();
        }
        #endregion Private Methods
    }
}
