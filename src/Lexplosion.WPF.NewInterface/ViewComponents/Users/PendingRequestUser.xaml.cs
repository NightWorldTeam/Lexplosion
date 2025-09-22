using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.ViewComponents.Users
{
    /// <summary>
    /// Interaction logic for PendingRequestUser.xaml
    /// </summary>
    public partial class PendingRequestUser : NightWorldUserViewBase
    {
        public PendingRequestUser()
        {
            InitializeComponent();
        }

        protected override Border GetBodyBorder() => PART_BodyBorder;
        protected override TextBlock GetNicknameTB() => PART_NicknameTB;
        protected override TextBlock GetStatusTB() => PART_StatusTB;
        protected override Border GetStatusIndicator() => PART_StatusIndicator;
    }
}
