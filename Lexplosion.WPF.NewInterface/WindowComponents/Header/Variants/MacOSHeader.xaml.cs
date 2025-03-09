using Lexplosion.Global;
using Lexplosion.Logic.FileSystem;
using Lexplosion.WPF.NewInterface.WindowComponents.Header.Variants;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
