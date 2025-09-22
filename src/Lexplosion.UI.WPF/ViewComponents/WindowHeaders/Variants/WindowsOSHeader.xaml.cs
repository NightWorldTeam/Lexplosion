using Lexplosion.Global;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.UI.WPF.WindowComponents.Header.Variants
{
    /// <summary>
    /// Interaction logic for WindowsOSHeader.xaml
    /// </summary>
    public partial class WindowsOSHeader : HeaderBase
    {
        public WindowsOSHeader()
        {
            InitializeComponent();

            WindowHeaderPanelButtonsGrid.HorizontalAlignment = GlobalData.GeneralSettings.NavBarInLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            AdditionalFuncsPanel.ChangedOrintation += ChangeOrintation;
            AdditionalFuncsPanel.NotificationsButtonState += NotificationOpenedChanged;
            ChangeOrintation();
        }

        public override void ChangeOrintation()
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

                AdditionalFuncsPanel.HorizontalAlignment = HorizontalAlignment.Left;

                GlobalData.GeneralSettings.NavBarInLeft = true;
            }
            else
            {
                WindowHeaderPanelButtons.RenderTransform = new RotateTransform(360);
                WindowHeaderPanelButtonsGrid.HorizontalAlignment = HorizontalAlignment.Left;

                AdditionalFuncsPanel.HorizontalAlignment = HorizontalAlignment.Right;

                GlobalData.GeneralSettings.NavBarInLeft = false;
            }

            Runtime.ServicesContainer.DataFilesService.SaveSettings(GlobalData.GeneralSettings);
        }
    }
}
