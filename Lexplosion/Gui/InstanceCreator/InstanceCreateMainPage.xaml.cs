using Lexplosion.Global;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
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
        public static readonly SolidColorBrush _unavalibleNameColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        private List<string> _instanceTags = new List<string>() 
        { 
            "Tech", "Magic", "Sci-Fi", "Adventure and RPG", "Exploration", "Mini Game", "Quests", 
            "Hardcore", "Map Based", "Small / Light", "Extra Large", "Combat / PvP", "Multiplayer",
            "FTB Offical Pack", "Skyblock"
        };

        private List<string> _selectedModsList = new List<string>();
        private List<string> _unavailableNames = new List<string>();
        private MainWindow _mainWindow;
        private ModloaderType selectedModloaderType = ModloaderType.None;

        private Dictionary<string, List<string>> ForgeVersionsList = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> FabricVersionList = new Dictionary<string, List<string>>();

        public InstanceCreateMainPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            PreInitializePage();
        }

        private void PreInitializePage() 
        {
            // получаем все занятые имена модпаков
            foreach (var instance in UserData.Instances.List.Keys)
                _unavailableNames.Add(UserData.Instances.List[instance].Name);

            // получаем все версии майнкрафта
            Lexplosion.Run.TaskRun(() => SetupMinecraftVersions());
            
            // выставляем последнию версию
            VersionCB.SelectedIndex = 0;
            ModloaderVersion.Visibility = Visibility.Hidden;
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
                        //ForgeVersionsList.Add(version.id, ToServer.GetModloadersList(version.id, ModloaderType.Forge));
                        //FabricVersionList.Add(version.id, ToServer.GetModloadersList(version.id, ModloaderType.Fabric));
                    });
                }
            }
        }

        private void SetupModloaderVersions(string minecraftVersion, ModloaderType modloaderType)//, List<string> modloaderVersions) 
        {
            var modloaderVersions = ToServer.GetModloadersList(minecraftVersion, modloaderType);
            foreach (var version in modloaderVersions) 
            {
                this.Dispatcher.Invoke(() => {
                    ModloaderVersion.Items.Add(version);
                });
            }
        }

        private void CreateInstanceButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: при добавление release, snapshot заменять их.
            if (InstanceNameTB.Text != "")
            {
                string instanceVersion = VersionCB.Text;
                Console.WriteLine(instanceVersion);
                ManageLogic.CreateInstance(InstanceNameTB.Text, InstanceSource.Local, instanceVersion, selectedModloaderType, ModloaderVersion.Text);
                _mainWindow.LeftPanel.BackToInstanceContainer(LeftPanel.PageType.InstanceLibrary, null);
            }
        }

        private void InstanceNameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_unavailableNames.Contains(InstanceNameTB.Text))
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
            var gameVersion = VersionCB.Text;
            if (selectedRadioButton.Name == "NoneSelected") 
            {
                ModloaderVersion.Visibility = Visibility.Collapsed;
                selectedModloaderType = ModloaderType.None;
            }
            else if (selectedRadioButton.Name == "ForgeSelected")
            {
                ModloaderVersion.Visibility = Visibility.Visible;
                selectedModloaderType = ModloaderType.Forge;
                SetupModloaderVersions(gameVersion, ModloaderType.Forge);//, ForgeVersionsList[gameVersion]);
            }
            else if (selectedRadioButton.Name == "FabricSelected")
            {
                ModloaderVersion.Visibility = Visibility.Visible;
                selectedModloaderType = ModloaderType.Fabric;
                SetupModloaderVersions(gameVersion, ModloaderType.Fabric);//, FabricVersionList[gameVersion]);
            }
        }

        private void VersionCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("123");
        }
    }
}
