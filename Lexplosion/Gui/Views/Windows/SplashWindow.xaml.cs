using System;
using System.ComponentModel;
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
        public SplashWindow()
        {
            InitializeComponent();
            MouseDown += delegate { try { DragMove(); } catch { } };
        }

        public void SmoothClosing()
        {
            DoubleAnimation _oa = new DoubleAnimation();
            _oa.From = Opacity;
            _oa.To = 0.0;
            _oa.Duration = new Duration(TimeSpan.FromMilliseconds(170d));
            BeginAnimation(OpacityProperty, _oa);

            Lexplosion.Run.TaskRun(delegate ()
            {
                Thread.Sleep(170);
                App.Current.Dispatcher.Invoke(delegate ()
                {
                    Close();
                });
            });
        }
    }
}
