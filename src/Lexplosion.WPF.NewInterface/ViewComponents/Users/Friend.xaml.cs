using Lexplosion.WPF.NewInterface.Controls;
using Lexplosion.WPF.NewInterface.Tools;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.ViewComponents.Users
{
    /// <summary>
    /// Логика взаимодействия для Friend.xaml
    /// </summary>
    public partial class Friend : NightWorldUserViewBase
    {
        static Friend()
        {
        }

        public Friend()
        {
            InitializeComponent();
        }

        protected override Border GetBodyBorder()
        {
            return PART_BodyBorder;
        }

        protected override TextBlock GetNicknameTB()
        {
            return PART_NicknameTB;
        }

        protected override Border GetStatusIndicator()
        {
            return PART_StatusIndicator;
        }

        protected override TextBlock GetStatusTB()
        {
            return PART_StatusTB;
        }


        #region Private Methods


        protected override void UpdateBanner()
        {
            base.UpdateBanner();

            if (Banner == null)
            {
                return;
            }

            if (Banner.Colors != null)
            {
                if (Banner.Colors.PrimaryColor != null && Banner.Colors.PrimaryColor > 0x01000000) 
                {
                    MoreMenuToggleButton.Background = new SolidColorBrush(ColorTools.GetColor(Banner.Colors.PrimaryColor.Value));
                }

                if (Banner.Colors.ActivityColor != null && Banner.Colors.ActivityColor > 0x01000000) 
                {
                    MoreMenuToggleButton.Style.Setters.Add(new Setter(AdvancedToggleButton.ForegroundProperty, Banner.Colors.ActivityColor));
                }
            }
        }


        #endregion Private Methods
    }
}
