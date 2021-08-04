using Lexplosion.Global;
using Lexplosion.Gui.Pages.Left;
using Lexplosion.Gui.Pages.Right.Instance;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.Pages.Right.Menu
{
    /// <summary>
    /// Interaction logic for TestModpacksContainerPage.xaml
    /// </summary>
    public partial class InstanceContainerPage : Page
    {
		// дОЛБАЁБ, ПОТОМУ-ЧТО ЭТОТ ваш VIsual Studия
		class InstanceData
		{
			/* string */
			public string InstanceTitle;
			public string InstanceId;
			public string author;
			public string overview;
			/* int */
			public int CurseforgeInstanceId;
			/* uri */
			public Uri LogoPath;
			/* tags*/
			public List<string> tags;
			/* bool */
			public bool IsInstalled;
			public bool IsAddedToLibrary;
		}

		public static InstanceContainerPage obj = null;
		public bool LaunchButtonBlock = false; //блокировщик кнопки запуска модпака
		private List<string> _instanceTags1 = new List<string>() { "1.10.2", "Mods", "NightWorld" };
		private MainWindow _mainWindow;
		private readonly Uri _nonImageUri = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
		private Dictionary<object, InstanceData> _instancesData = new Dictionary<object, InstanceData>();

		public InstanceContainerPage(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			InitializeComponent();
			GetInitializeInstance();
			//CreateFakeInstance(4);
		}

		private async void GetInitializeInstance() 
		{
			await Task.Run(() => InitializeInstance());
		}

		private void InitializeInstance() 
		{
			//TODO: Вызывать функцию в LeftSideMenu, что вероянее всего уберёт задержку между auth и main window, а также уберёт перевызов из других страниц...
			List<CurseforgeInstanceInfo> curseforgeInstances = ToServer.GetCursforgeInstances(20, 0, ModpacksCategories.All);
			for (int j = 0; j < curseforgeInstances.ToArray().Length; j++)
			{
				BuildInstanceForm(curseforgeInstances[j].id.ToString(), j,
					new Uri(curseforgeInstances[j].attachments[0].url),
					curseforgeInstances[j].name,
					curseforgeInstances[j].authors[0].name,
					curseforgeInstances[j].summary,
					_instanceTags1);
			}
		}

		private void CreateFakeInstance(int count) 
		{
			Uri logoPath1 = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
			string description = "Цель данной сборки - развить свою колонию и построить транспортную сеть в виде железной дороги. Поезда здесь существуют не просто как декорации, они необходимы, ведь предметы имеют вес, руда генерируется огромными жилами, которые встречаются не очень то и часто. В процессе игры вам придётся постоянно перемещаться между различными месторождениями, своей базой, колонией. Основной индустриальный мод в этом модпаке - это Immersive Engineering, поэтому все строения буду выглядеть очень эффектно на фоне механизмов из этого мода. Во время игры вы с головой уйдёте в логистику, путешествия и индустриализацию.";
			string[] instanceName = new string[4] { "Energy of Space", "Long Tech", "Transport Network", "Over the Horizon" };
			string[] instanceId = new string[4] { "EOS", "LT", "TN", "OTH" };
			for (int j = 0; j < count; j++)
			{
				BuildInstanceForm(instanceId[j], j, logoPath1, instanceName[j], "NightWorld", description, _instanceTags1);
			}
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(string instanceName, int row, Uri logoPath, string title, string author, string overview, List<string> tags)
		{
			/// "EOS", 0, logoPath1, "Energy of Space", "NightWorld", "Our offical testing launcher modpack...", _instanceTags1
			// Добавляем строчку размером 150 px для нашего блока со сборкой.
			InstanceData instanceForm = new InstanceData()
			{
				/* string */
				InstanceTitle = title,
				InstanceId = "",
				author = author,
				overview = overview,
				/* int */
				CurseforgeInstanceId = Int32.Parse(instanceName),
				/* uri */
				LogoPath = logoPath,
				/* list<string >*/
				tags = tags,
				/* bool */
				IsInstalled = false,
				IsAddedToLibrary = false
			};
			this.Dispatcher.Invoke(() =>
			{
				InstanceGrid.RowDefinitions.Add(GetRowDefinition());

				var canvas = new Canvas()
				{
					Height = 120,
					Width = 620,
					Margin = new Thickness(40, 0, 0, 0),
					Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#151719")),
					Name = "id" + instanceName,
					Effect = new DropShadowEffect() {
						ShadowDepth = 1,
						Color = (Color)ColorConverter.ConvertFromString("#151719"),
						Opacity = 0.3
					}
				};

				var grid = new Grid();
				// Делаем разметку для элементов.
				ColumnDefinition columnDefinition1 = new ColumnDefinition()
				{
					Width = new GridLength(120, GridUnitType.Pixel)
				};

				ColumnDefinition columnDefinition2 = new ColumnDefinition()
				{
					Width = new GridLength(440, GridUnitType.Pixel)
				};

				ColumnDefinition columnDefinition3 = new ColumnDefinition()
				{
					Width = new GridLength(60, GridUnitType.Pixel)
				};

				RowDefinition rowDefinition = new RowDefinition()
				{
					Height = new GridLength(120, GridUnitType.Pixel)
				};

				// Instance Logo
				var moreButton = new Button()
				{
					Name = "id" + instanceName,
					Background = new ImageBrush(new BitmapImage(logoPath)),
					Style = (Style)Application.Current.FindResource("InstanceMoreButton")
				};
				moreButton.Click += (object sender, RoutedEventArgs e) => ClickedMoreButton(sender, e, title, overview, author, tags);

				// Title
				var textContentGrid = new Grid()
				{
					Margin = new Thickness(10, 0, 5, 0)
				};

				RowDefinition textContentRowDefinition1 = new RowDefinition()
				{
					Height = new GridLength(30, GridUnitType.Pixel)
				};

				RowDefinition textContentRowDefinition2 = new RowDefinition()
				{
					Height = new GridLength(1, GridUnitType.Star)
				};

				// Разметка для заголовков
				var titleGrid = new Grid();
				ColumnDefinition columnDefinitionTitle1 = new ColumnDefinition()
				{
					Width = new GridLength(1, GridUnitType.Star)
				};

				ColumnDefinition columnDefinitionTitle2 = new ColumnDefinition()
				{
					Width = new GridLength(100, GridUnitType.Pixel)
				};

				RowDefinition rowDefinitionTitle1 = new RowDefinition()
				{
					Height = new GridLength(30, GridUnitType.Pixel)
				};

				// Заголовок - Название
				var textBlockTitle = new TextBlock()
				{
					Text = title,
					Padding = new Thickness(0),
					FontSize = 22,
					Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
					TextTrimming = TextTrimming.WordEllipsis
				};

				// Об Авторе
				var textBlockAuthor = new TextBlock()
				{
					//Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
					Text = "by " + author,
					FontSize = 12,
					Padding = new Thickness(0),
					Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a9b1ba")),
					VerticalAlignment = VerticalAlignment.Center,
					FontFamily = new FontFamily(new Uri("pack://application:,,,/assets/fonts/"), "./#Casper Bold"),
					TextTrimming = TextTrimming.WordEllipsis
				};

				// Описание
				var overviewGrid = new Grid()
				{
					Margin = new Thickness(0, 5, 0, 0)
				};

				RowDefinition overviewRowDefinition1 = new RowDefinition()
				{
					Height = new GridLength(35, GridUnitType.Pixel)
				};

				RowDefinition overviewRowDefinition2 = new RowDefinition()
				{
					Height = new GridLength(1, GridUnitType.Star)
				};

				var textBlockOverview = new TextBlock()
				{
					Text = overview,
					Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d5d5d5")),
					HorizontalAlignment = HorizontalAlignment.Left,
					FontSize = 16,
					TextTrimming = TextTrimming.WordEllipsis
				};

				// панель с тегами
				var tagsWrapPanel = new WrapPanel()
				{
					Margin = new Thickness(0, 1, 1, 1),
					Orientation = Orientation.Horizontal
				};

				// добавление тегов
				foreach (string tag in tags)
				{
					tagsWrapPanel.Children.Add(GetTagsButton(tag));
				}

				// панель с кнопками
				var instanceButtonGrid = new Grid();
				RowDefinition instanceButtonRowDefinition1 = new RowDefinition()
				{
					Height = new GridLength(60, GridUnitType.Pixel)
				};
				RowDefinition instanceButtonRowDefinition2 = new RowDefinition()
				{
					Height = new GridLength(60, GridUnitType.Pixel)
				};

				var instanceLaunchButton = new UserControls.InstanceMultiButton(UserControls.InstanceMultiButton.ButtonTypes.Basic, instanceForm.IsInstalled, instanceForm.IsAddedToLibrary)
				{
					Name = "id" + instanceName + "Download",
				};
				_instancesData.Add((object)instanceLaunchButton, instanceForm);
				if (_instancesData[(object)instanceLaunchButton].IsInstalled) 
				{
					instanceLaunchButton.MouseLeftButtonDown += LaunchInstance;
				}
				else 
				{
					instanceLaunchButton.MouseLeftButtonDown += CurrentDownloadInstance;
				}

				var addToLibrary = new UserControls.InstanceMultiButton(UserControls.InstanceMultiButton.ButtonTypes.Library, instanceForm.IsInstalled, instanceForm.IsAddedToLibrary)
				{
					Name = "id" + instanceName + "Export",
				};
				_instancesData.Add((object)addToLibrary, instanceForm);
				if (_instancesData[(object)addToLibrary].IsAddedToLibrary)
				{

				}
				else 
				{
					
				}

				var progressBar = new ProgressBar()
				{
					Minimum = 0,
					Maximum = 100,
					Value = 48,
					Width = 400,
					Height = 60,
					Visibility = Visibility.Hidden
					//Style = (Style)Application.Current.FindResource("InstanceProgressBar")
				};


				// Устанавливаем разметку для формы
				grid.ColumnDefinitions.Add(columnDefinition1);
				grid.ColumnDefinitions.Add(columnDefinition2);
				grid.ColumnDefinitions.Add(columnDefinition3);
				grid.RowDefinitions.Add(rowDefinition);
				overviewGrid.RowDefinitions.Add(overviewRowDefinition1);
				overviewGrid.RowDefinitions.Add(overviewRowDefinition2);
				titleGrid.ColumnDefinitions.Add(columnDefinitionTitle1);
				titleGrid.ColumnDefinitions.Add(columnDefinitionTitle2);
				titleGrid.RowDefinitions.Add(rowDefinitionTitle1);
				textContentGrid.RowDefinitions.Add(textContentRowDefinition1);
				textContentGrid.RowDefinitions.Add(textContentRowDefinition2);
				instanceButtonGrid.RowDefinitions.Add(instanceButtonRowDefinition1);
				instanceButtonGrid.RowDefinitions.Add(instanceButtonRowDefinition2);

				// Добавление в Столбики и Колноки в форме.
				Grid.SetRow(canvas, row);
				Grid.SetColumn(moreButton, 0);
				Grid.SetColumn(textContentGrid, 1);
				Grid.SetRow(titleGrid, 0);
				Grid.SetColumn(textBlockAuthor, 1);
				Grid.SetRow(overviewGrid, 1);
				Grid.SetColumn(textBlockTitle, 0);
				Grid.SetRow(tagsWrapPanel, 1);
				Grid.SetColumn(textBlockOverview, 0);
				Grid.SetRow(textBlockOverview, 0);
				Grid.SetColumn(instanceButtonGrid, 2);
				Grid.SetRow(instanceLaunchButton, 0);
				Grid.SetRow(addToLibrary, 1);
				Grid.SetColumn(progressBar, 1);
				// Добавление объектов в форму.
				// Добавление дочерних элементов
				instanceButtonGrid.Children.Add(addToLibrary);
				instanceButtonGrid.Children.Add(instanceLaunchButton);
				overviewGrid.Children.Add(tagsWrapPanel);
				overviewGrid.Children.Add(textBlockOverview);
				textContentGrid.Children.Add(overviewGrid);
				titleGrid.Children.Add(textBlockTitle);
				titleGrid.Children.Add(textBlockAuthor);
				textContentGrid.Children.Add(titleGrid);
				grid.Children.Add(instanceButtonGrid);
				grid.Children.Add(textContentGrid);
				grid.Children.Add(moreButton);
				grid.Children.Add(progressBar);
				canvas.Children.Add(grid);
				InstanceGrid.Children.Add(canvas);

				// TODO: Удачить на релизе.
				instanceButtonGrid.ShowGridLines = false;
				overviewGrid.ShowGridLines = false;
				titleGrid.ShowGridLines = false;
				textContentGrid.ShowGridLines = false;
				grid.ShowGridLines = false;
			});
		}

		private Button GetTagsButton(string content)
        {
			var tag = new Button() 
			{ 
				Name = "tag" + content.Replace('.', '_'),
				Content = content,
				Style = (Style)Application.Current.FindResource("TagStyle"),
			};
			tag.Click += TagButtonClick;
			return tag;
		}

		private RowDefinition GetRowDefinition()
        {
			RowDefinition rowDefinition = new RowDefinition() 
			{
				Height = new GridLength(150, GridUnitType.Pixel)
			};
			return rowDefinition;
		}

		private void TagButtonClick(object sender, RoutedEventArgs e) 
		{
			string tagName = ((Button)sender).Name;
			MessageBox.Show(tagName);
		}

		private void LaunchInstance(object sender, RoutedEventArgs e) 
		{
			if (!LaunchButtonBlock)
			{
				string instanceId = ((Button)sender).Name.Replace("Download", "");

				//проиводим действие только если произошел клик по запущенному модпаку, или никакой модпак не запущен
				if (instanceId == LaunchGame.runnigInstance || LaunchGame.runnigInstance == "")
				{
					LaunchButtonBlock = true;
					ManageLogic.СlientManager(instanceId);
				}
			}	
		}

		private void CurrentDownloadInstance(object sender, RoutedEventArgs e) 
		{
			if (_instancesData[sender].CurseforgeInstanceId != 0) {
				MessageBox.Show(_instancesData[sender].CurseforgeInstanceId.ToString());
				//string instanceId = ManageLogic.CreateInstance(
				//	_instancesData[sender].InstanceTitle, InstanceType.Curseforge, "", "", _instancesData[sender].CurseforgeInstanceId
				//	);
				//ManageLogic.DownloadInstance(instanceId, InstanceType.Curseforge);
			}
		}

		private void ExportInstance(object sender, RoutedEventArgs e)
		{

		}

		private void ClickedMoreButton(object sender, RoutedEventArgs e, string title, string description, string author, List<string> tags)
        {
            string instanceName = ((Button)sender).Name;
			var lsmp = LeftSideMenuPage.instance;
			string[] ButtonContents = new string[4] { title, "Экспорт", "Настройки", "Вернуться" };
			RoutedEventHandler[] ButtonClicks = new RoutedEventHandler[4] { lsmp.InstanceOverview, lsmp.InstanceExport, lsmp.InstanceSetting, lsmp.BackToMainMenu };
			
			for (int i = 0; i < 4; i++)
            {
				SwitchToggleButton(lsmp.LeftSideMenu, ButtonContents[i], ButtonClicks[i], i);
            }
			_mainWindow.instanceTitle = title;
			_mainWindow.instanceDescription = description;
			_mainWindow.instanceAuthor = author;
			_mainWindow.instanceTags = tags;

			InstancePage instancePage = new InstancePage(_mainWindow);
			_mainWindow.RightSideFrame.Navigate(instancePage);
			instancePage.BottomSideFrame.Navigate(new OverviewPage(_mainWindow));
		}

		private ToggleButton SwitchToggleButton(StackPanel pageInstance, string content, RoutedEventHandler routedEventHandler, int index)
		{
			ToggleButton toggleButton = (ToggleButton)pageInstance.FindName("LeftSideMenuButton" + index);

			toggleButton.Content = content;
			toggleButton.Style = (Style)Application.Current.FindResource("MWCBS1");
			toggleButton.Click += routedEventHandler;

			if (index == 0) toggleButton.IsChecked = true;

			return toggleButton;
		}
	}
}