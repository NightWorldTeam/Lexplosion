using Lexplosion.Objects;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using Lexplosion.Gui.Windows;

namespace Lexplosion.Gui.Pages.Right.Modpack
{
    /// <summary>
    /// Логика взаимодействия для OverviewPage.xaml
    /// </summary>
    public partial class OverviewPage : Page
    {

        public static OverviewPage instance = null;
        private int images_count;
        private int lastIndex = 0;

        public OverviewPage()
        {
            InitializeComponent();
            instance = this;
            SetAssets();
        }

        public void SetAssets()
        {
            string modpack = MainWindow.Obj.selectedModpack;

            if (UserData.profilesAssets != null && UserData.profilesAssets.ContainsKey(modpack))
            {

                Description.Text = UserData.profilesAssets[modpack].description;
                images_count = UserData.profilesAssets[modpack].images.Count-1;
                SetImages(UserData.profilesAssets[modpack].images);
            }
            else
            {
                Description.Text = "";
            }
        }

        private void Arrow_Right_Button(object sender, RoutedEventArgs e)
        {
            string modpack = MainWindow.Obj.selectedModpack;

            if(UserData.profilesAssets != null)
            {
                if (lastIndex < images_count)
                {
                    lastIndex += 1;
                }
                else
                {
                    lastIndex = 0;
                }
                SetImages(UserData.profilesAssets[modpack].images);
            }
        }

        private void Arrow_Left_Button(object sender, RoutedEventArgs e)
        {
            string modpack = MainWindow.Obj.selectedModpack;

            if(UserData.profilesAssets != null)
            {
                if (lastIndex - 1 != -1)
                {
                    lastIndex -= 1;
                }
                else
                {
                    lastIndex = images_count;
                }
                SetImages(UserData.profilesAssets[modpack].images);
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
