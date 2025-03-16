﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    /// <summary>
    /// Interaction logic for ServerProfileOverviewGalleryView.xaml
    /// </summary>
    public partial class InstanceProfileOverviewGalleryView : UserControl
    {
        public InstanceProfileOverviewGalleryView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.Command.Execute(button.CommandParameter);
        }
    }
}
