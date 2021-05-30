using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Lexplosion.Gui
{
    static class GuiUris
    {
        public static Uri MainWindow = new Uri("pack://application:,,,/Gui/Windows/MainWindow.xaml");

        public static Uri LeftSideMenuPage = new Uri("pack://application:,,,/Gui/Pages/Left/LeftSideMenuPage.xaml");


        public static Uri InstancePage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/InstancePage.xaml");
        public static Uri OverviewPage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/OverviewPage.xaml");
        public static Uri VersionPage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/VersionPage.xaml");
        public static Uri ModsListPage = new Uri("pack://application:,,,/Gui/Pages/Right/Instance/ModsListPage.xaml");

        public static Uri ModpacksContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ModpacksContainerPage.xaml");
        public static Uri LibraryContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/LibraryContainerPage.xaml");
        public static Uri ServersContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/ServersContainerPage.xaml");
        public static Uri SettingsContainerPage = new Uri("pack://application:,,,/Gui/Pages/Right/Menu/SettingsContainerPage.xaml");
    }
}
