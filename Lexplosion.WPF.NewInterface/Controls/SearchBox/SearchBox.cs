using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_PLACEHOLDER_NAME, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_SEARCH_BUTTON_NAME, Type = typeof(Button))]
    public class SearchBox : TextBox
    {
        private const string PART_PLACEHOLDER_NAME = "PART_Placeholder";
        private const string PART_SEARCH_BUTTON_NAME = "PART_Search_Button";


        private TextBlock _placeholderBlock;
        private Button _searchButton;

        private string _lastRequests = string.Empty;


        #region Dependency Properties


        public static readonly DependencyProperty SearchCommandProperty
            = DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(SearchBox), 
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty SearchCommandParameterProperty
            = DependencyProperty.Register("SearchCommandParameter", typeof(object), typeof(SearchBox), 
                new FrameworkPropertyMetadata(defaultValue: (object) null));

        public static readonly DependencyProperty PlaceholderProperty
            = DependencyProperty.Register("Placeholder", typeof(string), typeof(SearchBox), 
                new FrameworkPropertyMetadata(defaultValue: string.Empty, propertyChangedCallback: OnPlaceholderChanged));

        public ICommand SearchCommand 
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        public object SearchCommandParameter 
        {
            get => (object)GetValue(SearchCommandParameterProperty);
            set => SetValue(SearchCommandParameterProperty, value);
        }

        public string Placeholder 
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        protected bool IsEmpty 
        {
            get; set;
        }


        #endregion Dependency Properties


        #region Consturctors


        static SearchBox() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected void SetPlaceholderText(string placeholder) 
        {
            if (_placeholderBlock != null) 
            { 
                _placeholderBlock.Text = placeholder;
            }
        }


        public override void OnApplyTemplate()
        {
            _searchButton = Template.FindName(PART_SEARCH_BUTTON_NAME, this) as Button;
            _placeholderBlock = Template.FindName(PART_PLACEHOLDER_NAME, this) as TextBlock;


            if (_searchButton == null) 
            {
                new Exception("Search Button is not exists");
            }

            if (_placeholderBlock == null) 
            {
                new Exception("Placeholder is not exists");
            }

            _searchButton.Click += searchButton_Click;
            this.KeyDown += inputField_KeyDown;
            _placeholderBlock.Text = Placeholder;

            base.OnApplyTemplate();
        }

        protected override void OnInitialized(EventArgs e)
        {
            UpdateIsEmpty();
            base.OnInitialized(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnTextChanged(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnLostFocus(e);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastRequests == this.Text) return;

            _lastRequests = this.Text;
            ExecuteSearchCommand();
        }

        private void inputField_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter || _lastRequests == this.Text) return;

            _lastRequests = this.Text;
            ExecuteSearchCommand();
            e.Handled = true;
        }


        private void ExecuteSearchCommand() 
        {
            if (SearchCommand == null) return;

            if (SearchCommandParameter == null)
            {
                SearchCommand?.Execute(_lastRequests);
            }
            else 
            {
                SearchCommand?.Execute(new object[2] { _lastRequests, SearchCommandParameter });
            }
        }


        private static void OnPlaceholderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var searchBox = sender as SearchBox;
            searchBox.SetPlaceholderText((string)e.NewValue);
        }

        /// <summary>
        /// Проверяет, содержит ли TextBox текст.
        /// Нужно, чтобы если TextBox содержит текст, то не показывать Placeholder.
        /// </summary>
        private void UpdateIsEmpty()
        {
            IsEmpty = string.IsNullOrEmpty(Text);

            if (_placeholderBlock != null) 
            { 
                if (IsEmpty)
                {
                    ShowPlaceholderBox();
                }
                else 
                {
                    HidePlaceholderBox();
                }
            }
        }

        private void HidePlaceholderBox() 
        {
            DoubleAnimation dA = new DoubleAnimation()
            {
                From = _placeholderBlock.Opacity,
                To = 0,
                Duration = new TimeSpan(0, 0, 0, 0, 50)
            };

            _placeholderBlock.BeginAnimation(FrameworkElement.OpacityProperty, dA);
        }

        private void ShowPlaceholderBox()
        {
            DoubleAnimation dA = new DoubleAnimation()
            {
                From = _placeholderBlock.Opacity,
                To = 1,
                Duration = new TimeSpan(0, 0, 0, 0, 250)
            };

            _placeholderBlock.BeginAnimation(FrameworkElement.OpacityProperty, dA);
        }


        #endregion Private Methods
    }
}
