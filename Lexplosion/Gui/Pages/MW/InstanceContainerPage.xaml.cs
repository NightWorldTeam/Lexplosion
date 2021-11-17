using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.Pages.MW
{
    /// <summary>
    /// Interaction logic for InstanceContainerPage.xaml
    /// </summary>
    /// 


    // TODO: Сделать общую страницу контейнер для Library и Catalog.
    // TODO: Сделать общую страницу контейнер для Library и Catalog.
    public partial class InstanceContainerPage : Page
	{
		public static InstanceContainerPage obj = null;

		private MainWindow _mainWindow;
		private Paginator paginator;
		private int pageSize = 10;

		public bool _isInitializeInstance = false;
		public SearchBox searchBox;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();

			obj = this;
			_mainWindow = mainWindow;

			InitializeControlElements();
			GetInitializeInstance(InstanceSource.Curseforge);
		}

		private BitmapImage ToImage(byte[] array)
		{
			if (array is null)
				return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png"));
			BitmapImage image = new BitmapImage();
			image.BeginInit();
			image.StreamSource = new System.IO.MemoryStream(array);
			image.EndInit();
			return image;
		}

		private void InitializeControlElements()
		{
			searchBox = new SearchBox(this)
			{
				Margin = new Thickness(0, 14, 0, 0),
				Width = 400,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			paginator = new Paginator(this);
			paginator.Visibility = Visibility.Hidden;

			Grid.SetRow(searchBox, 0);
			Grid.SetRow(paginator, 2);

			ControlElementsGrid.Children.Add(searchBox);
			ControlElementsGrid.Children.Add(paginator);
		}

		public async void GetInitializeInstance(InstanceSource instanceSource)
		{
			await Task.Run(() => InitializeInstance(instanceSource));
			_isInitializeInstance = true;
			ChangeLoadingLabel("", Visibility.Collapsed);
		}

		private void InitializeInstance(InstanceSource instanceSource, int pageIndex = 0, string searchBoxText = "")
		{
			Console.WriteLine("Номер страницы - " + pageIndex.ToString());
			var instances = OutsideDataManager.GetInstances(
				instanceSource, pageSize, pageIndex, ModpacksCategories.All, searchBoxText
			);
			Console.WriteLine("Количество модпаков - " + instances.Count.ToString());
			paginator.ChangePaginatorVisibility(instances.Count, pageSize);

			if (instances.Count == 0) ChangeLoadingLabel("Результаты не найдены.", Visibility.Visible);
			else
			{
				for (int j = 0; j < instances.ToArray().Length; j++)
				{
					// TODO: размер curseforgeInstances[j].attachments или curseforgeInstances[j].authors может быть равен нулю и тогда будет исключение
					// TODO: в curseforgeInstances[j].attachments нужно брать не первый элемент, а тот у котрого isDefault стоит на true
					BuildInstanceForm(instances[j], j);
					ChangeLoadingLabel("", Visibility.Collapsed);
				}
			}
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(OutsideInstance outsideInstance, int row)
		{

			this.Dispatcher.Invoke(() =>
			{
				if (InstanceGrid.RowDefinitions.Count < 10)
					InstanceGrid.RowDefinitions.Add(GetRowDefinition());
				UserControls.InstanceForm instanceForm = new UserControls.InstanceForm(
					_mainWindow, outsideInstance.Name, outsideInstance.LocalId, outsideInstance.Author, outsideInstance.Description,
					outsideInstance.Id, ToImage(outsideInstance.MainImage), outsideInstance.Categories, outsideInstance.IsInstalled, false);

				Grid.SetRow(instanceForm, row);
				InstanceGrid.Children.Add(instanceForm);
			});
		}

		public void ChangePage()
		{
			var selectedInstanceSource = (InstanceSource)searchBox.SourceBox.SelectedIndex;
			var searchBoxText = searchBox.SearchTextBox.Text;

			ClearGrid();
			// TODO: Добавить анимация для скрола.
			ContainerPage_ScrollViewer.ScrollToVerticalOffset(0.0);
			InitializeInstance(selectedInstanceSource, paginator.PageIndex, searchBoxText);
		}

		public void SearchInstances()
		{
			var searchBoxTextLength = searchBox.SearchTextBox.Text.Length;
			var sourceBoxSelectedIndex = searchBox.SourceBox.SelectedIndex;
			var searchBoxText = searchBox.SearchTextBox.Text;
			var loadingLableText = LoadingLabel.Text;
			var selectedInstanceSource = (InstanceSource)sourceBoxSelectedIndex;
			paginator.PageIndex = 0;
			ClearGrid();

			ChangeLoadingLabel("Идёт загрузка. Пожалуйста подождите...", Visibility.Visible);

			Lexplosion.Run.TaskRun(delegate ()
			{
				if (searchBoxTextLength != 0 || sourceBoxSelectedIndex != searchBox.LastSelectedIndex)
				{
					_isInitializeInstance = false;
					InitializeInstance(selectedInstanceSource, paginator.PageIndex, searchBoxText);
					searchBox.LastRequest = searchBoxText;
					searchBox.LastSelectedIndex = sourceBoxSelectedIndex;
				}
				else
				{
					searchBox.LastRequest = searchBoxText;
					GetInitializeInstance(selectedInstanceSource);
					ChangeLoadingLabel("Идёт загрузка. Пожалуйста подождите...", Visibility.Visible);
				}
			});
		}

		private void ChangeLoadingLabel(string content, Visibility visibility)
		{
			this.Dispatcher.Invoke(() => {
				LoadingLabel.Text = content;
				LoadingLabel.Visibility = visibility;
			});
		}

		private void ClearGrid()
		{
			if (InstanceGrid.Children.Count > 2) InstanceGrid.Children.RemoveRange(1, 10);
			if (InstanceGrid.RowDefinitions.Count > 2)
				InstanceGrid.RowDefinitions.RemoveRange(0, InstanceGrid.RowDefinitions.Count - 1);
		}

		private RowDefinition GetRowDefinition(int height = 150)
		{
			RowDefinition rowDefinition = new RowDefinition()
			{
				Height = new GridLength(height, GridUnitType.Pixel)
			};
			return rowDefinition;
		}
	}
}
