using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Views.Pages.MainContent.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AboutUsView.xaml
    /// </summary>
    public partial class AboutUsView : UserControl
    {
        public AboutUsView()
        {
            InitializeComponent();

            //CopyrightTextBlock.Text = GlobalData.GeneralSettings.LanguageId == "ru-RU" ? "© NightWorld, 2022г. Все права защищены." : "Copyright © 2022 NightWorld. All rights reserved.";
            //ProtectionTextBlock.Text = GlobalData.GeneralSettings.LanguageId == "ru-RU" ? "Данная программа защищена законами об авторских правах и международными соглашениями. Незаконное воспроизведение или распространение данной программы или любой ее части влечет гражданскую и уголовную ответственность." : "This program is protected by copyright laws and international treaties. Illegal reproduction or distribution of this software or any part of it is subject to civil and criminal liability.";

            VersionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var hel2xBitmap = new BitmapImage();
            hel2xBitmap.BeginInit();
            hel2xBitmap.UriSource = new System.Uri("https://night-world.org/requestProcessing/getUserImage.php?user_login=_Hel2x_");
            hel2xBitmap.EndInit();
            Hel2xHead.Background = new ImageBrush(hel2xBitmap);

            var sklaipBitmap = new BitmapImage();
            sklaipBitmap.BeginInit();
            sklaipBitmap.UriSource = new System.Uri("https://night-world.org/requestProcessing/getUserImage.php?user_login=Sklaip");
            sklaipBitmap.EndInit();
            SklaipHead.Background = new ImageBrush(sklaipBitmap);
        }

        private void IgorVK_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Constants.VKDefaultUrl + "idhel2x");
        }

        private void SvyatVK_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Constants.VKDefaultUrl + "lord_of_anecdotes");
        }

        private void GroupVK_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Constants.VKDefaultUrl + "nightworld_offical");
        }
    }
}
