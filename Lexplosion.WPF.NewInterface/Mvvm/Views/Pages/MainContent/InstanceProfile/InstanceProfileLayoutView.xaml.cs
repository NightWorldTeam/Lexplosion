﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    /// <summary>
    /// Логика взаимодействия для InstanceProfileLayoutView.xaml
    /// </summary>
    public partial class InstanceProfileLayoutView : UserControl
    {
        public InstanceProfileLayoutView()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            var da = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            grid.BeginAnimation(OpacityProperty, da);
        }
    }
}
