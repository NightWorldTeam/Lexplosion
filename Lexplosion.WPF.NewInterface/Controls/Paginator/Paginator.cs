using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_PREVIOUS_PAGE_BUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_NEXT_PAGE_BUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_NUMBERS_PANEL, Type = typeof(Panel))]
    public sealed class Paginator : Control
    {
        private const string PART_PREVIOUS_PAGE_BUTTON = "PART_PreviousPageButton";
        private const string PART_NEXT_PAGE_BUTTON = "PART_NextPageButton";
        private const string PART_NUMBERS_PANEL = "PART_NumbersPanel";

        private Button _previousPageButton;
        private Button _nextPageButton;
        private TextBlock _currentIndexValueTextBlock;
        private TextBlock _maxCountTextBlock;
        private Panel _numbersPanel;


        private bool _isFirst;
        private bool _isLast;


        #region Dependency Properties


        public static readonly DependencyProperty PrevCommandProperty
            = DependencyProperty.Register(nameof(PrevCommand), typeof(ICommand), typeof(Paginator),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty NextCommandProperty
            = DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(Paginator),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ToCommandProperty
            = DependencyProperty.Register(nameof(ToCommand), typeof(ICommand), typeof(Paginator), 
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PageCountProperty
            = DependencyProperty.Register(nameof(PageCount), typeof(uint), typeof(Paginator),
                new FrameworkPropertyMetadata(defaultValue: (uint)0, propertyChangedCallback: OnPageCountPropertyChanged));

        public static readonly DependencyProperty CurrentPageIndexProperty
            = DependencyProperty.Register(nameof(CurrentPageIndex), typeof(uint), typeof(Paginator),
                new FrameworkPropertyMetadata(defaultValue: (uint)0, propertyChangedCallback: OnCurrentPageIndexChanged));

        public static readonly DependencyProperty PageNumberStyleProperty
            = DependencyProperty.Register(nameof(PageNumberStyle), typeof(Style), typeof(Paginator), 
                new FrameworkPropertyMetadata(null));

        public ICommand PrevCommand
        {
            get => (ICommand)GetValue(PrevCommandProperty);
            set => SetValue(PrevCommandProperty, value);
        }

        public ICommand NextCommand 
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        public ICommand ToCommand 
        {
            get => (ICommand)GetValue(ToCommandProperty);
            set => SetValue(ToCommandProperty, value);
        }

        public uint PageCount 
        {
            get => (uint)GetValue(PageCountProperty);
            set => SetValue(PageCountProperty, value);
        }

        public uint CurrentPageIndex
        {
            get => (uint)GetValue(CurrentPageIndexProperty);
            set => SetValue(CurrentPageIndexProperty, value);
        }

        public Style PageNumberStyle 
        {
            set => SetValue(PageNumberStyleProperty, value);
            get => (Style)GetValue(PageNumberStyleProperty);
        }


        #endregion Dependency Properties


        #region Constructors


        public Paginator()
        {

        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _previousPageButton = GetPartHandler(PART_PREVIOUS_PAGE_BUTTON) as Button;
            _nextPageButton = GetPartHandler(PART_NEXT_PAGE_BUTTON) as Button;
            _currentIndexValueTextBlock = GetPartHandler("CurrentIndexValueTextBlock") as TextBlock;
            _maxCountTextBlock = GetPartHandler("MaxPage") as TextBlock;
            _numbersPanel = GetPartHandler(PART_NUMBERS_PANEL) as Panel;
            _currentIndexValueTextBlock.Text = (CurrentPageIndex + 1).ToString();
            _maxCountTextBlock.Text = 1.ToString();

            _previousPageButton.IsEnabled = CurrentPageIndex > 0;
            _nextPageButton.IsEnabled = true;
            _previousPageButton.Click += _previousPageButton_Click;
            _nextPageButton.Click += _nextPageButton_Click;

            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void _nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex + 1 <= PageCount)
            {
                CurrentPageIndex++;
                NextCommand?.Execute(CurrentPageIndex);
            }
        }

        private void _previousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPageIndex - 1 != uint.MaxValue)
            {
                CurrentPageIndex--;
                PrevCommand?.Execute(CurrentPageIndex);
            }
        }

        private void _pageIndex_Click() 
        {
            if (CurrentPageIndex - 1 > 0 && CurrentPageIndex + 1 <= PageCount)
            {
                CurrentPageIndex++;
                ToCommand?.Execute(CurrentPageIndex);
            }
        }


        /// <summary>
        /// Return part if is it exists else will throw exeception
        /// </summary>
        /// <param name="name">part name</param>
        /// <returns>object</returns>
        private object GetPartHandler(string name)
        {
            var part = Template.FindName(name, this);

            if (part == null)
            {
                throw new Exception($"{name} doesn't exists");
            }

            return part;
        }


        private static void OnCurrentPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Paginator _this) 
            {
                _this._isFirst = _this.CurrentPageIndex - 1 == uint.MaxValue; 
                _this._isLast = _this.CurrentPageIndex + 1 == _this.PageCount;

                if (_this._previousPageButton != null)
                    _this._previousPageButton.IsEnabled = !_this._isFirst;
                
                if (_this._nextPageButton != null)
                    _this._nextPageButton.IsEnabled = !_this._isLast;

                if (_this._currentIndexValueTextBlock != null) 
                {
                    _this._currentIndexValueTextBlock.Text = ((uint)e.NewValue + 1).ToString();
                }
            }
        }

        private static void OnPageCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Paginator _this) 
            {
                if (_this._maxCountTextBlock != null) 
                {
                    _this._maxCountTextBlock.Text = e.NewValue.ToString();
                }
            }
        }


        #endregion Private Methods
    }
}
