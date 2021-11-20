using Lexplosion.Global;
using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.Pages.Instance
{
    /// <summary>
    /// Interaction logic for OverviewPage.xaml
    /// </summary>
    public partial class OverviewPage : Page
    {
        public static OverviewPage instance = null;
        private InstanceProperties _instanceProperties;
        private List<string> images = new List<string> {
                @"https://minecraftonly.ru/uploads/posts/2016-05/1463658257_2.jpg",
                @"https://minecraftonly.ru/uploads/posts/2016-05/1463658449_3.jpg",
                @"https://minecraftonly.ru/uploads/posts/2016-05/1463658449_3.jpg"
        };

        public OverviewPage(InstanceProperties instanceProperties)
        {
            InitializeComponent();
            _instanceProperties = instanceProperties;
            Gallery gallery = new Gallery(images)
            {
                Height = 216,
                Width = 564
            };
            Grid.SetRow(gallery, 0);
            Console.WriteLine(1);
            Container.Children.Add(gallery);
            SetAssets();
        }

        public void SetAssets()
        {
            if (_instanceProperties.InstanceAssets.description != null)
            {
                Description.Text = _instanceProperties.InstanceAssets.description;
            }
            else
            {
                Description.Text = "";
            }
        }
    }
}