using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
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

        private List<string> unavailableNames = new List<string>();

        public InstanceCreateMainPage()
        {
            InitializeComponent();
            /**foreach (string tag in instanceTags) 
            {
                TagsListCB.Items.Add(tag);
            }
            */
            foreach (var instance in UserData.Instances.List.Keys)
                unavailableNames.Add(UserData.Instances.List[instance].Name);

            Lexplosion.Run.ThreadRun(() => SetupMinecraftVersions());
            
            VersionCB.SelectedIndex = 0;
            ModloaderVersion.Items.Add("Ну тут либо forge должен быть");
            ModloaderVersion.Items.Add("Ну или fabric. Я про их версии если чё");
            NoneSelected.IsChecked = true;
        }

        private void SetupMinecraftVersions() 
        {
            foreach (var version in ToServer.GetVersionsList())
            {
                if(version.type == "release")
                {
                    this.Dispatcher.Invoke(() => {
                        VersionCB.Items.Add(version.id);
                    });
                } 
            }
        }

        private void CreateInstanceButton_Click(object sender, RoutedEventArgs e)
        {
            // InstanceNameTB.Text - Поле с название сборки
            // VersionCB - комбобокс с версиями
            // NoneSelected.IsChecked = True; - ничего не выбрано радиокнопка
            // ForgeSelected.IsChecked = True; - фордж радиокнопка
            // FabricSelected.IsChecked = True; - фабрик радиокнопка

            ManageLogic.CreateInstance(InstanceNameTB.Text, InstanceSource.Local, VersionCB.Text, "");
        }

        public static readonly SolidColorBrush unavalibleNameColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        private void InstanceNameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (unavailableNames.Contains(InstanceNameTB.Text))
            {
                Console.WriteLine("Yes");
                InstanceNameTB.Foreground = Brushes.Red;
            }
            else 
            {
                if (InstanceNameTB.Foreground.Equals(Brushes.Red)) 
                {
                    InstanceNameTB.Foreground = Brushes.White;
                }
            }
        }
    }
}
