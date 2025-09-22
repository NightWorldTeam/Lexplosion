using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lexplosion.UI.WPF.Mvvm.ViewModels;
using System.Threading;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages
{
    /// <summary>
    /// Interaction logic for WelcomeView.xaml
    /// </summary>
    public partial class WelcomeView : UserControl
    {
        private WelcomeViewModel _viewmodel;

        public WelcomeView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewmodel = (WelcomeViewModel)DataContext;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var da = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(2),
                From = 0,
                To = 1,
                BeginTime = TimeSpan.FromSeconds(0.5)
            };

            da.Completed += Da_Completed;

            // Применяем анимацию к заголовку
            Lexplosion.BeginAnimation(OpacityProperty, da);
            Logo.BeginAnimation(OpacityProperty, da);
        }

        private void Da_Completed(object sender, EventArgs e)
        {
            var da1 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                From = 0,
                To = 1,
            };

            da1.Completed += SubtitleLoaded;
            WelcomeText.BeginAnimation(OpacityProperty, da1);
        }

        private void SubtitleLoaded(object sender, EventArgs e)
        {
            var da1 = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                From = 1,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(2)
            };

            _viewmodel.ToDarkTheme();

            Runtime.TaskRun(() =>
            {
                Thread.Sleep(1000);
                App.Current.Dispatcher.Invoke(() =>
                {
                    da1.Completed += ToThemeSelect;

                    Lexplosion.BeginAnimation(OpacityProperty, da1);
                    Logo.BeginAnimation(OpacityProperty, da1);
                    WelcomeText.BeginAnimation(OpacityProperty, da1);
                });
            });
        }

        private void ToThemeSelect(object sender, EventArgs e)
        {
            _viewmodel.ToThemeSelectCommand.Execute(null);
        }

        private void Logo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Runtime.DebugWrite(e.GetPosition((IInputElement)this), color: ConsoleColor.Red);
        }
    }
}
