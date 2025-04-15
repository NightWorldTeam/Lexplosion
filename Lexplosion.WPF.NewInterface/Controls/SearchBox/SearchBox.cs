using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_PLACEHOLDER_NAME, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_SEARCH_BUTTON_NAME, Type = typeof(Button))]
    [TemplatePart(Name = PART_CLEAR_BUTTON_NAME, Type = typeof(Button))]
    public class SearchBox : TextBox
    {
        private const string PART_PLACEHOLDER_NAME = "PART_Placeholder";
        private const string PART_SEARCH_BUTTON_NAME = "PART_Search_Button";
        private const string PART_CLEAR_BUTTON_NAME = "PART_Clear_Button";


        private TextBlock _placeholderBlock;
        private Button _searchButton;
        private Button _clearButton;

        private string _lastRequests = string.Empty;


        #region Dependency Properties


        public static readonly DependencyProperty SearchCommandProperty
            = DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty SearchCommandParameterProperty
            = DependencyProperty.Register(nameof(SearchCommandParameter), typeof(object), typeof(SearchBox),
                new FrameworkPropertyMetadata(defaultValue: (object)null));

        public static readonly DependencyProperty PlaceholderProperty
            = DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBox),
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

        public bool IsEmpty
        {
            get => (bool)string.IsNullOrEmpty(Text);
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
            _clearButton = Template.FindName(PART_CLEAR_BUTTON_NAME, this) as Button;

            if (_searchButton == null)
            {
                throw new Exception("Search Button is not exists");
            }

            if (_placeholderBlock == null)
            {
                throw new Exception("PlaceholderKey is not exists");
            }

            if (_clearButton == null)
            {
                throw new Exception("Clear Button is not exists");
            }

            _searchButton.Click += searchButton_Click;
            
            _clearButton.Click += _clearButton_Click;
            _clearButton.Loaded += (sender, e) =>
            {
                _clearButton.Margin = new Thickness(0, 0, -(16 + _clearButton.ActualHeight), 0);
            };

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
            if (_lastRequests == Text) 
                return;

            _lastRequests = Text;
            ExecuteSearchCommand();
        }

        private void inputField_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter || _lastRequests == Text) 
                return;

            _lastRequests = Text;
            ExecuteSearchCommand();
            e.Handled = true;
        }

        private void _clearButton_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
            UpdateIsEmpty();

            if (_lastRequests == Text)
                return;

            _lastRequests = Text;
            ExecuteSearchCommand();
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
        /// Нужно, чтобы если TextBox содержит текст, то не показывать PlaceholderKey.
        /// </summary>
        private void UpdateIsEmpty()
        {
            if (_placeholderBlock == null)
            {
                return;
            }

                if (IsEmpty)
                {
                    if (this.IsFocused)
                        HidePlaceholderBox();
                    else
                        ShowPlaceholderBox();
                    HideClearButton();
                }
                else
                {
                    HidePlaceholderBox();
                    ShowClearButton();
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

        private void ShowClearButton() 
        {
            var tA = new ThicknessAnimation()
            {
                From = _clearButton.Margin,
                To = new Thickness(0, 0, -4, 0),
                Duration = new TimeSpan(0, 0, 0, 0, 250),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            _clearButton.BeginAnimation(FrameworkElement.MarginProperty, tA);
        }

        private void HideClearButton()
        {
            var tA = new ThicknessAnimation()
            {
                From = _clearButton.Margin,
                To = new Thickness(0, 0, -(16 + _clearButton.ActualHeight), 0),
                Duration = new TimeSpan(0, 0, 0, 0, 250),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut },
            };

            _clearButton.BeginAnimation(FrameworkElement.MarginProperty, tA);
        }


        #endregion Private Methods
    }
}
