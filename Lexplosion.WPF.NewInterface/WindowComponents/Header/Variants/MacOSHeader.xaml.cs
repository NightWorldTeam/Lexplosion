﻿using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.WindowComponents.Header.Variants;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.WindowComponents.Header.Variants
{
    /// <summary>
    /// Interaction logic for MacOSHeader.xaml
    /// </summary>
    public partial class MacOSHeader : HeaderBase
    {
        public MacOSHeader()
        {
            InitializeComponent();

            WindowHeaderPanelButtonsGrid.HorizontalAlignment = GlobalData.GeneralSettings.NavBarInLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            ChangeOrintation(null, null);
        }

        public override void ChangeOrintation(object sender, MouseButtonEventArgs e)
        {
            var opacityHideAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 0
            };

            var opacityShowAnimation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(0.35 / 2),
                To = 1
            };

            // перемещаем кнопки и панель в нужную сторону.
            opacityHideAnimation.Completed += (object sender, EventArgs e) =>
            {
                ChangeWHPHorizontalOrintation();
                WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityShowAnimation);
            };

            // скрываем 
            WindowHeaderPanelButtonsGrid.BeginAnimation(OpacityProperty, opacityHideAnimation);
        }

        public void ChangeWHPHorizontalOrintation() 
        {
            if (WindowHeaderPanelButtonsGrid.HorizontalAlignment == HorizontalAlignment.Left)
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(180);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Right;

                AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Left;

                GlobalData.GeneralSettings.NavBarInLeft = true;
            }
            else
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(360);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Left;

                AddtionalFuncs.HorizontalAlignment = HorizontalAlignment.Right;

                GlobalData.GeneralSettings.NavBarInLeft = false;
            }

            DataFilesManager.SaveSettings(GlobalData.GeneralSettings);
        } 
    }
}
