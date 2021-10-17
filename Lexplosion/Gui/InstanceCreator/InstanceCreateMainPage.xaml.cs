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

namespace Lexplosion.Gui.InstanceCreator
{
    /// <summary>
    /// Interaction logic for InstanceCreateMainPage.xaml
    /// </summary>
    public partial class InstanceCreateMainPage : Page
    {
        private List<string> instanceTags = new List<string>() 
        { 
            "Tech", "Magic", "Sci-Fi", "Adventure and RPG", "Exploration", "Mini Game", "Quests", 
            "Hardcore", "Map Based", "Small / Light", "Extra Large", "Combat / PvP", "Multiplayer",
            "FTB Offical Pack", "Skyblock"
        };
        private List<string> minecraftVersion = new List<string>() 
        { 
            "1.17.1", "1.16.5", "1.15.2", "1.14.4", "1.13.2", "1.12.2", "1.11", 
            "1.10.2", "1.9", "1.8.2", "1.7.10", "1.7.2", "1.7", "1.6.4"
        };

        public InstanceCreateMainPage()
        {
            InitializeComponent();
            /**foreach (string tag in instanceTags) 
            {
                TagsListCB.Items.Add(tag);
            }
            */
            int i = 0;
            foreach (string version in minecraftVersion) 
            {
                if (i == 0) 
                {
                    VersionCB.Items.Add("Latest release " + version);
                } 
                else 
                { 
                    VersionCB.Items.Add("Release " + version);
                }
                i++;
            }
            ModloaderVersion.Items.Add("Ну тут либо forge должен быть");
            ModloaderVersion.Items.Add("Ну или fabric. Я про их версии если чё");
            NoneSelected.IsChecked = true;
        }


    }
}
