using Lexplosion.Global;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.Pages.Right.Menu
{
    /// <summary>
    /// Interaction logic for TestModpacksContainerPage.xaml
    /// </summary>
    public partial class ModpacksContainerPage : Page
    {
		public static ModpacksContainerPage obj = null;
		public bool LaunchButtonBlock = false; //блокировщик кнопки запуска модпака

		public ModpacksContainerPage()
        {
            InitializeComponent();

			obj = this;
			var i = 0;

			foreach(string instanceId in UserData.InstancesList.Keys)
            {
				List<string> instance_tags1 = new List<string>() { "1.10.2", "Mods", "NightWorld" };

				Uri logoPath = null;
				string desc = "";

				if (UserData.instancesAssets.ContainsKey(instanceId))
                {
					string dir = UserData.settings["gamePath"] + "/launcherAssets/" + UserData.instancesAssets[instanceId].mainImage;

					if (File.Exists(dir))
					{
						logoPath = new Uri(dir);
					}
					else
					{
						logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
					}

					if (UserData.instancesAssets[instanceId].description.Length > 36)
					{
						desc = UserData.instancesAssets[instanceId].description.Substring(0, 36).Trim() + "...";
					}
					else
					{
						desc = UserData.instancesAssets[instanceId].description;
					}

				}
				else
				{
					logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
				}

                try
                {
					BuildInstanceForm(instanceId, i, logoPath, UserData.InstancesList[instanceId], "NightWorld", desc, instance_tags1);
					i++;
				}
                catch 
				{ 

				}
			}
		}


		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(string instance_name, int row, Uri logo_path, string title, string author, string overview, List<string> tags)
		{
			/// "EOF", 0, logo_path1, "Energy of Space", "NightWorld", "Our offical testing launcher modpack...", instance_tags1
			// Добавляем строчку размером 150 px для нашего блока со сборкой.
			InstanceGrid.RowDefinitions.Add(GetRowDefinition());

			var canvas = new Canvas()
			{
				Height = 120,
				Width = 600,
				Margin = new Thickness(40, 0, 0, 0),
				Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#151719")),
				Name = instance_name,
				Effect = new DropShadowEffect() {
					ShadowDepth = 0,
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
				Width = new GridLength(420, GridUnitType.Pixel)
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
				Name = instance_name + "More",
				Background = new ImageBrush(new BitmapImage(logo_path)),
				Style = (Style)Application.Current.FindResource("InstanceMoreButton")
			};
			moreButton.Click += ClickedMoreButton;

			// Titlew
			var textContentGrid = new Grid() 
			{
				Margin = new Thickness(10, 0, 10, 0)
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
				Width = new GridLength(80, GridUnitType.Pixel)
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
				Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"))
			};

			// Об Авторе
			var textBlockAuthor = new TextBlock() 
			{
				Text = "by " + author,
				Padding = new Thickness(0),
				Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a9b1ba")),
				VerticalAlignment = VerticalAlignment.Center,
				FontFamily = new FontFamily(new Uri("pack://application:,,,/assets/fonts/"), "./#Casper Bold")
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
				Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")),
				HorizontalAlignment = HorizontalAlignment.Left,
				FontSize = 16
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

			var downloadButton = new Button() 
			{ 
				Name = instance_name + "Download",
				BorderThickness = new Thickness(2),
				Style = (Style)Application.Current.FindResource("InstanceDonwloadButton")
			};
			downloadButton.Click += LaunchInstance;

			var exportButton = new Button() 
			{
				Name = instance_name + "Export",
				BorderThickness = new Thickness(2),
				Style = (Style)Application.Current.FindResource("InstanceExportButton")
			};
			exportButton.Click += ExportInstance;

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
			Grid.SetRow(downloadButton, 0);
			Grid.SetRow(exportButton, 1);
			Grid.SetColumn(progressBar, 1);
			// Добавление объектов в форму.
			// Добавление дочерних элементов
			instanceButtonGrid.Children.Add(exportButton);
			instanceButtonGrid.Children.Add(downloadButton);
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
					ManageLogic.СlientManager(instanceId, Logic.InstanceType.Nightworld);
				}
			}	
		}

		private void ExportInstance(object sender, RoutedEventArgs e)
		{

		}

		private void ClickedMoreButton(object sender, RoutedEventArgs e)
        {
            string instanceId = ((Button)sender).Name;
			var lsmp = LeftSideMenuPage.instance;

			string[] ButtonContents = new string[4] { "Energy of Space", "Экспорт", "Настройки", "Назад" };
			RoutedEventHandler[] ButtonClicks = new RoutedEventHandler[4] { lsmp.InstanceOverview, lsmp.InstanceExport, lsmp.InstanceSetting, lsmp.BackToMainMenu };
			
			for (int i = 0; i < 4; i++)
            {
				SwitchToggleButton(lsmp.LeftSideMenu, ButtonContents[i], ButtonClicks[i], i);
            }
            GetRightSideFrame().Source = GuiUris.InstancePage;
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

		public static Frame GetRightSideFrame() => MainWindow.instance.RightSideFrame;
	}
}