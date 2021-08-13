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
		public bool LaunchButtonBlock = false; //блокировщик кнопки запуска модпака
		private List<string> _instanceTags1 = new List<string>() { "1.10.2", "Mods", "NightWorld" };
		private MainWindow _mainWindow;
		private readonly Uri _nonImageUri = new Uri("pack://application:,,,/assets/images/icons/non_image.png");

		private bool _isInitializeInstance = false;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			InitializeComponent();
			GetInitializeInstance();
			InitializeLeftSideMenu("Каталог", "Библиотека", "Сетевая игра", "Настройки");
			//CreateFakeInstance(4);
		}

		private void InitializeLeftSideMenu(string btn0, string btn1, string btn2, string btn3) 
		{
			LeftSideMenuButton0.Content = btn0;
			LeftSideMenuButton1.Content = btn1;
			LeftSideMenuButton2.Content = btn2;
			LeftSideMenuButton3.Content = btn3;
		}

		private async void GetInitializeInstance()
		{
			await Task.Run(() => InitializeInstance());
			_isInitializeInstance = true;
		}

		private void InitializeInstance()
		{
			List<CurseforgeInstanceInfo> curseforgeInstances = ToServer.GetCursforgeInstances(10, 0, ModpacksCategories.All);

			for (int j = 0; j < curseforgeInstances.ToArray().Length; j++)
			{
				BuildInstanceForm(curseforgeInstances[j].id.ToString(), j + 1,
					new Uri(curseforgeInstances[j].attachments[0].url),
					curseforgeInstances[j].name,
					curseforgeInstances[j].authors[0].name,
					curseforgeInstances[j].summary,
					_instanceTags1);
			}
		}

		private void CreateFakeInstance(int count)
		{
			/*
			Uri logoPath1 = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
			string description = "Цель данной сборки - развить свою колонию и построить транспортную сеть в виде железной дороги. Поезда здесь существуют не просто как декорации, они необходимы, ведь предметы имеют вес, руда генерируется огромными жилами, которые встречаются не очень то и часто. В процессе игры вам придётся постоянно перемещаться между различными месторождениями, своей базой, колонией. Основной индустриальный мод в этом модпаке - это Immersive Engineering, поэтому все строения буду выглядеть очень эффектно на фоне механизмов из этого мода. Во время игры вы с головой уйдёте в логистику, путешествия и индустриализацию.";
			string[] instanceName = new string[4] { "Energy of Space", "Long Tech", "Transport Network", "Over the Horizon" };
			string[] instanceId = new string[4] { "123", "123", "123", "123" };
			for (int j = 0; j < count; j++)
			{
				BuildInstanceForm(instanceId[j], j+1, logoPath1, instanceName[j], "NightWorld", description, _instanceTags1);
			}
			*/
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(string instanceId, int row, Uri logoPath, string title, string author, string overview, List<string> tags)
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
				List<CurseforgeInstanceInfo> curseforgeInstances = ToServer.GetCursforgeInstances(10, 0, ModpacksCategories.All, SearchBox.Text);
				for (int j = 0; j < curseforgeInstances.ToArray().Length; j++)
				{
					BuildInstanceForm(curseforgeInstances[j].id.ToString(), j + 1,
						new Uri(curseforgeInstances[j].attachments[0].url),
						curseforgeInstances[j].name,
						curseforgeInstances[j].authors[0].name,
						curseforgeInstances[j].summary,
						_instanceTags1);
				}
			}
		}
	}
}
