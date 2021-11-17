using Lexplosion.Global;
using Lexplosion.Gui.UserControls;
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

        class InstanceData
        {
            public string title;
            public string description;
        }

        private InstanceData instanceData;

        public OverviewPage(string title, string description)
        {
            InitializeComponent();
            this.instanceData = new InstanceData()
            {
                title = title,
                description = description
            };
            SetInstanceData(instanceData);
            SetAssets();
            List<string> images = new List<string> { 
                @"https://minecraftonly.ru/uploads/posts/2016-05/1463658257_2.jpg", 
                @"https://minecraftonly.ru/uploads/posts/2016-05/1463658449_3.jpg",
                @"https://minecraftonly.ru/uploads/posts/2016-05/1463658449_3.jpg"
            };
            Gallery gallery = new Gallery(images)
            {
                Height = 216,
                Width = 564
            };
            Grid.SetRow(gallery, 0);
            Container.Children.Add(gallery);
        }

        private void SetInstanceData(InstanceData instanceData)
        {
            this.instanceData = instanceData;
        }

        public void SetAssets()
        { /*
            if (instanceData.description != null)
            {
                Description.Text = instanceData.description;
            }
            else
            {
                Description.Text = "";
            }
            */
        }
    }
}