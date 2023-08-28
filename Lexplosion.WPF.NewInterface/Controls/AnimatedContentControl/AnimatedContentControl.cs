using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public enum ContentControlAnimation
    {
        None,
        Fade,
        SlideToLeft,
        SlideToRight,
        SlideToTop,
        SlideToBottom,
    }

    public class AnimatedContentControl : ContentControl
    {
        private Shape _paintArea;
        private ContentPresenter _contentPresenter;
        private DpiScale _dpi;


        public static readonly DependencyProperty AnimationTypeProperty
            = DependencyProperty.Register(nameof(AnimationType), typeof(ContentControlAnimation), typeof(AnimatedContentControl),
                new FrameworkPropertyMetadata(defaultValue: ContentControlAnimation.SlideToLeft));


        public ContentControlAnimation AnimationType
        {
            get => (ContentControlAnimation)GetValue(AnimationTypeProperty);
            set => SetValue(AnimationTypeProperty, value);
        }


        /// <summary>
        /// This gets called when the template has been applied and we have our visual tree
        /// </summary>
        public override void OnApplyTemplate()
        {
            _paintArea = Template.FindName("PART_PaintArea", this) as Shape;
            _contentPresenter = Template.FindName("PART_MainContent", this) as ContentPresenter;

            base.OnApplyTemplate();
        }

        /// <summary>
        /// This gets called when the content we're displaying has changed
        /// </summary>
        /// <param name="oldContent">The content that was previously displayed</param>
        /// <param name="newContent">The new content that is displayed</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (_paintArea != null && _contentPresenter != null)
            {
                _paintArea.Fill = CreateBrushFromVisual(_contentPresenter);

                switch (AnimationType)
                {
                    case ContentControlAnimation.SlideToLeft:
                        BeginXAnimateContentReplacement();
                        break;
                    case ContentControlAnimation.SlideToTop:
                        BeginYAnimateContentReplacement();
                        break;
                    case ContentControlAnimation.Fade:
                        BeginFadeAnimationContentReplacement();
                        break;
                    default:
                        break;
                }

            }
            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// Creates a brush based on the current appearance of a visual element. 
        /// The brush is an ImageBrush and once created, won't update its look
        /// </summary>
        /// <param name="v">The visual element to take a snapshot of</param>
        private Brush CreateBrushFromVisual(Visual v)
        {
            if (v == null)
                throw new ArgumentNullException("v");
            _dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);

            var target = new RenderTargetBitmap((int)(this.ActualWidth * _dpi.DpiScaleX), (int)(this.ActualHeight * _dpi.DpiScaleY),
                                                _dpi.PixelsPerInchX, _dpi.PixelsPerInchY, PixelFormats.Default);
            target.Render(v);
            var brush = new ImageBrush(target);
            brush.Freeze();
            return brush;
        }

        private void BeginFadeAnimationContentReplacement()
        {
            _paintArea.Visibility = Visibility.Visible;
            _contentPresenter.Opacity = 0;

            var opacityAnimation = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(3)
            };

            opacityAnimation.Completed += OpacityAnimation_Completed; //(s, e) => _paintArea.Visibility = Visibility.Hidden;

            _paintArea.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void OpacityAnimation_Completed(object sender, EventArgs e)
        {
            _paintArea.Visibility = Visibility.Hidden;
            _paintArea.Opacity = 1;
            var opacityAnimation1 = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.35)
            };
            _contentPresenter.BeginAnimation(OpacityProperty, opacityAnimation1);
        }

        private void BeginYAnimateContentReplacement()
        {
            var newContentTransform = new TranslateTransform();
            var oldContentTransform = new TranslateTransform();

            _paintArea.RenderTransform = oldContentTransform;
            _contentPresenter.RenderTransform = newContentTransform;

            _paintArea.Visibility = Visibility.Visible;

            newContentTransform.BeginAnimation(
                TranslateTransform.YProperty,
                CreateSlideAnimation(this.ActualHeight, 0));

            oldContentTransform.BeginAnimation(
                TranslateTransform.YProperty,
                CreateSlideAnimation(0, -this.ActualHeight, (s, e) => _paintArea.Visibility = Visibility.Hidden));

            _paintArea.BeginAnimation(
                OpacityProperty,
                CreateOpacityAnimation(1, 0, (s, e) => _paintArea.Opacity = 1));

            _contentPresenter.BeginAnimation(
                OpacityProperty,
                CreateOpacityAnimation(0, 1));
        }


        private void BeginXAnimateContentReplacement()
        {
            var newContentTransform = new TranslateTransform();
            var oldContentTransform = new TranslateTransform();

            _paintArea.RenderTransform = oldContentTransform;
            _contentPresenter.RenderTransform = newContentTransform;

            _paintArea.Visibility = Visibility.Visible;

            newContentTransform.BeginAnimation(
                TranslateTransform.XProperty,
                CreateSlideAnimation(this.ActualWidth, 0));

            oldContentTransform.BeginAnimation(
                TranslateTransform.XProperty,
                CreateSlideAnimation(0, -this.ActualWidth, (s, e) => _paintArea.Visibility = Visibility.Hidden));

            _paintArea.BeginAnimation(
                OpacityProperty,
                CreateOpacityAnimation(1, 0, (s, e) => _paintArea.Opacity = 1));
            _contentPresenter.BeginAnimation(
                OpacityProperty,
                CreateOpacityAnimation(0, 1));
        }

        private AnimationTimeline CreateSlideAnimation(double from, double to, EventHandler whenDone = null)
        {
            var duration = new Duration(TimeSpan.FromSeconds(0.35 * 1.5));
            var anim = new DoubleAnimation(from, to, duration);
            if (whenDone != null)
                anim.Completed += whenDone;
            anim.Freeze();
            return anim;
        }

        private AnimationTimeline CreateOpacityAnimation(double from, double to, EventHandler whenDone = null)
        {
            var duration = new Duration(TimeSpan.FromSeconds(0.35));
            var anim = new DoubleAnimation(from, to, duration);
            if (whenDone != null)
                anim.Completed += whenDone;
            anim.Freeze();
            return anim;
        }
    }
}
