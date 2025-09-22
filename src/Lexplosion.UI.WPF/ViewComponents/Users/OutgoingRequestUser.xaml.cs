using System.Windows.Controls;

namespace Lexplosion.UI.WPF.ViewComponents.Users
{
    /// <summary>
    /// Interaction logic for OutgoingRequestUser.xaml
    /// </summary>
    public partial class OutgoingRequestUser : NightWorldUserViewBase
    {
        public OutgoingRequestUser()
        {
            InitializeComponent();
        }

        protected override Border GetBodyBorder() => PART_BodyBorder;
        protected override TextBlock GetNicknameTB() => PART_NicknameTB;
        protected override TextBlock GetStatusTB() => PART_StatusTB;
        protected override Border GetStatusIndicator() => PART_StatusIndicator;
    }
}
