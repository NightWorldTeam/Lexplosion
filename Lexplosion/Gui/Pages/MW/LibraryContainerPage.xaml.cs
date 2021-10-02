using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
			List<string> instanceTags = new List<string>();

			int i = 0;
			foreach (string key in UserData.Instances.List.Keys)
			{
				string description = "";
				string image = "pack://application:,,,/assets/images/icons/non_image.png";
				if (UserData.Instances.Assets.ContainsKey(key))
				{
					description = UserData.Instances.Assets[key].description;
					image = WithDirectory.directory + "/instances-assets/" + UserData.Instances.Assets[key].mainImage;
				}

				BuildInstanceForm(
					key, i,
					new Uri(image),
					UserData.Instances.List[key].Name,
					"by NightWorld",
					description,
					instanceTags
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
	}
}
