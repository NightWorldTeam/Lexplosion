using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private string loadingPlaceholder = CultureInfo.CurrentCulture.Name == "ru-RU" ? "Идет загрузка..." : "Loading...";
        private string updatePlaceholder = CultureInfo.CurrentCulture.Name == "ru-RU" ? "Идет обновление..." : "Updating...";

        public SplashWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };

            this.LoadingBoard.Placeholder = loadingPlaceholder;
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
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(220d));
            BeginAnimation(OpacityProperty, _oa);

            Lexplosion.Runtime.TaskRun(delegate ()
            {
                Thread.Sleep(220);
                App.Current.Dispatcher.Invoke(delegate ()
                {
                    Close();
                });
            });
        }
    }
}
