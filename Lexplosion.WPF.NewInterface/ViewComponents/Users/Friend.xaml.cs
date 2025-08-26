using Lexplosion.WPF.NewInterface.Tools;
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
            if (Banner == null)
            {
                return;
            }

            base.UpdateBanner();

            if (Banner.MoreButtonColor != null)
            {
                MoreMenuToggleButton.Background = new SolidColorBrush(ColorTools.GetColor(Banner.MoreButtonIconColor.Value));
                MoreMenuToggleButton.Style.Triggers.Clear();
                var isCheckedTrigger = new Trigger();
                isCheckedTrigger.Property = ToggleButton.IsCheckedProperty;
                isCheckedTrigger.Value = true;
                // не даем кнопке исчезнуть
                isCheckedTrigger.Setters.Add(new Setter(FrameworkElement.VisibilityProperty, true));
                //
                //isCheckedTrigger.Setters.Add(new Setter(FrameworkElement));

                MoreMenuToggleButton.Style.Triggers.Add(isCheckedTrigger);
                

            }
            else
            {
                //NicknameTB.SetResourceReference(ForegroundProperty, "PrimaryForegroundSolidColorBrush");
            }
        }


        #endregion Private Methods
    }
}
