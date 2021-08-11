using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using Lexplosion.Global;
using Lexplosion.Gui.Pages.Left;
using Lexplosion.Gui.Windows;

namespace Lexplosion.Gui.Pages.Right.Instance
{
    /// <summary>
    /// Логика взаимодействия для OverviewPage.xaml
    /// </summary>
    public partial class OverviewPage : Page
    {

        public static OverviewPage instance = null;
        private int images_count = 1;
        private int lastIndex = 0;

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
        }

        private void SetInstanceData(InstanceData instanceData) 
        {
            this.instanceData = instanceData;
        }

        public void SetAssets()
        {
            if (instanceData.description != null) 
            { 
                Description.Text = instanceData.description;
            }
            else
            {
                Description.Text = "";
            }
        }

        private void Arrow_Right_Button(object sender, RoutedEventArgs e)
        {
            /*string modpack = LeftSideMenuPage.instance.selectedInstance;

            if (UserData.instancesAssets != null)
            {
                if (lastIndex < images_count)
                {
                    lastIndex += 1;
                }
                else
                {
                    lastIndex = 0;
                }
                SetImages(UserData.instancesAssets[modpack].images);
            }
            */
        }

        private void Arrow_Left_Button(object sender, RoutedEventArgs e)
        {
            /*string modpack = LeftSideMenuPage.instance.selectedInstance;

            if (UserData.instancesAssets != null)
            {
                if (lastIndex - 1 != -1)
                {
                    lastIndex -= 1;
                }
                else
                {
                    lastIndex = images_count;
                }
                SetImages(UserData.instancesAssets[modpack].images);
            }
            */
        }

        private void SetImages(List<string> mpAssets)
        {
            if (mpAssets.Count != 0)
            {
                try
                {
                    ChangeImageBrush.ImageSource = new BitmapImage(new Uri(UserData.settings["gamePath"] + "/launcherAssets/" + mpAssets[lastIndex], UriKind.Relative));
                    ImageManagerGrid.Visibility = Visibility.Visible;
                }
                catch  { }
            }
            else
            {
                ImageManagerGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
