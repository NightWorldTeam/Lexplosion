using Lexplosion.WPF.NewInterface.Controls.Message.Core.Types;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_TEXT_NAME, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_ICON_NAME, Type = typeof(Path))]
    public class MessageItem : Control
    {
        public const string PART_TEXT_NAME = "PART_Text";
        public const string PART_ICON_NAME = "PART_Icon";

        public const string InfoIcon = "M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z M464 336a48 48 0 1 0 96 0 48 48 0 1 0-96 0zm72 112h-48c-4.4 0-8 3.6-8 8v272c0 4.4 3.6 8 8 8h48c4.4 0 8-3.6 8-8V456c0-4.4-3.6-8-8-8z";
        public const string SuccessIcon = "M699 353h-46.9c-10.2 0-19.9 4.9-25.9 13.3L469 584.3l-71.2-98.8c-6-8.3-15.6-13.3-25.9-13.3H325c-6.5 0-10.3 7.4-6.5 12.7l124.6 172.8a31.8 31.8 0 0 0 51.7 0l210.6-292c3.9-5.3.1-12.7-6.4-12.7z M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z";
        public const string WarningIcon = "M464 720a48 48 0 1 0 96 0 48 48 0 1 0-96 0zm16-304v184c0 4.4 3.6 8 8 8h48c4.4 0 8-3.6 8-8V416c0-4.4-3.6-8-8-8h-48c-4.4 0-8 3.6-8 8zm475.7 440l-416-720c-6.2-10.7-16.9-16-27.7-16s-21.6 5.3-27.7 16l-416 720C56 877.4 71.4 904 96 904h832c24.6 0 40-26.6 27.7-48zm-783.5-27.9L512 239.9l339.8 588.2H172.2z";


        private readonly DispatcherTimer _timer = new();


        private SolidColorBrush _infoIconSolidColorBush = new SolidColorBrush(Color.FromRgb(22, 127, 252));
        private SolidColorBrush _successIconSolidColorBush = new SolidColorBrush(Color.FromRgb(117, 255, 51));
        private SolidColorBrush _warningIconSolidColorBush = new SolidColorBrush(Color.FromRgb(255, 191, 0));
        private SolidColorBrush _errorIconSolidColorBush = new SolidColorBrush(Color.FromRgb(255, 0, 0));


        private TextBlock _partText;
        private Path _partIcon;


        #region Properties


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MessageItem),
            new FrameworkPropertyMetadata(
                defaultValue: string.Empty,
                propertyChangedCallback: OnTextChanged
            )
        );

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type),
            typeof(MessageType),
            typeof(MessageItem),
            new FrameworkPropertyMetadata(
                defaultValue: MessageType.Error,
                propertyChangedCallback: OnTypeChanged
            )
        );

        public static readonly DependencyProperty InfoIconColorProperty =
            DependencyProperty.Register(
            nameof(InfoIconColor),
            typeof(SolidColorBrush),
            typeof(MessageItem),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnInfoIconColorChanged
            )
        );

        public static readonly DependencyProperty SuccessIconColorProperty =
            DependencyProperty.Register(
                nameof(SuccessIconColor),
                typeof(SolidColorBrush),
                typeof(MessageItem),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: OnSuccessIconColorChanged
                )
            );

        public static readonly DependencyProperty WarningIconColorProperty =
            DependencyProperty.Register(
                nameof(WarningIconColor),
                typeof(SolidColorBrush),
                typeof(MessageItem),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: OnWarningIconColorChanged
                )
            );

        public static readonly DependencyProperty ErrorIconColorProperty =
            DependencyProperty.Register(
                nameof(ErrorIconColor),
                typeof(SolidColorBrush),
                typeof(MessageItem),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: OnErrorIconColorChanged
                )
            );

        public static readonly DependencyProperty LiveTimeProperty =
            DependencyProperty.Register(
                nameof(LiveTime),
                typeof(TimeSpan),
                typeof(MessageItem),
                new FrameworkPropertyMetadata(
                    defaultValue: TimeSpan.FromSeconds(4),
                    propertyChangedCallback: OnLiveTimeChanged
                )
            );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public MessageType Type
        {
            get => (MessageType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public SolidColorBrush InfoIconColor
        {
            get => (SolidColorBrush)GetValue(InfoIconColorProperty);
            set => SetValue(InfoIconColorProperty, value);
        }


        public SolidColorBrush SuccessIconColor
        {
            get => (SolidColorBrush)GetValue(SuccessIconColorProperty);
            set => SetValue(SuccessIconColorProperty, value);
        }

        public SolidColorBrush WarningIconColor
        {
            get => (SolidColorBrush)GetValue(WarningIconColorProperty);
            set => SetValue(WarningIconColorProperty, value);
        }

        public SolidColorBrush ErrorIconColor
        {
            get => (SolidColorBrush)GetValue(ErrorIconColorProperty);
            set => SetValue(ErrorIconColorProperty, value);
        }

        public TimeSpan LiveTime
        {
            get => (TimeSpan)GetValue(LiveTimeProperty);
            set => SetValue(LiveTimeProperty, value);
        }


        #endregion Properties


        #region Constructors


        static MessageItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MessageItem),
                new FrameworkPropertyMetadata(typeof(MessageItem))
            );
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _partText = Template.FindName(PART_TEXT_NAME, this) as TextBlock;
            _partIcon = (Path)Template.FindName(PART_ICON_NAME, this);

            ChangeIconByType(Type);
            SetLiveTime(LiveTime);
        }


        #endregion Public & Protected Methods


        #region Private Methods


        private void SetLiveTime(TimeSpan span) 
        {
            _timer.Tick += _timer_Tick;
            _timer.Interval = LiveTime;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer)sender;
            timer.Stop();
            this.Visibility = Visibility.Collapsed;
        }

        private void ChangeIconColor(SolidColorBrush brush) 
        {
            _partIcon.Fill = brush;
        }

        private void ChangeIconByType(MessageType type) 
        {
            if (_partIcon == null)
                return;

            switch (type)
            {
                case MessageType.Info:
                    _partIcon.Data = Geometry.Parse(InfoIcon);
                    ChangeIconColor(InfoIconColor);
                    break;
                case MessageType.Success:
                    _partIcon.Data = Geometry.Parse(SuccessIcon);
                    ChangeIconColor(SuccessIconColor);
                    break;
                case MessageType.Warning:
                    _partIcon.Data = Geometry.Parse(WarningIcon);
                    ChangeIconColor(WarningIconColor);
                    break;
                case MessageType.Error:
                    _partIcon.Data = Geometry.Parse(InfoIcon);
                    ChangeIconColor(ErrorIconColor);
                    break;
                default:
                    break;
            }
        }


        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this)
            {

            }
        }

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this)
            {
                _this.ChangeIconByType(_this.Type);
            }
        }

        private static void OnInfoIconColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this) 
            {
                if (_this.Type == MessageType.Warning)
                    _this.ChangeIconColor((SolidColorBrush)e.NewValue);
            }
        }

        private static void OnSuccessIconColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this) 
            {
                if (_this.Type == MessageType.Success)
                    _this.ChangeIconColor((SolidColorBrush)e.NewValue);
            }
        }

        private static void OnWarningIconColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this) 
            {
                if (_this.Type == MessageType.Warning)
                    _this.ChangeIconColor((SolidColorBrush)e.NewValue);
            }
        }

        private static void OnErrorIconColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this) 
            {
                if (_this.Type == MessageType.Warning)
                    _this.ChangeIconColor((SolidColorBrush)e.NewValue);
            }
        }

        private static void OnLiveTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageItem _this)
            {

            }
        }


        #endregion Private Methods
    }
}
