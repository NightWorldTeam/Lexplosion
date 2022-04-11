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
		private Dictionary<string, InstanceForm> _instances = new Dictionary<string, InstanceForm>();

		public LibraryContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();
			_mainWindow = mainWindow;
			InitializeInstance();
			UserData.Instances.AddInstanceNofity += InitializeInstance;
			UserData.Instances.SetAssetsNofity += SetInstanceAssets;

			// TODO: Сделать различного рода сортировки - по времени создания, по версии, по наиграности
		}

		public void InitializeInstance()
		{
			if (UserData.Instances.Record.Keys.Count == 0)
				return;

			var i = 0;
			// обновление assets
			string description, imageUrl, author, outsideInstanceId;
			var instanceTags = new List<string>();

			_instances.Clear();

			foreach (string key in UserData.Instances.Record.Keys)
			{
				description = "This modpack is not have description...";
				imageUrl = "pack://application:,,,/assets/images/icons/non_image.png";
				author = "by NightWorld";
				outsideInstanceId = string.Empty;
				if (UserData.Instances.Assets.ContainsKey(key))
				{
					if (UserData.Instances.Assets[key] != null)
					{
						description = UserData.Instances.Assets[key].description;
						imageUrl = WithDirectory.directory + "/instances-assets/" + UserData.Instances.Assets[key].mainImage;
						author = UserData.Instances.Assets[key].author;

						foreach (var key1 in UserData.Instances.ExternalIds.Keys)
						{
							if (UserData.Instances.ExternalIds[key1] == key)
								outsideInstanceId = key1;
						}
					}
				}

				UserControls.InstanceForm instance = BuildInstanceForm(
					key, i, imageUrl, UserData.Instances.Record[key].Name, author, description, outsideInstanceId, instanceTags,
					false, true
				);
				_instances[key] = instance;
				i++;
			}
			InstanceGrid.RowDefinitions.Add(GetRowDefinition());
			Grid.SetRow(AddInstanceBtn, i);
			i++;
		}

		private InstanceForm BuildInstanceForm(string id, 
			int row, 
			string logo, 
			string title, 
			string author, 
			string overview, 
			string outsideInstanceId, 
			List<string> tags, 
			bool isInstalled,
			bool isUpdateAvailable)
		{
			/// "EOS", 0, logo_path1, "Energy of Space", "NightWorld", "Our offical testing launcher modpack...", _instanceTags1
			var instanceForm = new InstanceForm(
				_mainWindow, title, id, author, overview, outsideInstanceId, new BitmapImage(new Uri(logo)), tags, true, isInstalled, isUpdateAvailable
			);
			// Добавляем строчку размером 150 px для нашего блока со сборкой.
			if (InstanceGrid.RowDefinitions.Count <= row) {
				InstanceGrid.RowDefinitions.Add(GetRowDefinition());
			}
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
				_instances[id].SetLocalInstanceAssets(assets);
			});
		}

        private void AddInstanceBtn_Click(object sender, RoutedEventArgs e)
        {
			_mainWindow.LeftPanel.AddCustomModpack();
        }
    }
}
