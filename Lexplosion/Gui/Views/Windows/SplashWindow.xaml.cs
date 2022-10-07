using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private const string loadingPlaceholder = "Идет загрузка...";
        private const string updatePlaceholder = "Идет обновление...";

        public SplashWindow()
        {
            InitializeComponent();
            MouseDown += delegate { DragMove(); };
        }

        public void ChangeLoadingBoardPlaceholder(bool isUpdate = false)
        {
            if (isUpdate && this.LoadingBoard.Placeholder == updatePlaceholder || !isUpdate && this.LoadingBoard.Placeholder == loadingPlaceholder)
            {
                return;
            }

            this.LoadingBoard.Placeholder = isUpdate ? updatePlaceholder : loadingPlaceholder;
        }

        public void SmoothClosing()
        {
            DoubleAnimation _oa = new DoubleAnimation();
            _oa.From = Opacity;
            _oa.To = 0.0;
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(190d));
            BeginAnimation(OpacityProperty, _oa);

            Lexplosion.Runtime.TaskRun(delegate ()
            {
                Thread.Sleep(190);
                App.Current.Dispatcher.Invoke(delegate ()
                {
                    Close();
                });
            });
        }
    }
}
