﻿using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AppearanceSettingsView.xaml
    /// </summary>
    public partial class AppearanceSettingsView : UserControl
    {
        public AppearanceSettingsView()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            Point s = e.GetPosition(e.Device.Target);
        }
    }
}
