using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
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
	/// 
	

	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
	// TODO: Сделать общую страницу контейнер для Library и Catalog.
    public partial class InstanceContainerPage : Page
	{
		public static InstanceContainerPage obj = null;

		private MainWindow _mainWindow;
		public bool _isInitializeInstance = false;
		public SearchBox searchBox;
		private Paginator paginator;
		private int pageSize = 10;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();

			obj = this;
			_mainWindow = mainWindow;

			InitializeControlElements();
			GetInitializeInstance(InstanceSource.Curseforge);
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

		private void InitializeInstance(InstanceSource instanceSource, int pageIndex=0, string searchBoxText="")
		{
			var instances = ManageLogic.GetOutsideInstances(
				instanceSource, pageSize, pageIndex, ModpacksCategories.All, searchBoxText
			);
			
			this.Dispatcher.Invoke(() => { 
				if (instances.Count < 10) paginator.Visibility = Visibility.Hidden;
				else paginator.Visibility = Visibility.Visible;
			});

			if (instances.Count == 0) ChangeLoadingLabel("Результаты не найдены.", Visibility.Visible);
			else {
				for (int j = 0; j < instances.ToArray().Length; j++)
				{
					// TODO: размер curseforgeInstances[j].attachments или curseforgeInstances[j].authors может быть равен нулю и тогда будет исключение
					// TODO: в curseforgeInstances[j].attachments нужно брать не первый элемент, а тот у котрого isDefault стоит на true
					BuildInstanceForm(instances[j].Id.ToString(), j,
						new Uri(instances[j].MainImageUrl),
						instances[j].Name,
						instances[j].Author,
						instances[j].Description,
						instances[j].Categories);
					ChangeLoadingLabel("", Visibility.Collapsed);
				}
			}
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(string instanceId, int row, Uri logoPath, string title, string author, string overview, List<string> tags)
		{
			this.Dispatcher.Invoke(() =>
			{
				if (InstanceGrid.RowDefinitions.Count < 10)
				{ InstanceGrid.RowDefinitions.Add(GetRowDefinition()); }
				UserControls.InstanceForm instanceForm = new UserControls.InstanceForm(
					_mainWindow, title, "", author, overview, instanceId, logoPath, tags, false, false
				);

				Grid.SetRow(instanceForm, row);
				InstanceGrid.Children.Add(instanceForm);
			});
		}

		private RowDefinition GetRowDefinition(int height=150)
		{
			RowDefinition rowDefinition = new RowDefinition()
			{
				Height = new GridLength(height, GridUnitType.Pixel)
			};
			return rowDefinition;
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

			ClearGrid();

			ChangeLoadingLabel("Идёт загрузка. Пожалуйста подождите...", Visibility.Visible);

			Lexplosion.Run.ThreadRun(delegate ()
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
					GetInitializeInstance(selectedInstanceSource); ;
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
	}
}
