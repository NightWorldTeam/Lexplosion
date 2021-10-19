using Lexplosion.Logic.Network;
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

        public InstanceCreateMainPage()
        {
            InitializeComponent();
            /**foreach (string tag in instanceTags) 
            {
                TagsListCB.Items.Add(tag);
            }
            */
            foreach (var version in ToServer.GetVersionsList()) 
            {
                VersionCB.Items.Add(version.type + " " + version.id);
            }

            VersionCB.SelectedIndex = 0;
            ModloaderVersion.Items.Add("Ну тут либо forge должен быть");
            ModloaderVersion.Items.Add("Ну или fabric. Я про их версии если чё");
            NoneSelected.IsChecked = true;
        }

        private void CreateInstanceButton_Click(object sender, RoutedEventArgs e)
        {
            // InstanceNameTB.Text - Поле с название сборки
            // VersionCB - выбранная версия
            // NoneSelected.IsChecked = True; - ничего не выбрано
            // ForgeSelected.IsChecked = True; - фордж
            // FabricSelected.IsChecked = True; - фабрик
        }
    }
}
