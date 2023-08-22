using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Controls.Paginator
{
    [TemplatePart(Name = PART_PREVIOUS_PAGE_BUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_NEXT_PAGE_BUTTON, Type = typeof(Button))]
    public class Paginator : Control
    {
        private const string PART_PREVIOUS_PAGE_BUTTON = "PART_PreviousPageButton";
        private const string PART_NEXT_PAGE_BUTTON = "PART_NextPageButton";

        private Button _previousPageButton;
        private Button _nextPageButton;

        private int _currentPageIndex = int.MinValue;


        #region Dependency Properties


        public static readonly DependencyProperty NumberOfPagesProperty
            = DependencyProperty.Register("NumberOfPages", typeof(int), typeof(Paginator),
                new FrameworkPropertyMetadata(0));

        public int NumberOfPages 
        {
            get => (int)GetValue(NumberOfPagesProperty);
            set => SetValue(NumberOfPagesProperty, value);
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

            _previousPageButton.Click += _previousPageButton_Click;
            _nextPageButton.Click += _nextPageButton_Click;

            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void _nextPageButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void _previousPageButton_Click(object sender, RoutedEventArgs e)
        {

        }


        /// <summary>
        /// Return part if is it exists else will throw exeception
        /// </summary>
        /// <param name="name">part name</param>
        /// <returns>object</returns>
        private object GetPartHandler(string name) 
        {
            var part = Template.FindName(PART_PREVIOUS_PAGE_BUTTON, this);

            if (part == null) 
            {
                new Exception(name + " doesn't exists");
            }

            return part;
        }


        #endregion Private Methods
    }
}
