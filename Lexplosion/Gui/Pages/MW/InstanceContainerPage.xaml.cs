using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using System;
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
		public static InstanceContainerPage Obj = null;

		private MainWindow _mainWindow;
		private Paginator _paginator;
		private int _pageSize = 10;

		public bool _isInitializeInstance = false;
		public SearchBox SearchBox;
		private InstanceLoadingForm _instanceLoadingForm;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();

			Obj = this;
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
			SearchBox = new SearchBox(this)
			{
				Margin = new Thickness(0, 14, 0, 0),
				Width = 400,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			_paginator = new Paginator(this);
			_paginator.Visibility = Visibility.Hidden;

			Grid.SetRow(SearchBox, 0);
			Grid.SetRow(_paginator, 2);

			ControlElementsGrid.Children.Add(SearchBox);
			ControlElementsGrid.Children.Add(_paginator);
		}

		public async void GetInitializeInstance(InstanceSource instanceSource)
		{
			await Task.Run(() => InitializeInstance(instanceSource));
			_isInitializeInstance = true;
		}

		private void InitializeInstance(InstanceSource instanceSource, int pageIndex = 0, string searchBoxText = "")
		{
			LoadingForm();
			Lexplosion.Run.TaskRun(delegate () {
				var instances = OutsideDataManager.GetInstances(
					instanceSource, _pageSize, pageIndex, ModpacksCategories.All, searchBoxText
				);

				RemoveInstanceGridContent();
				if (instances.Count == 0) ChangeLoadingLabel("Результаты не найдены.", Visibility.Visible);
				else
				{
					for (int j = 0; j < instances.ToArray().Length; j++)
					{
						// TODO: размер curseforgeInstances[j].attachments или curseforgeInstances[j].authors может быть равен нулю и тогда будет исключение
						// TODO: в curseforgeInstances[j].attachments нужно брать не первый элемент, а тот у котрого isDefault стоит на true
						BuildInstanceForm(instances[j], j);
						ChangeLoadingLabel();
					}
				}
				_paginator.ChangePaginatorVisibility(instances.Count, _pageSize);
			});
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(OutsideInstance outsideInstance, int row)
		{
			
			this.Dispatcher.Invoke(() =>
			{
				if (InstanceGrid.RowDefinitions.Count < 10)
					InstanceGrid.RowDefinitions.Add(GetRowDefinition());
				InstanceForm instanceForm = new UserControls.InstanceForm(
					_mainWindow, outsideInstance.Name, outsideInstance.LocalId, outsideInstance.InstanceAssets.author,
					outsideInstance.InstanceAssets.description, outsideInstance.Id, ToImage(outsideInstance.MainImage),
					outsideInstance.Categories, outsideInstance.IsInstalled, false
				);
				Grid.SetRow(instanceForm, row);
				Console.WriteLine(outsideInstance.LocalId + " " + outsideInstance.Id);
				InstanceGrid.Children.Add(instanceForm);
			});
		}

		public void ChangePage()
		{
			var selectedInstanceSource = (InstanceSource)SearchBox.SourceBox.SelectedIndex;
			var searchBoxText = SearchBox.SearchTextBox.Text;

			ClearGrid();
			// TODO: Добавить анимация для скрола.
			ContainerPage_ScrollViewer.ScrollToVerticalOffset(0.0);

			InitializeInstance(selectedInstanceSource, _paginator.PageIndex, searchBoxText);
		}

		public void SearchInstances()
		{
			var searchBoxTextLength = SearchBox.SearchTextBox.Text.Length;
			var sourceBoxSelectedIndex = SearchBox.SourceBox.SelectedIndex;
			var searchBoxText = SearchBox.SearchTextBox.Text;
			var loadingLableText = LoadingLabel.Text;
			var selectedInstanceSource = (InstanceSource)sourceBoxSelectedIndex;
			_paginator.PageIndex = 0;
			_paginator.ChangePaginatorVisibility(0, 1);
			ClearGrid();
			LoadingForm();

			Lexplosion.Run.TaskRun(delegate ()
			{
				if (searchBoxTextLength != 0 || sourceBoxSelectedIndex != SearchBox.LastSelectedIndex)
				{
					_isInitializeInstance = false;

					InitializeInstance(selectedInstanceSource, _paginator.PageIndex, searchBoxText);
					SearchBox.LastRequest = searchBoxText;
					SearchBox.LastSelectedIndex = sourceBoxSelectedIndex;
				}
				else
				{
					SearchBox.LastRequest = searchBoxText;
					GetInitializeInstance(selectedInstanceSource);
				}
			});
		}

		private void ChangeLoadingLabel(string content="", Visibility visibility=Visibility.Hidden)
		{
			this.Dispatcher.Invoke(() => {
				LoadingLabel.Text = content;
				LoadingLabel.Visibility = visibility;
			});
		}

		private void ClearGrid()
		{
			RemoveInstanceGridContent();
			RemoveInstanceGridRowDefinitions();
		}

		private void RemoveInstanceGridContent()
		{
			this.Dispatcher.Invoke(() => {
				if (InstanceGrid.Children.Count > 2) InstanceGrid.Children.RemoveRange(1, 10);
			});
		}

		private void RemoveInstanceGridRowDefinitions()
		{
			this.Dispatcher.Invoke(() => {
				if (InstanceGrid.RowDefinitions.Count > 2)
					InstanceGrid.RowDefinitions.RemoveRange(0, InstanceGrid.RowDefinitions.Count - 1);
			});
		}

		private RowDefinition GetRowDefinition(int height = 150)
		{
			RowDefinition rowDefinition = new RowDefinition()
			{
				Height = new GridLength(height, GridUnitType.Pixel)
			};
			return rowDefinition;
		}

		private void LoadingForm()
		{
			this.Dispatcher.Invoke(delegate
			{
				for (int i = 0; i < 10; i++)
				{
					if (InstanceGrid.RowDefinitions.Count < 10)
						InstanceGrid.RowDefinitions.Add(GetRowDefinition());
					_instanceLoadingForm = new InstanceLoadingForm();
					Grid.SetRow(_instanceLoadingForm, i);
					InstanceGrid.Children.Add(_instanceLoadingForm);
				}
			});
		}
	}
}
