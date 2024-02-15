using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Lexplosion.Controls
{
    [TemplatePart(Name = PART_CURRENT_IMAGE, Type = typeof(Border))]
    [TemplatePart(Name = PART_PREVIOUS_IMAGE, Type = typeof(Border))]
    [TemplatePart(Name = PART_NEXT_IMAGE, Type = typeof(Border))]
    [TemplatePart(Name = PART_NONE_IMAGE_BLOCK, Type = typeof(Border))]
    [TemplatePart(Name = PART_NONE_IMAGE_TITLE, Type = typeof(TextBlock))]
    public class SimpleGallery : ContentControl
    {
        const string PART_CURRENT_IMAGE = "PART_CurrertImage";
        const string PART_PREVIOUS_IMAGE = "PART_PreviousButton";
        const string PART_NEXT_IMAGE = "PART_NextButton";
        const string PART_NONE_IMAGE_BLOCK = "PART_NoneImageBlock";
        const string PART_NONE_IMAGE_TITLE = "PART_NoneImageTitle";


        private Border _currentImageBorder;
        private Border _previousImageBorder;
        private Border _nextImageBorder;
        private Border _noneImageBlock;
        private TextBlock _noneImageTitle;

        private int _itemsCount;
        private int _selectedIndex;


        #region Dependency Properties


        public static readonly DependencyProperty ItemsSourceProperty
            = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SimpleGallery),
                new FrameworkPropertyMetadata(
                    (IEnumerable<ImageSource>)null,
                    new PropertyChangedCallback(OnItemsSourceChanged)));


        #region Next Command Property


        /// <summary>
        /// The DependencyProperty for RoutedCommand
        /// </summary>
        public static readonly DependencyProperty NextButtonCommandProperty =
                DependencyProperty.Register(
                        "NextButtonCommand",
                        typeof(ICommand),
                        typeof(SimpleGallery),
                        new FrameworkPropertyMetadata((ICommand)null,
                            new PropertyChangedCallback(OnNextButtonCommandChanged)));


        /// <summary>
        /// The DependencyProperty for the CommandParameter
        /// </summary>
        public static readonly DependencyProperty NextButtonCommandParameterProperty =
                DependencyProperty.Register(
                        "NextButtonCommandParameter",
                        typeof(object),
                        typeof(SimpleGallery),
                        new FrameworkPropertyMetadata((object)null));


        #endregion Next Command Property


        #region Prev Command Property


        /// <summary>
        ///     The DependencyProperty for RoutedCommand
        /// </summary>
        public static readonly DependencyProperty PrevButtonCommandProperty =
                DependencyProperty.Register(
                        "PrevButtonCommand",
                        typeof(ICommand),
                        typeof(SimpleGallery),
                        new FrameworkPropertyMetadata((ICommand)null,
                            new PropertyChangedCallback(OnPrevButtonCommandChanged)));

        /// <summary>
        /// The DependencyProperty for the CommandParameter
        /// </summary>
        public static readonly DependencyProperty PrevButtonCommandParameterProperty =
                DependencyProperty.Register(
                        "PrevButtonCommandParameter",
                        typeof(object),
                        typeof(SimpleGallery),
                        new FrameworkPropertyMetadata((object)null));


        #endregion Prev Command Property


        public IEnumerable<ImageSource> ItemsSource
        {
            get { return (IEnumerable<ImageSource>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ICommand NextButtonCommand
        {
            get => (ICommand)GetValue(NextButtonCommandProperty);
            set => SetValue(NextButtonCommandProperty, value);
        }

        public object NextButtonCommandParameter
        {
            get => (object)GetValue(NextButtonCommandProperty);
            set => SetValue(NextButtonCommandProperty, value);
        }

        public ICommand PrevButtonCommand
        {
            get => (ICommand)GetValue(PrevButtonCommandProperty);
            set => SetValue(PrevButtonCommandProperty, value);
        }

        public object PrevButtonCommandParameter
        {
            get => (object)GetValue(PrevButtonCommandParameterProperty);
            set => SetValue(PrevButtonCommandParameterProperty, value);
        }


        #endregion Dependency Properties


        #region Properties


        private bool _isLeftBorder;
        public bool IsLeftBorder
        {
            get => _isLeftBorder; private set
            {
                _isLeftBorder = value;
                OnIsEmptyOrIsBorder();
            }
        }

        private bool _isRightBorder;
        public bool IsRightBorder
        {
            get => _isRightBorder; private set
            {
                _isRightBorder = value;
                OnIsEmptyOrIsBorder();
            }
        }

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty; private set
            {
                _isEmpty = value;
                OnIsEmptyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        static SimpleGallery()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleGallery), new FrameworkPropertyMetadata(typeof(SimpleGallery)));
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _currentImageBorder = Template.FindName(PART_CURRENT_IMAGE, this) as Border;
            _previousImageBorder = Template.FindName(PART_PREVIOUS_IMAGE, this) as Border;
            _nextImageBorder = Template.FindName(PART_NEXT_IMAGE, this) as Border;
            _noneImageBlock = Template.FindName(PART_NONE_IMAGE_BLOCK, this) as Border;
            _noneImageTitle = Template.FindName(PART_NONE_IMAGE_TITLE, this) as TextBlock;

            if (_currentImageBorder == null) 
            {
                throw new Exception("Border for current image not exists");
            }

            if (_previousImageBorder != null)
            {
                _previousImageBorder.MouseLeftButtonDown += _previousImageBorder_MouseDown;
            }

            if (_nextImageBorder != null) 
            {
                _nextImageBorder.MouseLeftButtonDown += _nextImageBorder_MouseDown;
            }


            base.OnApplyTemplate();
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void _nextImageBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsRightBorder)
                return;

            ChangeSelectedImage(_selectedIndex + 1);

            if (NextButtonCommand != null)
                NextButtonCommand.Execute(NextButtonCommandParameter);
        }

        private void _previousImageBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLeftBorder)
                return;

            ChangeSelectedImage(_selectedIndex - 1);

            if (PrevButtonCommand != null)
                PrevButtonCommand.Execute(PrevButtonCommandParameter);
        }


        private void ChangeSelectedImage(int index)
        {
            _currentImageBorder.Background = new ImageBrush(
                ItemsSource.ElementAt(index));

            _selectedIndex = index;

            if (index - 1 < 0)
            {
                IsLeftBorder = true;
            }
            else IsLeftBorder = false;

            if (index + 1 >= _itemsCount)
            {
                IsRightBorder = true;
            }
            else IsRightBorder = false;
        }


        private void OnIsEmptyChanged()
        {
            if (_noneImageBlock != null)
            {
                _noneImageBlock.Visibility = _isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }

            OnIsEmptyOrIsBorder();
        }


        private void OnIsEmptyOrIsBorder()
        {
            if (_nextImageBorder != null)
            {
                _nextImageBorder.Visibility = _isEmpty || _isRightBorder
                    ? Visibility.Collapsed : Visibility.Visible;
            }

            if (_previousImageBorder != null)
            {
                _previousImageBorder.Visibility = _isEmpty || _isLeftBorder
                    ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SimpleGallery ic = (SimpleGallery)d;
            
            var oldValue = (IEnumerable<ImageSource>)e.OldValue;
            var newValue = (IEnumerable<ImageSource>)e.NewValue;

            Runtime.DebugWrite(newValue?.Count());

            if (newValue != null) 
            {
                ic._itemsCount = newValue.Count();

                if (ic._itemsCount > 0)
                {
                    ic.ChangeSelectedImage(0);
                    ic._currentImageBorder.Effect = null;
                    ic.IsEmpty = false;
                    ic.IsLeftBorder = true;
                    return;
                }
            }
            
            ic._currentImageBorder.Background = new ImageBrush(
                new BitmapImage(new Uri("pack://Application:,,,/Assets/Images/background/authBG.png"))
                );

            // set blue effect
            ic._currentImageBorder.Effect = new BlurEffect();

            ic.IsEmpty = true;
            ic.IsRightBorder = true;
            ic.IsLeftBorder = true;
        }


        private static void OnNextButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ic = (SimpleGallery)d;
        }

        private static void OnPrevButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ic = (SimpleGallery)d;
        }


        #endregion Private Methods
    }
}
