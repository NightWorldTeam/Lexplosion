using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using Lexplosion.Global;
using Lexplosion.Gui.Pages.Left;

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

        public OverviewPage(string title, string descrition)
        {
            InitializeComponent();
            SetAssets(title, descrition);
        }

        public void SetAssets(string title, string description)
        {
            if (description != null) 
            { 
                Description.Text = description;
            }
            else
            {
                Description.Text = "";
            }
        }

        private void Arrow_Right_Button(object sender, RoutedEventArgs e)
        {
            string modpack = LeftSideMenuPage.instance.selectedInstance;

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
        }

        private void Arrow_Left_Button(object sender, RoutedEventArgs e)
        {
            string modpack = LeftSideMenuPage.instance.selectedInstance;

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
