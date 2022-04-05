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
    /// 

    public partial class InstanceCreateMainPage : Page
    {
        public static readonly SolidColorBrush _unavalibleNameColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        private MainWindow _mainWindow;

        private List<string> _instanceTags = new List<string>() 
        { 
            "Tech", "Magic", "Sci-Fi", "Adventure and RPG", "Exploration", "Mini Game", "Quests", 
            "Hardcore", "Map Based", "Small / Light", "Extra Large", "Combat / PvP", "Multiplayer",
            "FTB Offical Pack", "Skyblock"
        };

        private List<string> _selectedModsList = new List<string>();
        private List<string> _unavailableNames = new List<string>();
        
        private ModloaderType _selectedModloaderType = ModloaderType.None;

        private Dictionary<string, List<string>> ForgeVersions = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> FabricVersions = new Dictionary<string, List<string>>();

        private RadioButton _selectedRadioButton;

        public InstanceCreateMainPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            PreInitializePage();
            InstanceNameTB.Text = "New Modpack";
        }

        private void PreInitializePage() 
        {
            // получаем все версии майнкрафта
            Lexplosion.Run.TaskRun(() => SetupMinecraftVersions());

            // получаем все занятые имена модпаков
            foreach (var instance in UserData.Instances.Record.Keys)
                _unavailableNames.Add(UserData.Instances.Record[instance].Name);
            
            // выставляем последнию версию
            VersionCB.SelectedIndex = 0;
            ModloaderVersion.Visibility = Visibility.Hidden;
            // устанавливаем отсутсвие modloader
            _selectedRadioButton = NoneSelected;
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

        private void SetupModloaderVersions(string mcVersion, ModloaderType modloaderType)
        {
            ModloaderVersion.Items.Clear();
            foreach (var version in LoadingModloaderVersion(mcVersion, modloaderType)) 
            {
                this.Dispatcher.Invoke(() => {
                    ModloaderVersion.Items.Add(version);
                });
            }
        }

        private List<string> LoadingModloaderVersion(string mcVersion, ModloaderType modloaderType)
        {
            if (modloaderType == ModloaderType.Forge)
            {
                if (ForgeVersions.ContainsKey(mcVersion)) return ForgeVersions[mcVersion];
                ForgeVersions.Add(mcVersion, ToServer.GetModloadersList(mcVersion, modloaderType));
                return ForgeVersions[mcVersion];
            }
            else
            { 
                if (FabricVersions.ContainsKey(mcVersion)) return FabricVersions[mcVersion];
                FabricVersions.Add(mcVersion, ToServer.GetModloadersList(mcVersion, modloaderType));
                return FabricVersions[mcVersion];
            }
        }

        private void CreateInstanceButton_Click(object sender, RoutedEventArgs e)
        { 
            // TODO: при добавление release, snapshot заменять их.
            if (InstanceNameTB.Text != "")
            {
                string instanceVersion = VersionCB.Text;
                Console.WriteLine(instanceVersion);
                ManageLogic.CreateInstance(
                    InstanceNameTB.Text, InstanceSource.Local, instanceVersion, _selectedModloaderType, ModloaderVersion.Text
                );
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
        
        private void SelectInstanceRadioButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Перезагружать форму после перевыбора версии
            var selectedRadioButton = (RadioButton)sender;
            var gameVersion = VersionCB.Text;
            if (selectedRadioButton.Name == "NoneSelected") 
            {
                ModloaderVersion.Visibility = Visibility.Collapsed;
                _selectedModloaderType = ModloaderType.None;
                
            }
            else if (selectedRadioButton.Name == "ForgeSelected")
            {
                ModloaderVersion.Visibility = Visibility.Visible;
                _selectedModloaderType = ModloaderType.Forge;

                SetupModloaderVersions(gameVersion, ModloaderType.Forge);
            }
            else if (selectedRadioButton.Name == "FabricSelected")
            {
                ModloaderVersion.Visibility = Visibility.Visible;
                _selectedModloaderType = ModloaderType.Fabric;
                SetupModloaderVersions(gameVersion, ModloaderType.Fabric);
            }
        }

        private void VersionCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gameVersion = VersionCB.Text;
            if (_selectedRadioButton.Name == "ForgeSelected")
                SetupModloaderVersions(VersionCB.Text, ModloaderType.Forge);
            else if (_selectedRadioButton.Name == "FabricSelected")
                SetupModloaderVersions(VersionCB.Text, ModloaderType.Fabric);
        }
    }
}
