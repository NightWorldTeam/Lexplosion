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

namespace Lexplosion.Gui.Pages.MW
{
    // <summary>
    /// Interaction logic for LibraryContainerPage.xaml
    /// </summary>
    public partial class LibraryContainerPage : Page
	{
		private MainWindow _mainWindow;
		private Dictionary<string, InstanceForm> instances = new Dictionary<string, InstanceForm>();

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
			int i = 0;
			// обновление assets
			string description, imageUrl;
			List<string> instanceTags = new List<string>();
			
			Console.WriteLine(String.Join(",", UserData.Instances.Record.Keys));

			instances.Clear();

			foreach (string key in UserData.Instances.Record.Keys)
			{
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
				else if (_mainWindow.DownloadingInstanceForms.ContainsKey(key)) 
				{
					InstanceGrid.RowDefinitions.Add(GetRowDefinition());
					Grid.SetRow(_mainWindow.DownloadingInstanceForms[key], i);
				}
				else
				{

					this.Dispatcher.Invoke(() =>
					{
						UserControls.InstanceForm instance = BuildInstanceForm(
							key, i, imageUrl, UserData.Instances.Record[key].Name, "by NightWorld", description, instanceTags
						);
						instances[key] = instance;
					});
				}
				i++;
			}
		}

		private InstanceForm BuildInstanceForm(string id, int row, string logo, string title, string author, string overview, List<string> tags)
		{
			/// "EOS", 0, logo_path1, "Energy of Space", "NightWorld", "Our offical testing launcher modpack...", _instanceTags1
			var instanceForm = new InstanceForm(
				_mainWindow, title, id, author, overview, "", new BitmapImage(new Uri(logo)), tags, true, true
			);
			// Добавляем строчку размером 150 px для нашего блока со сборкой.
			InstanceGrid.RowDefinitions.Add(GetRowDefinition());
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
			this.Dispatcher.Invoke(delegate
			{
				instances[id].SetLocalInstanceAssets(assets);
			});
		}
	}
}
