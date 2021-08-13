using Lexplosion.Global;
using Lexplosion.Gui.UserControls;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Lexplosion.Logic.Objects.CurseforgeInstanceInfo;

namespace Lexplosion.Gui.Pages.MW
{
	// <summary>
	/// Interaction logic for LibraryContainerPage.xaml
	/// </summary>
	public partial class LibraryContainerPage : Page
	{
		private MainWindow _mainWindow;
		public LibraryContainerPage(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			InitializeComponent();
			InitializeLeftPanel();
			InitializeInstance();
			
		}

		private void InitializeLeftPanel()
		{
			LeftPanel leftPanel = new LeftPanel(this, LeftPanel.PageType.InstanceLibrary, _mainWindow);
			Grid.SetColumn(leftPanel, 0);
			MainGrid.Children.Add(leftPanel);
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
			List<Category> instanceTags = new List<Category>();
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
					instanceTags
					);

				i++;
			}
		}


		private void BuildInstanceForm(string id, int row, Uri logo_path, string title, string author, string overview, List<Category> tags)
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
	}
}
