using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.Pages.Right.Menu
{
    /// <summary>
    /// Interaction logic for TestModpacksContainerPage.xaml
    /// </summary>
    public partial class ModpacksContainerPage : Page
    {
		private Uri logo_path = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
		private List<String> instance_tags = new List<String>() {"1.10.2", "Mods"};
		public ModpacksContainerPage()
        {
            InitializeComponent();
			BuildInstanceForm("EOF", 0, logo_path, "Energy of Space", "by NightWorld", "Our offical testing launcher modpack...", instance_tags);
        }


        private void BuildInstanceForm(string instance_name, int row, Uri logo_path, string title, string author, string overview, List<String> tags) 
        {
			var canvas = new Canvas();
			canvas.Height = 120;
			canvas.Width = 600;
			canvas.Margin = new Thickness(40, 0, 0, 0);
			canvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#151719"));
			canvas.Name = instance_name;

			// Добавление в Столбики и Колноки в форме.
            Grid.SetRow(canvas, row);

			var grid = new Grid();
			grid.ShowGridLines = false;
			// Делаем разметку для элементов.
			ColumnDefinition columnDefinition1 = new ColumnDefinition();
			ColumnDefinition columnDefinition2 = new ColumnDefinition();
			ColumnDefinition columnDefinition3 = new ColumnDefinition();
			RowDefinition rowDefinition = new RowDefinition();

			// Устанавливаем параметры (Длина, Ширина)
			columnDefinition1.Width = new GridLength(120, GridUnitType.Pixel);
			columnDefinition2.Width = new GridLength(420, GridUnitType.Pixel);
			columnDefinition3.Width = new GridLength(60, GridUnitType.Pixel);
			rowDefinition.Height = new GridLength(120, GridUnitType.Pixel);

			// Устанавливаем разметку для формы
			grid.ColumnDefinitions.Add(columnDefinition1);
			grid.ColumnDefinitions.Add(columnDefinition2);
			grid.ColumnDefinitions.Add(columnDefinition3);
			grid.RowDefinitions.Add(rowDefinition);

			// Instance Logo
			var image = new Border();
            image.Background = new ImageBrush(new BitmapImage(logo_path));
			Grid.SetColumn(image, 0);

			// Title
			var textContentGrid = new Grid();
			textContentGrid.ShowGridLines = false;
			RowDefinition textContentRowDefinition1 = new RowDefinition();
			RowDefinition textContentRowDefinition2 = new RowDefinition();

			textContentRowDefinition1.Height = new GridLength(30, GridUnitType.Pixel);
			textContentRowDefinition2.Height = new GridLength(1, GridUnitType.Star);

			textContentGrid.RowDefinitions.Add(textContentRowDefinition1);
			textContentGrid.RowDefinitions.Add(textContentRowDefinition2);

			Grid.SetColumn(textContentGrid, 1);

			// Разметка для заголовков
			var titleGrid = new Grid();
			titleGrid.ShowGridLines = false;
			ColumnDefinition columnDefinitionTitle1 = new ColumnDefinition();
			ColumnDefinition columnDefinitionTitle2 = new ColumnDefinition();
			RowDefinition rowDefinitionTitle1 = new RowDefinition();

			columnDefinitionTitle1.Width = new GridLength(1, GridUnitType.Star);
			columnDefinitionTitle2.Width = new GridLength(80, GridUnitType.Pixel);
			rowDefinitionTitle1.Height = new GridLength(30, GridUnitType.Pixel);

			titleGrid.ColumnDefinitions.Add(columnDefinitionTitle1);
			titleGrid.ColumnDefinitions.Add(columnDefinitionTitle2);
			titleGrid.RowDefinitions.Add(rowDefinitionTitle1);

			Grid.SetRow(titleGrid, 0);

			// Заголовок - Название
			var textBlockTitle = new TextBlock();
			textBlockTitle.Text = title;
			textBlockTitle.Padding = new Thickness(0);
			textBlockTitle.FontSize = 22;
			textBlockTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));

			Grid.SetColumn(textBlockTitle, 0);

			// Об Авторе
			var textBlockAuthor = new TextBlock();
			textBlockAuthor.Text = author;
			textBlockAuthor.Padding = new Thickness(0);
			textBlockAuthor.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
			textBlockAuthor.VerticalAlignment = VerticalAlignment.Center;

			Grid.SetColumn(textBlockAuthor, 1);


			// Описание - контент
			var overviewGrid = new Grid();
			overviewGrid.ShowGridLines = true;
			RowDefinition overviewRowDefinition1 = new RowDefinition();
			RowDefinition overviewRowDefinition2 = new RowDefinition();
			overviewRowDefinition1.Height = new GridLength(35, GridUnitType.Pixel);
			overviewRowDefinition2.Height = new GridLength(1, GridUnitType.Star);

			overviewGrid.RowDefinitions.Add(overviewRowDefinition1);
			overviewGrid.RowDefinitions.Add(overviewRowDefinition2);

			Grid.SetRow(overviewGrid, 1);
			
			var textBlockOverview = new TextBlock();
			textBlockOverview.Text = overview;
			textBlockOverview.Padding = new Thickness(0);
			textBlockOverview.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
			textBlockOverview.HorizontalAlignment = HorizontalAlignment.Left;
			textBlockOverview.FontSize = 16;

			Grid.SetColumn(textBlockOverview, 0);
			Grid.SetRow(textBlockOverview, 0);

			var tagsWrapPanel = new WrapPanel();
			tagsWrapPanel.Orientation = Orientation.Horizontal;
			tagsWrapPanel.VerticalAlignment = VerticalAlignment.Center;
			tagsWrapPanel.Children.Add(GetTagsButton(tags[0]));
			tagsWrapPanel.Children.Add(GetTagsButton(tags[1]));

			Grid.SetRow(tagsWrapPanel, 1);

			var instanceButtonGrid = new Grid();
			RowDefinition instanceButtonRowDefinition1 = new RowDefinition();
			RowDefinition instanceButtonRowDefinition2 = new RowDefinition();

			instanceButtonRowDefinition1.Height = new GridLength(50, GridUnitType.Pixel);
			instanceButtonRowDefinition2.Height = new GridLength(50, GridUnitType.Pixel);

			instanceButtonGrid.RowDefinitions.Add(instanceButtonRowDefinition1);
			instanceButtonGrid.RowDefinitions.Add(instanceButtonRowDefinition2);

			Grid.SetColumn(instanceButtonGrid, 2);

			var downloadButton = new Button();
			downloadButton.Click += DownloadInstance;
			downloadButton.Style = (Style)Application.Current.FindResource("InstanceBlockAction");

			var exportButton = new Button();
			exportButton.Click += ExportInstance;
			exportButton.Style = (Style)Application.Current.FindResource("InstanceBlockAction");

			Grid.SetRow(downloadButton, 0);
			Grid.SetRow(exportButton, 1);

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
			grid.Children.Add(image);
			canvas.Children.Add(grid);
			InstanceGrid.Children.Add(canvas);
		}

		private Button GetTagsButton(string content)
        {
			var tag = new Button();
			tag.Name = "tag" + content.Replace('.', '_');
			tag.Content = content;
			tag.Style = (Style)Application.Current.FindResource("TagStyle");
			tag.Click += TagButtonClick;
			return tag;
		}

		private void TagButtonClick(object sender, RoutedEventArgs e) 
		{

		}

		private void DownloadInstance(object sender, RoutedEventArgs e) 
		{ 
		
		}

		private void ExportInstance(object sender, RoutedEventArgs e)
		{

		}
	}
}