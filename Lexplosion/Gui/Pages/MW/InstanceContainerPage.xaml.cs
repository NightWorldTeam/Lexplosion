using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.MW
{
    /// <summary>
    /// Interaction logic for InstanceContainerPage.xaml
    /// </summary>
    public partial class InstanceContainerPage : Page
	{
		public static InstanceContainerPage obj = null;
		private MainWindow _mainWindow;
		//private readonly Uri _nonImageUri = new Uri("pack://application:,,,/assets/images/icons/non_image.png");

		private bool _isInitializeInstance = false;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();
			_mainWindow = mainWindow;
			obj = this;
			GetInitializeInstance();
		}

		private async void GetInitializeInstance()
		{
			await Task.Run(() => InitializeInstance());
			_isInitializeInstance = true;
		}

		private void InitializeInstance()
		{
			List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(10, 0, ModpacksCategories.All);

			for (int j = 0; j < curseforgeInstances.ToArray().Length; j++)
			{
				BuildInstanceForm(curseforgeInstances[j].id.ToString(), j + 1,
					new Uri(curseforgeInstances[j].attachments[0].thumbnailUrl),
					curseforgeInstances[j].name,
					curseforgeInstances[j].authors[0].name,
					curseforgeInstances[j].summary,
					curseforgeInstances[j].categories);
			}
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(string instanceId, int row, Uri logoPath, string title, string author, string overview, List<CurseforgeInstanceInfo.Category> tags)
		{
			this.Dispatcher.Invoke(() =>
			{
				InstanceGrid.RowDefinitions.Add(GetRowDefinition());
				UserControls.InstanceForm instanceForm = new UserControls.InstanceForm(_mainWindow, title, "", author, overview, Int32.Parse(instanceId), logoPath, tags, false, true);
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

		private void MatchingResults(object sender, RoutedEventArgs e)
		{
			if (InstanceGrid.Children.Count > 1)
			{
				InstanceGrid.Children.RemoveRange(1, 10);
			}

			if (InstanceGrid.RowDefinitions.Count > 1)
			{
				InstanceGrid.RowDefinitions.RemoveRange(1, InstanceGrid.RowDefinitions.Count - 1);
			}

			if (SearchBox.Text.Length == 0)
			{
				if (!_isInitializeInstance)
				{
					InitializeInstance();
				}
			}
			else
			{
				_isInitializeInstance = false;
				//TODO: Вызывать функцию в LeftSideMenu, что вероянее всего уберёт задержку между auth и main window, а также уберёт перевызов из других страниц...
				List<CurseforgeInstanceInfo> curseforgeInstances = CurseforgeApi.GetInstances(10, 0, ModpacksCategories.All, SearchBox.Text);
				for (int j = 0; j < curseforgeInstances.ToArray().Length; j++)
				{
					BuildInstanceForm(curseforgeInstances[j].id.ToString(), j + 1,
						new Uri(curseforgeInstances[j].attachments[0].thumbnailUrl),
						curseforgeInstances[j].name,
						curseforgeInstances[j].authors[0].name,
						curseforgeInstances[j].summary,
						curseforgeInstances[j].categories);
				}
			}
		}
	}
}
