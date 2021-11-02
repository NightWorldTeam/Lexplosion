using Lexplosion.Global;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.Gui.InstanceCreator
{
    /// <summary>
    /// Interaction logic for InstanceCreateMainPage.xaml
    /// </summary>
    public partial class InstanceCreateMainPage : Page
    {
        public static readonly SolidColorBrush unavalibleNameColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

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
            PreInitializePage();
        }

        private void PreInitializePage() 
        {
            // получаем все занятые имена модпаков
            foreach (var instance in UserData.Instances.List.Keys)
                unavailableNames.Add(UserData.Instances.List[instance].Name);

            // получаем все версии майнкрафта
            Lexplosion.Run.ThreadRun(() => SetupMinecraftVersions());

            // выставляем последнию версию
            VersionCB.SelectedIndex = 0;
            ModloaderVersion.Items.Add("Ну тут либо forge должен быть");
            ModloaderVersion.Items.Add("Ну или fabric. Я про их версии если чё");
            // устанавливаем отсутсвие modloader
            NoneSelected.IsChecked = true;
        }

        private void SetupMinecraftVersions()
        {
            foreach (var version in ToServer.GetVersionsList())
            {
                if (version.type == "release")
                {
                    this.Dispatcher.Invoke(() => {
                        //VersionCB.Items.Add(version.type + " " + version.id);
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
            // TODO: при добавление release, snapshot заменять их.
            string instanceVersion = VersionCB.Text;
            Console.WriteLine(instanceVersion);
            ManageLogic.CreateInstance(InstanceNameTB.Text, InstanceSource.Local, instanceVersion, ModloaderType.None, "");
        }

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
        
        private void CreateInstanceRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRadioButton = (RadioButton)sender;
            if (selectedRadioButton.Name == "NoneSelected") 
            {
                ModloaderVersion.Visibility = Visibility.Collapsed;
            }
            else if (selectedRadioButton.Name == "ForgeSelected")
            {
                ModloaderVersion.Visibility = Visibility.Visible;
            }
            else if (selectedRadioButton.Name == "FabricSelected")
            {
                ModloaderVersion.Visibility = Visibility.Visible;
            }
        }
    }
}
