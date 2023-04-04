using System.Reflection;
using System.Windows.Controls;

namespace Lexplosion.Common.Views.Pages.MainMenu.Settings
{
    /// <summary>
    /// Логика взаимодействия для AboutUsView.xaml
    /// </summary>
    public partial class AboutUsView : UserControl
    {
        public AboutUsView()
        {
            InitializeComponent();
            VersionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
