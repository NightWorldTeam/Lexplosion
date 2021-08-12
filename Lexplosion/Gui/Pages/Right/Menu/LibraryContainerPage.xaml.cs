using Lexplosion.Global;
using Lexplosion.Gui.Pages.Left;
using Lexplosion.Gui.Pages.Right.Instance;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.Pages.Right.Menu
{
    /// <summary>
    /// Interaction logic for LibraryContainerPage.xaml
    /// </summary>
    public partial class LibraryContainerPage : Page
    {
		private MainWindow _mainWindow;
		public LibraryContainerPage(MainWindow mainWindow)
        {
			_mainWindow = mainWindow;
			InitializeComponent();
			InitializeInstance();
		}

		/*
			public string description;
			public List<string> images;
			public string mainImage;
			public string xmx;
			public string xms;
		*/
		private void InitializeInstance() 
		{
			List<string> instance_tags1 = new List<string>() { "1.10.2", "Mods", "NightWorld" };
			Dictionary<string, InstanceParametrs> instancesList = UserData.InstancesList;
			Dictionary<string, InstanceAssets> instanceAssets = UserData.instancesAssets;

			int i = 0;
			foreach (string key in instancesList.Keys) 
			{
				string description = "";
				string image = "pack://application:,,,/assets/images/icons/non_image.png";
                if (instanceAssets.ContainsKey(key))
                {
					description = instanceAssets[key].description;
					image = WithDirectory.directory + "/launcherAssets/" + instanceAssets[key].mainImage;
				}

				BuildInstanceForm(
					key, i,
					new Uri(image),
					instancesList[key].Name,
					"by NightWorld",
					description,
					instance_tags1
					);

				i++;
			}
		}


		private void BuildInstanceForm(string id, int row, Uri logo_path, string title, string author, string overview, List<string> tags)
		{
			/// "EOS", 0, logo_path1, "Energy of Space", "NightWorld", "Our offical testing launcher modpack...", _instanceTags1
			// Добавляем строчку размером 150 px для нашего блока со сборкой.
			this.Dispatcher.Invoke(() =>
			{
				InstanceGrid.RowDefinitions.Add(GetRowDefinition());
				UserControls.InstanceForm instanceForm = new UserControls.InstanceForm(_mainWindow, title, id, author, overview, 0, logo_path, tags, true, true);
																																	 // Добавление в Столбики и Колноки в форме.
				Grid.SetRow(instanceForm, row);
				InstanceGrid.Children.Add(instanceForm);
			});
		}


		private RowDefinition GetRowDefinition()
		{
			RowDefinition rowDefinition = new RowDefinition()
			{
				Height = new GridLength(150, GridUnitType.Pixel)
			};
			return rowDefinition;
		}


		private ToggleButton SwitchToggleButton(StackPanel pageInstance, string content, RoutedEventHandler routedEventHandler, int index)
		{
			ToggleButton toggleButton = (ToggleButton)pageInstance.FindName("LeftSideMenuButton" + index);

			toggleButton.Content = content;
			toggleButton.Style = (Style)Application.Current.FindResource("MWCBS1");
			toggleButton.Click += routedEventHandler;

			if (index == 0) toggleButton.IsChecked = true;
			if (index == 1) toggleButton.IsChecked = false;

			return toggleButton;
		}
    }
}
