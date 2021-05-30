using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Lexplosion.Gui.Pages.Right.Instance;

namespace Lexplosion.Gui.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // хранит объект этого окна
        public static MainWindow Obj = null; 
        public static MainWindow instance = null;

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.Obj = this;
            instance = this;

            MouseDown += delegate { try { DragMove(); } catch { } };

            //это страница по умолчанию
            LeftSideFrame.Source = GuiUris.LeftSideMenuPage;
            //это страница по умолчанию
            RightSideFrame.Source = GuiUris.ModpacksContainerPage; 
        }

        /* <-- Функционал MessageBox --> */
        private void Okey(object sender, RoutedEventArgs e)
        {

        }

        public void SetMessageBox(string message, string title = "Ошибка")
        {
            MessageBox.Show(message + " " + title);
        }

        private void CloseWindow(object sender, RoutedEventArgs e) => Run.Exit();
        private void HideWindow(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
    }
}
