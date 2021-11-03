using Lexplosion.Global;
using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Lexplosion.Logic.Objects.CurseforgeInstanceInfo;

namespace Lexplosion.Gui.Pages.MW
{
	// <summary>
	/// Interaction logic for LibraryContainerPage.xaml
	/// </summary>
	public partial class LibraryContainerPage : Page
	{
		private MainWindow _mainWindow;
		private Dictionary<string, UserControls.InstanceForm> instances = new Dictionary<string, InstanceForm>();

		public LibraryContainerPage(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			InitializeComponent();
			InitializeInstance();
			UserData.Instances.AddInstanceNofity += InitializeInstance;
			UserData.Instances.SetAssetsNofity += SetInstanceAssets;
		}

		/*
			public string description;
			public List<string> images;
			public string mainImage;
			public string xmx;
			public string xms;
		*/
		public void InitializeInstance()
		{
			List<string> instanceTags = new List<string>();

			int i = 0;
			Console.WriteLine(String.Join(",", UserData.Instances.List.Keys));

			instances.Clear();

			foreach (string key in UserData.Instances.List.Keys)
			{
				string description;
				string imageUrl;
				// обновление assets
				description = "This modpack is not have description...";
				imageUrl = "pack://application:,,,/assets/images/icons/non_image.png";

				if (UserData.Instances.Assets.ContainsKey(key))
				{
					if (UserData.Instances.Assets[key] != null)
					{
						description = UserData.Instances.Assets[key].description;
						imageUrl = WithDirectory.directory + "/instances-assets/" + UserData.Instances.Assets[key].mainImage;
					}
				}

				this.Dispatcher.Invoke(() =>
				{
					UserControls.InstanceForm instance = BuildInstanceForm (key, i, imageUrl,
						UserData.Instances.List[key].Name, "by NightWorld", description, instanceTags);

					instances[key] = instance;
				});

				i++;
			}
		}

		private UserControls.InstanceForm BuildInstanceForm(string id, int row, string logo, string title, string author, string overview, List<string> tags)
		{
			/// "EOS", 0, logo_path1, "Energy of Space", "NightWorld", "Our offical testing launcher modpack...", _instanceTags1
			// Добавляем строчку размером 150 px для нашего блока со сборкой.
			InstanceGrid.RowDefinitions.Add(GetRowDefinition());
			var instanceForm = new UserControls.InstanceForm(_mainWindow, title, id, author, overview, "", new BitmapImage(new Uri(logo)), tags, true, true);
			// Добавление в Столбики и Колноки в форме.
			Grid.SetRow(instanceForm, row);
			InstanceGrid.Children.Add(instanceForm);

			return instanceForm;
		}

		private RowDefinition GetRowDefinition()
		{
			RowDefinition rowDefinition = new RowDefinition()
			{
				Height = new GridLength(150, GridUnitType.Pixel)
			};
			return rowDefinition;
		}

		public void SetInstanceAssets(string id, InstanceAssets assets) 
		{
			this.Dispatcher.Invoke(delegate () 
			{
				instances[id].SetInstanceAssets(assets);
			});
		}
	}
}
